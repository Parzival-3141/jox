using System;
using System.Collections.Generic;
using Jox.Parsing.AST;

namespace Jox.Parsing
{
    public static class Parser
    {
        private static List<Token> tokens;
        private static int currentIndex;

        public static IExpr Parse(List<Token> tokens)
        {
            Parser.tokens = tokens;
            currentIndex = 0;

            try
            {
                return Expression();
            }
            catch (ParseError)
            {
                return null;
            }


        }

        #region Helper Methods

        private static bool Match(params TokenType[] types)
        {
            foreach(TokenType t in types)
            {
                if (CheckPeek(t))
                {
                    EatToken();
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
            return currentIndex >= tokens.Count;
        }

        #endregion

        #region Expressions

        private static IExpr Expression() => Equality();
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

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression");
                return new IExpr.Grouping(expr);
            }

            //  Error Productions for Binary operators missing a left-hand operand
            //  @Refactor: there's definitely a better way of doing this

            if(Match(
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

        private static Token Consume(TokenType type, string message)
        {
            if (CheckPeek(type)) return EatToken();

            throw Error(Peek(), message);
        }

        private static ParseError Error(Token token, string message)
        {
            Jox.Error(token, message);
            return new ParseError();
        }

        private static void SynchronizeState()
        {
            _ = EatToken();

            while (!AtEof())
            {
                if (PreviousToken().type == TokenType.SEMICOLON) return; //@Refactor: remove redundant EatToken and PreviousToken

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
