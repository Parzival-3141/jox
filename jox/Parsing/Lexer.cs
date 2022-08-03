using System;
using System.Collections.Generic;

namespace Jox.Parsing
{
    public struct Token
    {
        public TokenType type;
        public string lexeme;
        public object literal;
        public int line;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return $"{type} {lexeme} {literal}";
        }
    }

    public enum TokenType
    {
        // Single-character tokens.
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens.
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.
        IDENTIFIER, STRING, NUMBER,

        // Keywords.
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        EOF
    }


    public static class Lexer
    {
        private static List<Token> tokens = new List<Token>();
        private static string src;

        static int lexemeStartIndex = 0;
        static int currentIndex = 0;
        static int line = 1;

        static Dictionary<string, TokenType> keywords = new()
        {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "fun", TokenType.FUN },
            { "for", TokenType.FOR },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE },
        };


        public static List<Token> Lex(string source)
        {
            src = source;
            tokens.Clear();
            currentIndex = 0;
            line = 1;

            while (!AtEof())
            {
                lexemeStartIndex = currentIndex;

                LexToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private static void LexToken()
        {
            var c = EatChar();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;

                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                
                case '+': AddToken(TokenType.PLUS); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '*': AddToken(TokenType.STAR); break;


                case '!': AddToken(MatchNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(MatchNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(MatchNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(MatchNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;

                case '/':
                    if (MatchNext('/'))
                        while (Peek() != '\n' && !AtEof()) EatChar();
                    else
                        AddToken(TokenType.SLASH);
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n': line++; break;

                case '"': HandleString(); break;

                default:
                    if (char.IsDigit(c))
                        HandleNumber();

                    else if (IsAlpha(c))
                        HandleIdentifier();
                    
                    else
                        Jox.Error(line, $"Unexpected character '{c}'");
                    break;
            }
        }

        static void AddToken(TokenType type) => AddToken(type, null);
        static void AddToken(TokenType type, object literal)
        {
            tokens.Add(new Token(type, CurrentLexeme(), literal, line));
        }

        static void HandleString()
        {
            while(Peek() != '"' && !AtEof())
            {
                if (Peek() == '\n') line++; //@Refactor: maybe don't have multi-line strings? Use + instead.
                _ = EatChar();
            }

            if (AtEof())
            {
                Jox.Error(line, "Unterminated string.");
                return;
            }

            //  Eat closing "
            _ = EatChar();

            AddToken(TokenType.STRING, CurrentLexeme().Trim('"'));
        }

        static void HandleNumber()
        {
            while (char.IsDigit(Peek())) EatChar();

            if(Peek() == '.' && char.IsDigit(Peek(1)))
            {
                EatChar(); // Eat .

                while (char.IsDigit(Peek())) EatChar();
            }

            AddToken(TokenType.NUMBER, double.Parse(CurrentLexeme()));
        }

        static void HandleIdentifier()
        {
            while (IsAlphaNumeric(Peek())) EatChar();

            if (!keywords.TryGetValue(CurrentLexeme(), out TokenType type))
                type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';
        static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);

        static string CurrentLexeme() => src.Substring(lexemeStartIndex, currentIndex - lexemeStartIndex);
        private static bool AtEof() => currentIndex >= src.Length;

        static bool MatchNext(char expected)
        {
            if (AtEof()) return false;
            if (Peek() != expected) return false;

            _ = EatChar();
            return true;
        }

        private static char EatChar() => src[currentIndex++];
        private static char Peek()
        {
            if (AtEof()) return '\0';
            return src[currentIndex];
        }

        private static char Peek(int lookAhead)
        {
            if (currentIndex + lookAhead >= src.Length) return '\0';
            return src[currentIndex + lookAhead];
        }

    }
}