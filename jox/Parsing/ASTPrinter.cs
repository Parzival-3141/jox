using System;
using Jox.Parsing.AST;

namespace Jox.Parsing
{
    public class ASTPrinter : IExpr.IVisitor<string>
    {
        public void RunTest()
        {
            var expression = new IExpr.Binary(
                new IExpr.Unary(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new IExpr.Literal(123)),

                new Token(TokenType.STAR, "*", null, 1),
                
                new IExpr.Grouping(
                    new IExpr.Literal(45.67)));

            Print(expression);
        }

        public void Print(IExpr expr) => Console.WriteLine(expr.Accept(this));

        private string Parenthesize(string name, params IExpr[] exprs)
        {
            string result = "(" + name;
            
            foreach(var ex in exprs)
            {
                result += " " + ex.Accept(this);
            }

            result += ")";
            return result;
        }

        public string VisitBinaryExpr(IExpr.Binary expr)
        {
            return Parenthesize(expr.@operator.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(IExpr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(IExpr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        public string VisitUnaryExpr(IExpr.Unary expr)
        {
            return Parenthesize(expr.@operator.lexeme, expr.right);
        }

        public string VisitVariableExpr(IExpr.Variable expr)
        {
            return "var " + expr.ident.lexeme;
        }

        public string VisitAssignExpr(IExpr.Assign expr)
        {
            return Parenthesize("assign " + expr.ident.lexeme, expr.value);
        }
    }
}
