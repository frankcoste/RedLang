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
    private Dictionary<string, LLVMValueRef> arrayPointers = new();
    private Dictionary<string, int> arraySizes = new();

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
        string typeText = context.type().GetText();

        Console.WriteLine($"  [DEBUG] Declarando variable '{varName}' tipo '{typeText}'");

        // Verificar si es un array
        if (typeText.StartsWith("array["))
        {
            // Extraer el tipo base del array
            string baseType = typeText.Substring(6, typeText.Length - 7); // Quitar "array[" y "]"
            var elementType = GetLLVMType(baseType);
            
            // Por ahora, crear un array de tamaño fijo (100 elementos)
            int arraySize = 100;
            var arrayType = LLVMTypeRef.CreateArray(elementType, (uint)arraySize);
            
            // Alocar el array
            var arrayAlloca = builder.BuildAlloca(arrayType, varName);
            arrayPointers[varName] = arrayAlloca;
            arraySizes[varName] = arraySize;
            
            // Inicializar a ceros
            var zeroInit = LLVMValueRef.CreateConstNull(arrayType);
            builder.BuildStore(zeroInit, arrayAlloca);
            
            Console.WriteLine($"  [DEBUG] Array '{varName}' declarado con {arraySize} elementos");
            return arrayAlloca;
        }
        
        // Declaración normal (no array)
        var varType = GetLLVMType(typeText);
        var alloca = builder.BuildAlloca(varType, varName);
        namedValues[varName] = alloca;
        variableTypes[varName] = varType;

        if (context.expression() != null)
        {
            Console.WriteLine($"  [DEBUG] Evaluando expresión: '{context.expression().GetText()}'");
            var value = Visit(context.expression());
            
            if (value.Handle == IntPtr.Zero)
            {
                Console.WriteLine($"  [ERROR] Expresión evaluó a null");
                value = GetDefaultValue(varType);
            }
            var castedValue = BuildCast(value, varType);
            
            Console.WriteLine($"  [DEBUG] Resultado: {castedValue}");
            builder.BuildStore(castedValue, alloca); 
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
        {
            var varType = variableTypes[varName]; // Obtener el tipo de la variable de destino
            var castedValue = BuildCast(value, varType); // Convertir el valor
            builder.BuildStore(castedValue, namedValues[varName]);
            return castedValue; // Devuelve el valor convertido
        }

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
        // Verificar callExpr primero (tiene prioridad)
        if (context.callExpr() != null)
        {
            return Visit(context.callExpr());
        }
        
        // Verificar arrayAccess
        if (context.arrayAccess() != null)
        {
            return Visit(context.arrayAccess());
        }
        
        // Variable simple
        if (context.IDENT() != null)
        {
            string name = context.IDENT().GetText();
            
            // Verificar si es un array (no debería llegar aquí para arrays, pero por si acaso)
            if (arrayPointers.ContainsKey(name))
            {
                Console.WriteLine($"[ERROR] Intento de usar array '{name}' sin índice");
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
            }
            
            if (namedValues.ContainsKey(name))
                return builder.BuildLoad2(variableTypes[name], namedValues[name], name);
            
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }
        
        // Literal o expresión entre paréntesis
        return base.VisitPrimary(context);
    }

    public override LLVMValueRef VisitUnary([NotNull] RedLangParser.UnaryContext context)
    {
        // Si tenemos operador unario (- o not)
        if (context.MINUS() != null || context.NOT() != null)
        {
            // Primero verificar si hay un unary anidado
            if (context.unary() != null)
            {
                var value = Visit(context.unary());
                
                if (value.Handle == IntPtr.Zero)
                {
                    Console.WriteLine("[ERROR] VisitUnary: Visit(unary) devolvió un valor nulo");
                    return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
                }

                if (context.MINUS() != null)
                    return value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                        ? builder.BuildNeg(value, "negtmp")
                        : builder.BuildFNeg(value, "fnegtmp");
                else if (context.NOT() != null)
                    return builder.BuildNot(value, "nottmp");
            }
        }
        
        // Si no hay operador unario, visitar el primary
        if (context.primary() != null)
        {
            var value = Visit(context.primary());
            
            if (value.Handle == IntPtr.Zero)
            {
                Console.WriteLine("[ERROR] VisitUnary: Visit(primary) devolvió un valor nulo");
                return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
            }
            
            return value;
        }

        return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
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

        // Crear merge block temporalmente
        var mergeBlock = currentFunction.AppendBasicBlock("ifcont");

        // Crear condicional inicial
        builder.BuildCondBr(condition, thenBlock, 
            context.OTHERWISE() != null ? elseBlock : mergeBlock);

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

        // Si ambas ramas terminan, eliminar el merge block
        bool bothTerminate = thenHasTerminator && (context.OTHERWISE() == null || elseHasTerminator);
        
        if (bothTerminate)
        {
            // Eliminar el merge block ya que no se usa
            mergeBlock.Delete();
            // No hay currentBlock válido después de esto
            currentBlock = default;
        }
        else
        {
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

            // Continuar en mergeBlock
            builder.PositionAtEnd(mergeBlock);
            currentBlock = mergeBlock;
        }

        return default;
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

    public override LLVMValueRef VisitArrayAccess([NotNull] RedLangParser.ArrayAccessContext context)
    {
       string arrayName = context.IDENT().GetText();
            
        if (!arrayPointers.ContainsKey(arrayName))
        {
            Console.WriteLine($"[ERROR] Array '{arrayName}' no declarado");
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
        }

        var arrayPtr = arrayPointers[arrayName];
        var indexExpr = Visit(context.expression());
            
        // Obtener el puntero al elemento
        var elementPtr = builder.BuildGEP2(
            LLVMTypeRef.Int32,
            arrayPtr,
            new[] { indexExpr },
            "arrayidx"
        );
            
        // Cargar el valor
        return builder.BuildLoad2(LLVMTypeRef.Int32, elementPtr, "arrayload");
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

        string format;
        LLVMValueRef targetPtr; 


        if (varType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        {
            format = "%s"; // Formato para string


            const uint bufferSize = 1024;
            var bufferType = LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, bufferSize);
            var bufferAlloca = builder.BuildAlloca(bufferType, $"{varName}_buffer");

            targetPtr = builder.BuildGEP2(bufferType, bufferAlloca,
                new[] {
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0), 
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0)  
                },
            "buffer_ptr");

            var originalStringPtr = varPtr;

            var formatStr = GetOrCreateFormatString(format);
            var scanCall = builder.BuildCall2(scanfFunctionType, scanfFunc, new[] { formatStr, targetPtr }, "scancall");
            
          
            builder.BuildStore(targetPtr, originalStringPtr);

            return scanCall;
        }
        else 
        {
            targetPtr = varPtr; 
            if (varType == LLVMTypeRef.Int32)
            {
                format = "%d";
            }
            else if (varType == LLVMTypeRef.Double)
            {
                format = "%lf";
            }
            else if (varType == LLVMTypeRef.Int1)
            {
                format = "%d"; 
            }
            else
            {
                format = "%d"; // Default
            }
        }
        

        var finalFormatStr = GetOrCreateFormatString(format);

        return builder.BuildCall2(scanfFunctionType, scanfFunc, new[] { finalFormatStr, targetPtr }, "scancall");
    }
    public override LLVMValueRef VisitArguments([NotNull] RedLangParser.ArgumentsContext context) => VisitChildren(context);
    public override LLVMValueRef VisitParam([NotNull] RedLangParser.ParamContext context) => VisitChildren(context);
    public override LLVMValueRef VisitParameters([NotNull] RedLangParser.ParametersContext context) => VisitChildren(context);
    public override LLVMValueRef VisitType([NotNull] RedLangParser.TypeContext context) => VisitChildren(context);
    public override LLVMValueRef VisitArrayAssignment([NotNull] RedLangParser.ArrayAssignmentContext context)
    {
        var arrayAccessCtx = context.arrayAccess();
        string arrayName = arrayAccessCtx.IDENT().GetText();

        if (!arrayPointers.ContainsKey(arrayName))
        {
            Console.WriteLine($"[ERROR] Array '{arrayName}' no declarado");
            return default;
        }

        var arrayPtr = arrayPointers[arrayName];
        var indexExpr = Visit(arrayAccessCtx.expression());
        var value = Visit(context.expression());

        // Obtener el puntero al elemento
        var elementPtr = builder.BuildGEP2(
            LLVMTypeRef.Int32,
            arrayPtr,
            new[] { indexExpr },
            "arrayidx"
        );

        // Almacenar el valor
        builder.BuildStore(value, elementPtr);
        return value;
    }
    public override LLVMValueRef VisitReadFileStmt([NotNull] RedLangParser.ReadFileStmtContext context)
    {
        // Por ahora, solo imprimir un mensaje de que no está implementado
        var printfFunc = EnsurePrintfDeclared();
        var printfType = functionTypes["show"];
        var formatStr = GetOrCreateFormatString("readfile: no implementado\n");

        return builder.BuildCall2(printfType, printfFunc, new[] { formatStr }, "readfile_stub");
    }
    public override LLVMValueRef VisitWriteFileStmt([NotNull] RedLangParser.WriteFileStmtContext context)
    {
        // Por ahora, solo imprimir un mensaje de que no está implementado
        var printfFunc = EnsurePrintfDeclared();
        var printfType = functionTypes["show"];
        var formatStr = GetOrCreateFormatString("writefile: no implementado\n");

        return builder.BuildCall2(printfType, printfFunc, new[] { formatStr }, "writefile_stub");
    }

    //METODOS AUXILIARES

    private LLVMValueRef BuildCast(LLVMValueRef value, LLVMTypeRef destType, string name = "casttmp")
    {
        var sourceType = value.TypeOf;

        if (sourceType.Handle == destType.Handle)
        {
            return value; // No se necesita cast, los tipos ya son iguales
        }

        // De entero a flotante (i32 -> double)
        if (sourceType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && destType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
        {
            return builder.BuildSIToFP(value, destType, name); // SIToFP = Signed Integer to Floating Point
        }

        // De flotante a entero (double -> i32)
        if (sourceType.Kind == LLVMTypeKind.LLVMDoubleTypeKind && destType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        {
            return builder.BuildFPToSI(value, destType, name); // FPToSI = Floating Point to Signed Integer
        }

        // De booleano (i1) a entero (i32) o flotante (double)
        if (sourceType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && sourceType.IntWidth == 1)
        {
            if (destType.Kind == LLVMTypeKind.LLVMIntegerTypeKind) // i1 -> i32
            {
                return builder.BuildZExt(value, destType, name); // ZExt = Zero Extend
            }
            if (destType.Kind == LLVMTypeKind.LLVMDoubleTypeKind) // i1 -> double
            {
                var intVal = builder.BuildZExt(value, LLVMTypeRef.Int32, "i1toi32");
                return builder.BuildSIToFP(intVal, destType, name);
            }
        }

        // Si no se encuentra una conversión válida, devuelve el valor original (puede causar errores de LLVM, pero evita que el compilador se caiga)
        Console.WriteLine($"[WARNING] No cast available from {sourceType} to {destType}");
        return value;
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
    // ----
}

