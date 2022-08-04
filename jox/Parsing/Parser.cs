using System;
using System.Collections.Generic;
using Jox.Parsing.AST;

namespace Jox.Parsing
{
    public static class Parser
    {
        private static List<IStmt> statements = new List<IStmt>();
        private static List<Token> tokens;
        private static int currentIndex;

        public static List<IStmt> Parse(List<Token> tokens)
        {
            Parser.statements.Clear();
            Parser.tokens = tokens;
            currentIndex = 0;

            while (!AtEof())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private static IStmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch
            {
                SynchronizeState();
                return null;
            }
        }

        private static IStmt VarDeclaration()
        {
            Token name = EatExpectedToken(TokenType.IDENTIFIER, "Expected variable name.");

            IExpr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            EatExpectedToken(TokenType.SEMICOLON, "Expected ';' after variable declaration.");
            return new IStmt.Var(name, initializer);
        }

        private static IStmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.LEFT_BRACE)) return new IStmt.Block(BlockStatement());
            return ExpressionStatement();
        }

        private static List<IStmt> BlockStatement()
        {
            var statements = new List<IStmt>();

            while (!CheckPeek(TokenType.RIGHT_BRACE) && !AtEof())
                statements.Add(Declaration());

            EatExpectedToken(TokenType.RIGHT_BRACE, "Expected '}' after block.");
            return statements;
        }

        private static IStmt ExpressionStatement()
        {
            IExpr expr = Expression();
            EatExpectedToken(TokenType.SEMICOLON, "Expected ';' after expression.");
            return new IStmt.Expression(expr);
        }

        private static IStmt PrintStatement()
        {
            IExpr value = Expression();
            EatExpectedToken(TokenType.SEMICOLON, "Expected ';' after value.");
            return new IStmt.Print(value);
        }

        #region Helper Methods

        private static bool Match(params TokenType[] types)
        {
            foreach (TokenType t in types)
            {
                if (CheckPeek(t))
                {
                    _ = EatToken();
                    return true;
                }
            }

            return false;
        }

        private static Token EatToken()
        {
            if (!AtEof()) currentIndex++;
            return PreviousToken();
        }

        private static bool CheckPeek(TokenType type)
        {
            if (AtEof()) return false;
            return Peek().type == type;
        }

        private static Token Peek()
        {
            return tokens[currentIndex];
        }

        private static Token PreviousToken()
        {
            return tokens[currentIndex - 1];
        }

        private static bool AtEof()
        {
            //return currentIndex >= tokens.Count - 1;
            return Peek().type == TokenType.EOF;
        }

        #endregion

        #region Expressions

        private static IExpr Expression() => Assignment();
        
        private static IExpr Assignment()
        {
            IExpr expr = Equality();


            if (Match(TokenType.EQUAL))
            {
                Token equals = PreviousToken();
                IExpr value  = Assignment();

                if(expr is IExpr.Variable var)
                {
                    return new IExpr.Assign(var.ident, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private static IExpr Equality()
        {
            IExpr expr = Comparison();

            while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
                expr = new IExpr.Binary(expr, PreviousToken(), Comparison());

            return expr;
        }

        private static IExpr Comparison()
        {
            IExpr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL)) 
                expr = new IExpr.Binary(expr, PreviousToken(), Term());
                

            return expr;
        }

        private static IExpr Term()
        {
            IExpr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
                expr = new IExpr.Binary(expr, PreviousToken(), Factor());

            return expr;
        }

        private static IExpr Factor()
        {
            IExpr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
                expr = new IExpr.Binary(expr, PreviousToken(), Unary());

            return expr;
        }

        private static IExpr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
                return new IExpr.Unary(PreviousToken(), Unary());
            
            return Primary();
        }

        private static IExpr Primary()
        {
            if (Match(TokenType.TRUE))  return new IExpr.Literal(true);
            if (Match(TokenType.FALSE)) return new IExpr.Literal(false);
            if (Match(TokenType.NIL))   return new IExpr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) return new IExpr.Literal(PreviousToken().literal);
            if (Match(TokenType.IDENTIFIER)) return new IExpr.Variable(PreviousToken());

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                EatExpectedToken(TokenType.RIGHT_PAREN, "Expected ')' after expression");
                return new IExpr.Grouping(expr);
            }

            //  Error Productions for Binary operators missing a left-hand operand
            //  @Refactor: there's definitely a better way of doing this

            if (Match(
                TokenType.BANG_EQUAL,
                TokenType.EQUAL, TokenType.EQUAL_EQUAL,
                TokenType.GREATER, TokenType.GREATER_EQUAL,
                TokenType.LESS, TokenType.LESS_EQUAL,
                TokenType.MINUS, TokenType.PLUS,
                TokenType.SLASH, TokenType.STAR))
            {
                Error(PreviousToken(), "Binary Operator is missing a left-hand operand");
                return Expression();
            }

            throw Error(Peek(), "Expected an Expression.");
        }

        #endregion

        private static Token EatExpectedToken(TokenType type, string errorMessage)
        {
            if (CheckPeek(type)) return EatToken();

            throw Error(Peek(), errorMessage);
        }

        private static ParseError Error(Token token, string message)
        {
            Jox.ParseError(token, message);
            return new ParseError();
        }

        private static void SynchronizeState()
        {
            _ = EatToken();

            while (!AtEof())
            {
                if (PreviousToken().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                _ = EatToken();
            }
        }
    }
}
