using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace MiProyectoANTLR
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Iniciando compilación...");

                if (!File.Exists("CodigoRedlang.txt"))
                {
                    Console.WriteLine("ERROR: No se encuentra el archivo CodigoPrueba.txt");
                    return;
                }

                Console.WriteLine("Leyendo archivo...");
                string input = File.ReadAllText("CodigoPrueba.txt");
                Console.WriteLine($"Contenido leído ({input.Length} caracteres)");

                Console.WriteLine("Creando lexer...");
                AntlrInputStream inputStream = new AntlrInputStream(input);
                ExprLexer lexer = new ExprLexer(inputStream);

                Console.WriteLine("Creando parser...");
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                RedLangParser parser = new RedLangParser(tokenStream);

                Console.WriteLine("Parseando programa...");
                RedLangParser.ProgramContext tree = parser.program();
                Console.WriteLine("Árbol parseado exitosamente");

                Console.WriteLine("Creando generador de código LLVM...");
                LLVMCodeGeneratorVisitor codeGenerator = new LLVMCodeGeneratorVisitor();

                Console.WriteLine("Generando código LLVM IR...");
                codeGenerator.Visit(tree);
                Console.WriteLine("Código generado exitosamente");

                Console.WriteLine("\n--- LLVM IR Generado ---");
                string ir = codeGenerator.GetIR();
                Console.WriteLine(ir);

                Console.WriteLine("\nGuardando archivo...");
                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.ll");
                codeGenerator.WriteIRToFile(outputPath);
                Console.WriteLine($"LLVM IR guardado en: {outputPath}");

                Console.WriteLine("\nCompilando con clang para generar .exe...");

                string exePath = Path.Combine(Directory.GetCurrentDirectory(), "output.exe");
                string clangCommand = $"--target=x86_64-w64-mingw32 \"{outputPath}\" -o \"{exePath}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "clang",
                    Arguments = clangCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    string stdout = await process.StandardOutput.ReadToEndAsync();
                    string stderr = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n✅ Compilación completada. Ejecutable generado en: {exePath}");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n❌ Error durante la compilación con clang:");
                        Console.WriteLine(stderr);
                    }

                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n--- ERROR DURANTE LA COMPILACIÓN ---");
                Console.WriteLine($"\nTipo de Error: {ex.GetType().Name}");
                Console.WriteLine($"\nMensaje: {ex.Message}");
                Console.WriteLine("\n--- Stack Trace ---");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }
    }
}
