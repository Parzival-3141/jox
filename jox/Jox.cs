using System;
using System.IO;

namespace Jox
{
    public static class Jox
    {
        private static bool hadError = false;
        private const int ERROR_INVALID_ARGS = 1;
        private const int ERROR_INVALID_SOURCE = 2;
        private const int ERROR_PARSE_FAILURE = 3;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jox <source-file>");
                Environment.Exit(ERROR_INVALID_ARGS);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
                RunPrompt();

        }

        private static void Run(string source)
        {
            var tokens = Parsing.Lexer.Lex(source);
            var expr = Parsing.Parser.Parse(tokens);
            //foreach(var t in tokens)
            //    Console.WriteLine(t);

            if (hadError) return;
            new Parsing.ASTPrinter().Print(expr);
        }

        private static void RunPrompt()
        {
            Console.WriteLine("jox Interactive Prompt :)");
            Console.WriteLine("Operators & Literals only, and just AST printing :(");
            
            while(true)
            {
                Console.Write("> ");
                
                var line = Console.ReadLine();
                if (line == null) break;
                
                Run(line);
                hadError = false;
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
                Environment.Exit(ERROR_INVALID_SOURCE);
            }

            Run(source);
            if (hadError) Environment.Exit(ERROR_PARSE_FAILURE);
        }

        //@Refactor: abstract into an 'ErrorReporter' that gets passed around
        public static void Error(int line, string message) => Report(line, "", message);
        public static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            hadError = true;
        }

        public static void Error(Parsing.Token token, string message)
        {
            if (token.type == Parsing.TokenType.EOF)
                Report(token.line, "at end of file", message);
            else
                Report(token.line, $"at '{token.lexeme}'", message);
        }
    }
}
