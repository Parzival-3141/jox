using Jox.Runtime;
using System;
using System.IO;
using System.Linq;

namespace Jox
{
    public static class Jox
    {
        private static Interpreter interpreter = new Interpreter();
        private static bool hadParseError = false;
        private static bool hadRuntimeError = false;
        private static bool printAST = false;

        private const int ERROR_INVALID_ARGS = 1;
        private const int ERROR_INVALID_SOURCE = 2;
        private const int ERROR_PARSE_FAILURE = 3;
        private const int ERROR_RUNTIME_FAILURE = 4;

        static void Main(string[] args)
        {
            //if (args.Length > 1)
            //{
            //    Console.WriteLine("Usage: jox <source-file>");
            //    Environment.Exit(ERROR_INVALID_ARGS);
            //}
            //else if (args.Length == 1)
            //{
            //    RunFile(args[0]);
            //}
            //else
            //    RunPrompt();

            if (args.Length > 2 || args.Contains("-h") || args.Contains("-help"))
            {
                Console.WriteLine("Usage: jox <source-file> [-debug] [-h|-help]");
                System.Environment.Exit(ERROR_INVALID_ARGS);
            }

            printAST = args.Contains("-debug");
            
            if(args.Length > 0 && args[0].EndsWith(".jox"))
                RunFile(args[0]);
            else
                RunPrompt();
        }

        private static void Run(string source)
        {
            var tokens = Parsing.Lexer.Lex(source);
            var statements = Parsing.Parser.Parse(tokens);

            //foreach(var t in tokens)
            //    Console.WriteLine(t);

            if (hadParseError) return;

            if (printAST)
            {
                var printer = new Parsing.ASTPrinter();
                foreach(dynamic stmt in statements)
                {
                    try
                    {
                        printer.Print(stmt.expression);
                    }
                    catch
                    {
                        Console.WriteLine("PrinterError: Invalid IStmt (" + stmt + ")");
                    }
                }
            }
            
            interpreter.Interpret(statements);
        }

        private static void RunPrompt()
        {
            Console.WriteLine("*WIP: Statements and Expressions only!*");
            Console.WriteLine("jox Interactive Prompt \n(Type 'exit' to quit)");
            
            while(true)
            {
                Console.Write("> ");
                
                var line = Console.ReadLine();
                if (line == null || line == "exit") break;
                
                Run(line);
                hadParseError = false;
            }
        }

        private static void RunFile(string path)
        {
            string source = "";

            try
            {
                source = File.ReadAllBytes(path).BytesToString();
            }
            catch
            {
                Console.WriteLine("Invalid source-file.");
                System.Environment.Exit(ERROR_INVALID_SOURCE);
            }

            Run(source);
            if (hadParseError) System.Environment.Exit(ERROR_PARSE_FAILURE);
            if (hadRuntimeError) System.Environment.Exit(ERROR_RUNTIME_FAILURE);
        }

        //@Refactor: could abstract into an 'ErrorReporter' that gets passed around
        public static void ReportError(int line, string context, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] {context}Error {where}: {message}");
        }

        public static void ReportError(Parsing.Token token, string context, string message)
        {
            ReportError(token.line, context, $"at '{token.lexeme}'", message);
        }

        public static void ParseError(int line, string message) => ReportError(line, "Parse", "", message);
        public static void ParseError(Parsing.Token token, string message)
        {
            if (token.type == Parsing.TokenType.EOF)
                ReportError(token.line, "Parse", "at end of file", message);
            else
                ReportError(token, "Parse", message);

            hadParseError = true;
        }

        internal static void RuntimeError(RuntimeError error)
        {
            ReportError(error.token, "Runtime", error.Message);
            hadRuntimeError = true;
        }
    }
}
