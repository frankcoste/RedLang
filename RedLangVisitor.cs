using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

public class RedLangVisitor : RedLangBaseVisitor<object>
{
    private readonly Dictionary<string, object> memory = new();
    private readonly Dictionary<string, RedLangParser.FunctionDeclContext> functions = new();

    public override object VisitProgram([NotNull] RedLangParser.ProgramContext context) 
    {
        // Primero registra todas las funciones
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (child is RedLangParser.FunctionDeclContext funcContext)
            {
                string functionName = funcContext.IDENT().GetText();
                functions[functionName] = funcContext;
            }
        }
        
        // Luego visita el resto del programa
        for (int i = 0; i < context.ChildCount; i++)
        {
            var child = context.GetChild(i);
            if (!(child is RedLangParser.FunctionDeclContext))
            {
                Visit(child);
            }
        }
        return null;
    }

    public override object VisitDeclaration([NotNull] RedLangParser.DeclarationContext context)
    {
        string varName = context.IDENT().GetText();
        object value = context.expression() != null ? Visit(context.expression()) : GetDefaultValueForType(context.type());
        memory[varName] = value;
        return null;
    }

    private object GetDefaultValueForType(RedLangParser.TypeContext typeContext)
    {
        string type = typeContext.GetText();
        switch (type)
        {
            case "i": return 0;
            case "s": return "";
            case "b": return false;
            default: return 0;
        }
    }

    public override object VisitLiteral([NotNull] RedLangParser.LiteralContext context)
    {
        if (context.INT_LIT() != null)
            return int.Parse(context.INT_LIT().GetText());
        if (context.STRING_LIT() != null)
            return context.STRING_LIT().GetText().Trim('"');
        if (context.TRUE() != null)
            return true;
        if (context.FALSE() != null)
            return false;
        return 0;
    }

    public override object VisitArguments([NotNull] RedLangParser.ArgumentsContext context) { return VisitChildren(context); }

    public override object VisitCallExpr([NotNull] RedLangParser.CallExprContext context)
    {
        string functionName = context.GetToken(RedLangParser.IDENT, 0).GetText();
        
        if (!functions.ContainsKey(functionName))
        {
            Console.WriteLine($"Error de función: La función '{functionName}' no está definida.");
            Environment.Exit(1);
        }
        
        RedLangParser.FunctionDeclContext functionDef = functions[functionName];
        
        Dictionary<string, object> savedMemory = new(memory);
        
        var parameters = functionDef.parameters().param();
        var arguments = context.arguments().expression(); 
        
        if (parameters.Length != arguments.Length)
        {
            Console.WriteLine($"Error de función: La función '{functionName}' espera {parameters.Length} argumentos pero recibió {arguments.Length}.");
            Environment.Exit(1);
        }
        
        // Limpiar la memoria temporal para variables locales
        Dictionary<string, object> localMemory = new(savedMemory);
        
        // Asignar argumentos a parámetros
        for (int i = 0; i < parameters.Length; i++)
        {
            string paramName = parameters[i].IDENT().GetText(); 
            object argValue = Visit(arguments[i]); 
            localMemory[paramName] = argValue; 
        }
        
        // Usar la memoria local para ejecutar la función
        var originalMemory = new Dictionary<string, object>(memory);
        foreach (var entry in localMemory)
        {
            memory[entry.Key] = entry.Value;
        }
        
        // Ejecutar el cuerpo de la función
        object returnValue = Visit(functionDef.block());
        
        // Restaurar la memoria original
        memory.Clear();
        foreach (var entry in originalMemory)
        {
            memory[entry.Key] = entry.Value;
        }

        return returnValue;
    }

    public override object VisitPrimary([NotNull] RedLangParser.PrimaryContext context)
    {
        if (context.IDENT() != null)
        {
            string name = context.IDENT().GetText();
            return memory.ContainsKey(name) ? memory[name] : 0;
        }
        return base.VisitPrimary(context);
    }

    public override object VisitUnary([NotNull] RedLangParser.UnaryContext context)
    {
        object result = Visit(context.primary());

        if (context.MINUS() != null)
        {
            if (result is int intValue)
                return -intValue;
            else if (result is double doubleValue)
                return -doubleValue;
            else
                return -Convert.ToDouble(result);
        }
        else if (context.NOT() != null) 
        {
            return !Convert.ToBoolean(result);
        }
        return result;
    }

    public override object VisitFactor([NotNull] RedLangParser.FactorContext context)
    {
        object result = Visit(context.unary(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == "*" || op == "/" || op == "%")
            {
                object right = Visit(context.unary(i / 2 + 1));

                switch (op)
                {
                    case "*":
                        if (result is int && right is int)
                            result = (int)result * (int)right;
                        else
                            result = Convert.ToDouble(result) * Convert.ToDouble(right);
                        break;
                    case "/":
                        result = Convert.ToDouble(result) / Convert.ToDouble(right);
                        break;
                    case "%":
                        if (result is int && right is int)
                            result = (int)result % (int)right;
                        else
                            result = Convert.ToDouble(result) % Convert.ToDouble(right);
                        break;
                }
            }
        }
        return result;
    }

    public override object VisitTerm([NotNull] RedLangParser.TermContext context)
    {
        object result = Visit(context.factor(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText(); 
            
            if (op == "+" || op == "-")
            {
                object right = Visit(context.factor(i / 2 + 1));

                switch (op)
                {
                    case "+":
                        if (result is string || right is string)
                            result = result.ToString() + right.ToString();
                        else if (result is int && right is int)
                            result = (int)result + (int)right;
                        else
                            result = Convert.ToDouble(result) + Convert.ToDouble(right);
                        break;
                    case "-":
                        if (result is int && right is int)
                            result = (int)result - (int)right;
                        else
                            result = Convert.ToDouble(result) - Convert.ToDouble(right);
                        break;
                }
            }
        }
        return result;   
    }

    public override object VisitComparison([NotNull] RedLangParser.ComparisonContext context)
    {
        object result = Visit(context.term(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText(); 
            if (op == ">" || op == "<" || op == ">=" || op == "<=")
            {
                object right = Visit(context.term((i + 1) / 2)); 

                double leftVal = Convert.ToDouble(result);
                double rightVal = Convert.ToDouble(right);

                switch (op)
                {
                    case ">":
                        result = leftVal > rightVal;
                        break;
                    case "<":
                        result = leftVal < rightVal;
                        break;
                    case ">=":
                        result = leftVal >= rightVal;
                        break;
                    case "<=":
                        result = leftVal <= rightVal;
                        break;
                }
            }
        }
        return result;
    }

    public override object VisitEquality([NotNull] RedLangParser.EqualityContext context)
    {
        object result = Visit(context.comparison(0));
        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == "==" || op == "!=")
            {
                object right = Visit(context.comparison((i + 1) / 2));

                switch (op)
                {
                    case "==":
                        result = object.Equals(result, right);
                        break;
                    case "!=":
                        result = !object.Equals(result, right);
                        break;
                }
            }
        }
        return result;
    }

    public override object VisitLogicAnd([NotNull] RedLangParser.LogicAndContext context)
    {
        object result = Visit(context.equality(0));

        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText(); 
            if (op == "and")
            {
                object right = Visit(context.equality((i + 1) / 2)); 
                bool leftval = Convert.ToBoolean(result);
                bool rightVal = Convert.ToBoolean(right);
                result = leftval && rightVal;
            }
        }
        return result;
    }

    public override object VisitLogicOr([NotNull] RedLangParser.LogicOrContext context)
    {
        object result = Visit(context.logicAnd(0));
        
        for (int i = 1; i < context.ChildCount; i += 2)
        {
            string op = context.GetChild(i).GetText();
            if (op == "or")
            {
                object right = Visit(context.logicAnd((i + 1) / 2));

                bool leftval = Convert.ToBoolean(result);
                bool rightVal = Convert.ToBoolean(right);

                result = leftval || rightVal;
            }
        }
        return result;
    }

    public override object VisitExpression([NotNull] RedLangParser.ExpressionContext context) {return VisitChildren(context);}

    public override object VisitReadStmt([NotNull] RedLangParser.ReadStmtContext context)
    {
        string varName = context.IDENT().GetText();
        string input = Console.ReadLine();
        memory[varName] = input;        
        return null;
    }

    public override object VisitPrintStmt([NotNull] RedLangParser.PrintStmtContext context)
    {
        object value = Visit(context.expression());
        Console.WriteLine(value?.ToString());
        return null;
    }

    public override object VisitAssignment([NotNull] RedLangParser.AssignmentContext context)
    {
        string varName = context.IDENT().GetText();
        object value = Visit(context.expression());
        memory[varName] = value;
        return null;
    }

    public override object VisitForStmt([NotNull] RedLangParser.ForStmtContext context)
    {
        if (context.declaration() != null)
        {
            Visit(context.declaration());
        }
        else if (context.assignment().Length > 0)
        {
            Visit(context.assignment(0));
        }

        while (true)
        {
            if (context.expression() != null)
            {
                bool condition = Convert.ToBoolean(Visit(context.expression()));
                if (!condition)
                    break;
            }

            Visit(context.block());

            var assignments = context.assignment();
            if (assignments.Length > 0)
            {
                Visit(assignments[assignments.Length - 1]);
            }
        }
        return null;
    }

    public override object VisitWhileStmt([NotNull] RedLangParser.WhileStmtContext context)
    {
        bool condition = Convert.ToBoolean(Visit(context.expression()));

        while (condition)
        {
            Visit(context.block());
            condition = Convert.ToBoolean(Visit(context.expression())); 
        }

        return null;
    }

    public override object VisitIfStmt([NotNull] RedLangParser.IfStmtContext context)
    {
        object conditionResult = Visit(context.expression());
        bool condition = Convert.ToBoolean(conditionResult);

        if (condition)
        {
            Visit(context.block(0));
        }
        else if (context.OTHERWISE() != null)
        {
            Visit(context.block(1));
        }

        return null;
    }

    public override object VisitBlock([NotNull] RedLangParser.BlockContext context) 
    {
        var statements = context.statement(); 
        
        foreach (var statement in statements)
        {
            object result = Visit(statement);
            
            if (statement.returnStmt() != null) // Si es una sentencia de retorno
            {
                return result; 
            }
        }
        
        return null; 
    }

    public override object VisitStatement([NotNull] RedLangParser.StatementContext context) { return VisitChildren(context); }

    public override object VisitReturnStmt([NotNull] RedLangParser.ReturnStmtContext context)
    {
        object result = Visit(context.expression());
        return result;
    }

    public override object VisitParam([NotNull] RedLangParser.ParamContext context) { return VisitChildren(context); }

    public override object VisitParameters([NotNull] RedLangParser.ParametersContext context) { return VisitChildren(context); }

    public override object VisitFunctionDecl([NotNull] RedLangParser.FunctionDeclContext context)
    {
        string functionName = context.GetToken(RedLangParser.IDENT, 0).GetText(); 
        functions[functionName] = context;
        return null;
    }

    public override object VisitType([NotNull] RedLangParser.TypeContext context)
    {
        return VisitChildren(context);
    }

    // Los métodos para arrays, lectura/escritura de archivos pueden permanecer igual
    public override object VisitArrayAccess([NotNull] RedLangParser.ArrayAccessContext context)
    {
        string varName = context.GetToken(RedLangParser.IDENT, 0).GetText();
        List<object> array = (List<object>)memory[varName];
        int index = Convert.ToInt32(Visit(context.expression()));

        if (index < 0 || index >= array.Count)
        {
            Console.WriteLine($"Error de array: Índice '{index}' fuera de los límites de [0..{array.Count - 1}].");
            Environment.Exit(1);
        }

        return array[index];
    }

    public override object VisitArrayAssignment([NotNull] RedLangParser.ArrayAssignmentContext context)
    {
        string varName = context.GetToken(RedLangParser.IDENT, 0).GetText();
        List<object> array = (List<object>)memory[varName]; 
        var expressions = context.GetRuleContexts<RedLangParser.ExpressionContext>();
        int index = Convert.ToInt32(Visit(expressions[0])); 

        if (index < 0 || index >= array.Count)
        {
            Console.WriteLine($"Error de array: Índice '{index}' fuera de los límites de [0..{array.Count - 1}].");
            Environment.Exit(1);
        }

        object value = Visit(expressions[1]); 
        array[index] = value;
        
        return null;
    }

    public override object VisitReadFileStmt([NotNull] RedLangParser.ReadFileStmtContext context) 
    {
        string varName = context.GetToken(RedLangParser.IDENT, 0).GetText(); 
        var expressions = context.GetRuleContexts<RedLangParser.ExpressionContext>();
        string filePath = Convert.ToString(Visit(expressions[0])); 
        string fileContent = System.IO.File.ReadAllText(filePath);
        memory[varName] = fileContent;
        return null; 
    }

    public override object VisitWriteFileStmt([NotNull] RedLangParser.WriteFileStmtContext context)
    {
        var expressions = context.GetRuleContexts<RedLangParser.ExpressionContext>();
        string filePath = Convert.ToString(Visit(expressions[0]));
        object contentValue = Visit(expressions[1]);
        System.IO.File.WriteAllText(filePath, Convert.ToString(contentValue));
        return null;
    }
}