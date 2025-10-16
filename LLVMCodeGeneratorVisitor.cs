using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using LLVMSharp.Interop;

public class LLVMCodeGeneratorVisitor : RedLangBaseVisitor<LLVMValueRef>
{
    private readonly LLVMModuleRef module;
    private readonly LLVMBuilderRef builder;
    private readonly LLVMContextRef context;

    private readonly Dictionary<string, LLVMValueRef> namedValues = new();
    private readonly Dictionary<string, LLVMTypeRef> variableTypes = new();
    private readonly Dictionary<string, LLVMValueRef> functions = new();
    private readonly Dictionary<string, LLVMTypeRef> functionTypes = new();

    // Cache para cadenas de formato (reutilizarlas evita duplicados)
    private readonly Dictionary<string, LLVMValueRef> formatStrings = new();

    private LLVMValueRef currentFunction;
    private LLVMBasicBlockRef currentBlock;

    public LLVMCodeGeneratorVisitor(string moduleName = "RedLangModule")
    {
        context = LLVMContextRef.Global;
        module = context.CreateModuleWithName(moduleName);
        builder = context.CreateBuilder();
    }

    public string GetIR()
    {
        return module.PrintToString();
    }

    public void WriteIRToFile(string filename)
    {
        module.PrintToFile(filename);
    }

    private LLVMTypeRef GetLLVMType(string typeName)
    {
        return typeName switch
        {
            "i" => LLVMTypeRef.Int32,
            "f" => LLVMTypeRef.Double,
            "s" => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
            "b" => LLVMTypeRef.Int1,
            _ => LLVMTypeRef.Int32
        };
    }

    private LLVMValueRef GetDefaultValue(LLVMTypeRef type)
    {
        if (type.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            return LLVMValueRef.CreateConstInt(type, 0, false);
        else if (type.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            return LLVMValueRef.CreateConstReal(type, 0.0);
        else if (type.Kind == LLVMTypeKind.LLVMPointerTypeKind)
            return LLVMValueRef.CreateConstPointerNull(type);
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
    }

    // ----------------- Helpers para printf/show -----------------
    private LLVMValueRef EnsurePrintfDeclared()
    {
        // Si ya registramos printf (bajo la clave "show"), devolvemos esa referencia
        if (functions.ContainsKey("show"))
            return functions["show"];

        // Crear tipo de printf: int printf(i8* fmt, ...)
        var printfParamTypes = new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) };
        var printfType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, printfParamTypes, true);

        var printfFunc = module.AddFunction("printf", printfType);

        // Guardamos en los diccionarios pero bajo la clave "show" para que el resto del código
        // pueda usar functions["show"] cuando el usuario escriba show(...)
        functions["show"] = printfFunc;
        functionTypes["show"] = printfType;

        return printfFunc;
    }

    private LLVMValueRef GetOrCreateFormatString(string format)
    {
        if (formatStrings.ContainsKey(format))
            return formatStrings[format];

        // BuildGlobalStringPtr crea un global y devuelve i8* apuntando a él
        var gstr = builder.BuildGlobalStringPtr(format, "fmt");
        formatStrings[format] = gstr;
        return gstr;
    }
    // ------------------------------------------------------------

    public override LLVMValueRef VisitProgram([NotNull] RedLangParser.ProgramContext context)
    {
        // Declaramos printf (y lo registramos como "show" internamente)
        EnsurePrintfDeclared();

        // Registrar funciones definidas por el usuario
        for (int i = 0; i < context.ChildCount; i++)
        {
            if (context.GetChild(i) is RedLangParser.FunctionDeclContext funcCtx)
            {
                RegisterFunction(funcCtx);
            }
        }

        // Crear función main para código global
        var mainType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
        var mainFunc = module.AddFunction("main", mainType);
        functions["main"] = mainFunc;
        functionTypes["main"] = mainType; // Consistencia
        currentFunction = mainFunc;

        var entryBlock = mainFunc.AppendBasicBlock("entry");
        builder.PositionAtEnd(entryBlock);
        currentBlock = entryBlock;

        // Procesar todo el código global y generar funciones
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);

            if (child is RedLangParser.FunctionDeclContext)
            {
                // Guardar contexto de main
                var savedFunc = currentFunction;
                var savedBlock = currentBlock;
                var savedValues = new Dictionary<string, LLVMValueRef>(namedValues);
                var savedTypes = new Dictionary<string, LLVMTypeRef>(variableTypes);

                // Generar función
                Visit(child);

                // Restaurar contexto de main
                currentFunction = savedFunc;
                currentBlock = savedBlock;
                builder.PositionAtEnd(savedBlock);
                namedValues.Clear();
                foreach (var kv in savedValues)
                    namedValues[kv.Key] = kv.Value;
                variableTypes.Clear();
                foreach (var kv in savedTypes)
                    variableTypes[kv.Key] = kv.Value;
            }
            else if (child is RedLangParser.DeclarationContext || child is RedLangParser.StatementContext)
            {
                // Código global - ejecutar en main
                Visit(child);
            }
        }

        // Verificar si el bloque actual ya tiene un terminator
        if (currentBlock.Terminator.Handle == IntPtr.Zero)
        {
            // Si no hay terminator, agregar return 0
            builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false));
        }

        return default;
    }

    private void RegisterFunction(RedLangParser.FunctionDeclContext context)
    {
        string functionName = context.IDENT().GetText();
        var returnType = GetLLVMType(context.type().GetText());

        var parameters = context.parameters()?.param() ?? Array.Empty<RedLangParser.ParamContext>();
        var paramTypes = new LLVMTypeRef[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            paramTypes[i] = GetLLVMType(parameters[i].type().GetText());
        }

        var functionType = LLVMTypeRef.CreateFunction(returnType, paramTypes);
        functions[functionName] = module.AddFunction(functionName, functionType);
        functionTypes[functionName] = functionType;
    }

    public override LLVMValueRef VisitFunctionDecl([NotNull] RedLangParser.FunctionDeclContext context)
    {
        string functionName = context.IDENT().GetText();
        var function = functions[functionName];
        currentFunction = function;

        var entryBlock = function.AppendBasicBlock("entry");
        builder.PositionAtEnd(entryBlock);
        currentBlock = entryBlock;

        var savedValues = new Dictionary<string, LLVMValueRef>(namedValues);
        var savedTypes = new Dictionary<string, LLVMTypeRef>(variableTypes);
        namedValues.Clear();
        variableTypes.Clear();

        var parameters = context.parameters()?.param() ?? Array.Empty<RedLangParser.ParamContext>();
        for (uint i = 0; i < parameters.Length; i++)
        {
            string paramName = parameters[i].IDENT().GetText();
            var paramType = GetLLVMType(parameters[i].type().GetText());
            var param = function.GetParam(i);
            param.Name = paramName;

            var alloca = builder.BuildAlloca(paramType, paramName);
            builder.BuildStore(param, alloca);
            namedValues[paramName] = alloca;
            variableTypes[paramName] = paramType;
        }

        Visit(context.block());

       if (currentBlock.Terminator.Handle == IntPtr.Zero &&
        function.TypeOf.ReturnType.Kind != LLVMTypeKind.LLVMVoidTypeKind)
        {
            if (currentBlock.FirstInstruction.Handle != IntPtr.Zero)
                {
                    builder.BuildRet(GetDefaultValue(function.TypeOf.ReturnType));
                }
        }


        namedValues.Clear();
        foreach (var kv in savedValues)
            namedValues[kv.Key] = kv.Value;
        variableTypes.Clear();
        foreach (var kv in savedTypes)
            variableTypes[kv.Key] = kv.Value;

        return function;
    }

    public override LLVMValueRef VisitDeclaration([NotNull] RedLangParser.DeclarationContext context)
    {
        string varName = context.IDENT().GetText();
        var varType = GetLLVMType(context.type().GetText());

        Console.WriteLine($"  [DEBUG] Declarando variable '{varName}' tipo '{context.type().GetText()}'");

        var alloca = builder.BuildAlloca(varType, varName);
        namedValues[varName] = alloca;
        variableTypes[varName] = varType;

        if (context.expression() != null)
        {
            Console.WriteLine($"  [DEBUG] Evaluando expresión: '{context.expression().GetText()}'");
            var value = Visit(context.expression());
            Console.WriteLine($"  [DEBUG] Resultado: {value}");
            builder.BuildStore(value, alloca);
        }
        else
        {
            builder.BuildStore(GetDefaultValue(varType), alloca);
        }

        return alloca;
    }

    public override LLVMValueRef VisitAssignment([NotNull] RedLangParser.AssignmentContext context)
    {
        string varName = context.IDENT().GetText();
        var value = Visit(context.expression());

        if (namedValues.ContainsKey(varName))
            builder.BuildStore(value, namedValues[varName]);

        return value;
    }

    public override LLVMValueRef VisitLiteral([NotNull] RedLangParser.LiteralContext context)
    {
        if (context.INT_LIT() != null)
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)int.Parse(context.INT_LIT().GetText()), true);
        if (context.FLOAT_LIT() != null)
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, double.Parse(context.FLOAT_LIT().GetText()));
        if (context.STRING_LIT() != null)
            return builder.BuildGlobalStringPtr(context.STRING_LIT().GetText().Trim('"'), "str");
        if (context.TRUE() != null)
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1, false);
        if (context.FALSE() != null)
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0, false);
        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
    }

    public override LLVMValueRef VisitPrimary([NotNull] RedLangParser.PrimaryContext context)
    {
        if (context.IDENT() != null)
        {
            string name = context.IDENT().GetText();
            if (namedValues.ContainsKey(name))
                return builder.BuildLoad2(variableTypes[name], namedValues[name], name);
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }
        return base.VisitPrimary(context);
    }

    public override LLVMValueRef VisitUnary([NotNull] RedLangParser.UnaryContext context)
    {
        if (context.primary() == null)
        {
            Console.WriteLine("[ERROR] VisitUnary: primary() es null");
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }

        var value = Visit(context.primary());

        if (value.Handle == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] VisitUnary: Visit(primary) devolvió un valor nulo");
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }

        if (context.MINUS() != null)
            return value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                ? builder.BuildNeg(value, "negtmp")
                : builder.BuildFNeg(value, "fnegtmp");
        else if (context.NOT() != null)
            return builder.BuildNot(value, "nottmp");
        return value;
    }

    public override LLVMValueRef VisitFactor([NotNull] RedLangParser.FactorContext context)
    {
        if (context.unary(0) == null)
        {
            Console.WriteLine("[ERROR] VisitFactor: unary(0) es null");
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }

        var result = Visit(context.unary(0));

        if (result.Handle == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] VisitFactor: Visit(unary) devolvió un valor nulo");
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == "*" || op == "/" || op == "%")
            {
                var right = Visit(context.unary(i / 2 + 1));
                bool isInt = result.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind;

                result = op switch
                {
                    "*" => isInt ? builder.BuildMul(result, right, "multmp") : builder.BuildFMul(result, right, "fmultmp"),
                    "/" => isInt ? builder.BuildSDiv(result, right, "divtmp") : builder.BuildFDiv(result, right, "fdivtmp"),
                    "%" => isInt ? builder.BuildSRem(result, right, "modtmp") : builder.BuildFRem(result, right, "fmodtmp"),
                    _ => result
                };
            }
        }
        return result;
    }

    public override LLVMValueRef VisitTerm([NotNull] RedLangParser.TermContext context)
    {
        var result = Visit(context.factor(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();

            if (op == "+" || op == "-")
            {
                var right = Visit(context.factor(i / 2 + 1));
                bool isInt = result.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind;

                result = op == "+"
                    ? (isInt ? builder.BuildAdd(result, right, "addtmp") : builder.BuildFAdd(result, right, "faddtmp"))
                    : (isInt ? builder.BuildSub(result, right, "subtmp") : builder.BuildFSub(result, right, "fsubtmp"));
            }
        }
        return result;
    }

    public override LLVMValueRef VisitComparison([NotNull] RedLangParser.ComparisonContext context)
    {
        var result = Visit(context.term(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == ">" || op == "<" || op == ">=" || op == "<=")
            {
                var right = Visit(context.term((i + 1) / 2));

                if (result.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                {
                    result = op switch
                    {
                        ">" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, result, right, "cmptmp"),
                        "<" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, result, right, "cmptmp"),
                        ">=" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, result, right, "cmptmp"),
                        "<=" => builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, result, right, "cmptmp"),
                        _ => result
                    };
                }
                else
                {
                    result = op switch
                    {
                        ">" => builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, result, right, "fcmptmp"),
                        "<" => builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, result, right, "fcmptmp"),
                        ">=" => builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, result, right, "fcmptmp"),
                        "<=" => builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, result, right, "fcmptmp"),
                        _ => result
                    };
                }
            }
        }
        return result;
    }

    public override LLVMValueRef VisitEquality([NotNull] RedLangParser.EqualityContext context)
    {
        var result = Visit(context.comparison(0));
        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == "==" || op == "!=")
            {
                var right = Visit(context.comparison((i + 1) / 2));

                if (result.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                    result = op == "=="
                        ? builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, result, right, "eqtmp")
                        : builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, result, right, "netmp");
                else
                    result = op == "=="
                        ? builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, result, right, "feqtmp")
                        : builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, result, right, "fnetmp");
            }
        }
        return result;
    }

    public override LLVMValueRef VisitLogicAnd([NotNull] RedLangParser.LogicAndContext context)
    {
        var result = Visit(context.equality(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            if (context.GetChild(i).GetText() == "and")
            {
                var right = Visit(context.equality((i + 1) / 2));
                result = builder.BuildAnd(result, right, "andtmp");
            }
        }
        return result;
    }

    public override LLVMValueRef VisitLogicOr([NotNull] RedLangParser.LogicOrContext context)
    {
        var result = Visit(context.logicAnd(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            if (context.GetChild(i).GetText() == "or")
            {
                var right = Visit(context.logicAnd((i + 1) / 2));
                result = builder.BuildOr(result, right, "ortmp");
            }
        }

        return result;
    }
    public override LLVMValueRef VisitPrintStmt([NotNull] RedLangParser.PrintStmtContext context)
    {
        var printfFunc = EnsurePrintfDeclared();
        var printfType = functionTypes["show"];

        var expr = context.expression();
        
        if (expr == null)
        {
            Console.WriteLine("[ERROR] VisitPrintStmt: no hay expresión");
            return default;
        }

        var value = Visit(expr);

        if (value.Handle == IntPtr.Zero)
        {
            Console.WriteLine("[ERROR] VisitPrintStmt: expresión evaluada a null");
            return default;
        }

        // CORRECCIÓN: Para booleanos, usar strings "true" o "false"
        if (value.TypeOf == LLVMTypeRef.Int1)
        {
            // Crear strings globales para true y false
            var trueStr = builder.BuildGlobalStringPtr("true", "str_true");
            var falseStr = builder.BuildGlobalStringPtr("false", "str_false");
            
            // Usar select para elegir entre true o false
            value = builder.BuildSelect(value, trueStr, falseStr, "bool_str");
            
            // Ahora el valor es un string (ptr i8)
            var formatStr = GetOrCreateFormatString("%s\n");
            return builder.BuildCall2(printfType, printfFunc, new[] { formatStr, value }, "printcall");
        }

        string format = value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind ? "%d\n"
            : value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind ? "%f\n"
            : "%s\n";

        var formatStr2 = GetOrCreateFormatString(format);
        return builder.BuildCall2(printfType, printfFunc, new[] { formatStr2, value }, "printcall");
    }
    
    public override LLVMValueRef VisitIfStmt([NotNull] RedLangParser.IfStmtContext context)
    {
        var condition = Visit(context.expression());

        var thenBlock = currentFunction.AppendBasicBlock("then");
        LLVMBasicBlockRef elseBlock = default;

        if (context.OTHERWISE() != null)
            elseBlock = currentFunction.AppendBasicBlock("else");

        // Crear condicional inicial
        builder.BuildCondBr(condition, thenBlock, 
            context.OTHERWISE() != null ? elseBlock : currentFunction.AppendBasicBlock("endif_tmp"));

        // THEN
        builder.PositionAtEnd(thenBlock);
        currentBlock = thenBlock;
        Visit(context.block(0));
        bool thenHasTerminator = currentBlock.Terminator.Handle != IntPtr.Zero;

        // ELSE
        bool elseHasTerminator = false;
        if (context.OTHERWISE() != null)
        {
            builder.PositionAtEnd(elseBlock);
            currentBlock = elseBlock;
            Visit(context.block(1));
            elseHasTerminator = currentBlock.Terminator.Handle != IntPtr.Zero;
        }

        // Solo creamos mergeBlock si alguno de los dos NO termina
        bool needsMerge = (!thenHasTerminator || !elseHasTerminator) || HasCodeAfter(context);
        LLVMBasicBlockRef mergeBlock = default;
        if (needsMerge)
            mergeBlock = currentFunction.AppendBasicBlock("ifcont");

        // Enlazar ramas sin terminator al merge
        if (!thenHasTerminator)
        {
            builder.PositionAtEnd(thenBlock);
            builder.BuildBr(mergeBlock);
        }

        if (context.OTHERWISE() != null && !elseHasTerminator)
        {
            builder.PositionAtEnd(elseBlock);
            builder.BuildBr(mergeBlock);
        }

        // Continuar en mergeBlock (solo si existe)
        if (needsMerge)
        {
            builder.PositionAtEnd(mergeBlock);
            currentBlock = mergeBlock;
        }

        return default;
    }


// Función auxiliar para saber si hay código después del if
private bool HasCodeAfter(RedLangParser.IfStmtContext context)
{
    if (context.Parent is RedLangParser.BlockContext parent)
    {
        var statements = parent.statement();
        for (int i = 0; i < statements.Length; i++)
        {
            if (object.ReferenceEquals(statements[i], context))
                return i < statements.Length - 1; // hay código después
        }
    }
    return false;
}


public override LLVMValueRef VisitWhileStmt([NotNull] RedLangParser.WhileStmtContext context)
{
    var condBlock = currentFunction.AppendBasicBlock("whilecond");
    var loopBlock = currentFunction.AppendBasicBlock("whileloop");
    var afterBlock = currentFunction.AppendBasicBlock("afterwhile");

    builder.BuildBr(condBlock);

    builder.PositionAtEnd(condBlock);
    currentBlock = condBlock;
    var condition = Visit(context.expression());
    builder.BuildCondBr(condition, loopBlock, afterBlock);

    builder.PositionAtEnd(loopBlock);
    currentBlock = loopBlock;
    Visit(context.block());
    if (currentBlock.Terminator.Handle == IntPtr.Zero)
        builder.BuildBr(condBlock);

    builder.PositionAtEnd(afterBlock);
    currentBlock = afterBlock;

    return default;
}


    public override LLVMValueRef VisitForStmt([NotNull] RedLangParser.ForStmtContext context)
    {
        if (context.declaration() != null)
            Visit(context.declaration());
        else if (context.assignment().Length > 0)
            Visit(context.assignment(0));

        var condBlock = currentFunction.AppendBasicBlock("forcond");
        var loopBlock = currentFunction.AppendBasicBlock("forloop");
        var incBlock = currentFunction.AppendBasicBlock("forinc");
        var afterBlock = currentFunction.AppendBasicBlock("afterfor");

        builder.BuildBr(condBlock);
        builder.PositionAtEnd(condBlock);
        currentBlock = condBlock;

        if (context.expression() != null)
        {
            var condition = Visit(context.expression());
            builder.BuildCondBr(condition, loopBlock, afterBlock);
        }
        else
            builder.BuildBr(loopBlock);

        builder.PositionAtEnd(loopBlock);
        currentBlock = loopBlock;
        Visit(context.block());
        if (currentBlock.Terminator.Handle == IntPtr.Zero)
            builder.BuildBr(incBlock);

        builder.PositionAtEnd(incBlock);
        currentBlock = incBlock;
        var assignments = context.assignment();
        if (assignments.Length > 0)
            Visit(assignments[assignments.Length - 1]);
        builder.BuildBr(condBlock);

        builder.PositionAtEnd(afterBlock);
        currentBlock = afterBlock;

        return default;
    }

    public override LLVMValueRef VisitCallExpr([NotNull] RedLangParser.CallExprContext context)
    {
        Console.WriteLine("\n[DEBUG-CALL] ==> Entrando a VisitCallExpr...");
        string functionName = context.IDENT()!.GetText();
        Console.WriteLine($"[DEBUG-CALL] Buscando función: '{functionName}'");

        if (!functions.ContainsKey(functionName))
        {
            Console.WriteLine($"[DEBUG-CALL] ERROR FATAL: La función '{functionName}' no está en el diccionario.");
            throw new Exception($"Error: Función '{functionName}' no definida");
        }

        var function = functions[functionName];
        var functionType = functionTypes[functionName];
        Console.WriteLine("[DEBUG-CALL] Función encontrada. Evaluando argumentos...");

        var argumentContexts = context.arguments()?.expression() ?? Array.Empty<RedLangParser.ExpressionContext>();
        var args = new LLVMValueRef[argumentContexts.Length];

        for (int i = 0; i < argumentContexts.Length; i++)
        {
            Console.WriteLine($"[DEBUG-CALL] Visitando argumento #{i + 1} ('{argumentContexts[i].GetText()}')");
            args[i] = Visit(argumentContexts[i]);
            Console.WriteLine($"[DEBUG-CALL] Argumento #{i + 1} evaluado a: {args[i]}");
        }

        Console.WriteLine("[DEBUG-CALL] Todos los argumentos evaluados. Intentando construir la llamada a LLVM...");

        Console.WriteLine($"[DEBUG-CALL] Tipo de función (correcto): {functionType}");
        Console.WriteLine($"[DEBUG-CALL] Función: {function}");
        Console.WriteLine($"[DEBUG-CALL] Número de argumentos: {args.Length}");

        try
        {
            var result = builder.BuildCall2(functionType, function, args, "calltmp");
            Console.WriteLine("[DEBUG-CALL] ¡Llamada a LLVM construida exitosamente!");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG-CALL] EXCEPCIÓN al construir llamada: {ex.Message}");
            Console.WriteLine($"[DEBUG-CALL] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public override LLVMValueRef VisitReturnStmt([NotNull] RedLangParser.ReturnStmtContext context)
    {
        var value = Visit(context.expression());
        return builder.BuildRet(value);
    }

    public override LLVMValueRef VisitBlock([NotNull] RedLangParser.BlockContext context)
    {
        foreach (var statement in context.statement())
        {
            var result = Visit(statement);
            if (statement.returnStmt() != null)
                return result;
        }
        return default;
    }

    public override LLVMValueRef VisitExpression([NotNull] RedLangParser.ExpressionContext context)
    {
        return VisitChildren(context);
    }

    public override LLVMValueRef VisitStatement([NotNull] RedLangParser.StatementContext context)
    {
        return VisitChildren(context);
    }

    public override LLVMValueRef VisitReadStmt([NotNull] RedLangParser.ReadStmtContext context)
    {
        string varName = context.IDENT().GetText();
        
        if (!namedValues.ContainsKey(varName))
        {
            Console.WriteLine($"[ERROR] VisitReadStmt: Variable '{varName}' no definida");
            return default;
        }
        
        // Asegurar que scanf esté declarado
        if (!functions.ContainsKey("scanf"))
        {
            var scanfFuncType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Int32,
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) },
                true
            );
            functions["scanf"] = module.AddFunction("scanf", scanfFuncType);
            functionTypes["scanf"] = scanfFuncType;
        }
        
        var scanfFunc = functions["scanf"];
        var scanfFunctionType = functionTypes["scanf"];
        var varPtr = namedValues[varName];
        var varType = variableTypes[varName];
        
        // Determinar el formato según el tipo
        string format;
        if (varType == LLVMTypeRef.Int32)
        {
            format = "%d";
        }
        else if (varType == LLVMTypeRef.Double)
        {
            format = "%lf";
        }
        else if (varType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            format = "%s";
        }
        else if (varType == LLVMTypeRef.Int1)
        {
            format = "%d";
        }
        else
        {
            format = "%d"; // default
        }
        
        var formatStr = GetOrCreateFormatString(format);
        
        // Llamar scanf con el formato y la dirección de la variable
        return builder.BuildCall2(scanfFunctionType, scanfFunc, new[] { formatStr, varPtr }, "scancall");
    }
    public override LLVMValueRef VisitArguments([NotNull] RedLangParser.ArgumentsContext context) => VisitChildren(context);
    public override LLVMValueRef VisitParam([NotNull] RedLangParser.ParamContext context) => VisitChildren(context);
    public override LLVMValueRef VisitParameters([NotNull] RedLangParser.ParametersContext context) => VisitChildren(context);
    public override LLVMValueRef VisitType([NotNull] RedLangParser.TypeContext context) => VisitChildren(context);
    public override LLVMValueRef VisitArrayAccess([NotNull] RedLangParser.ArrayAccessContext context) => default;
    public override LLVMValueRef VisitArrayAssignment([NotNull] RedLangParser.ArrayAssignmentContext context) => default;
    public override LLVMValueRef VisitReadFileStmt([NotNull] RedLangParser.ReadFileStmtContext context) => default;
    public override LLVMValueRef VisitWriteFileStmt([NotNull] RedLangParser.WriteFileStmtContext context) => default;
}
