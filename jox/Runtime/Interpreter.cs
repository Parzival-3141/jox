using Jox.Parsing;
using Jox.Parsing.AST;
using System.Linq;

namespace Jox.Runtime
{
    public sealed class Interpreter : IExpr.IVisitor<object>
    {
        public void Interpret(IExpr expression)
        {
            try
            {
                object value = Evaluate(expression);
                System.Console.WriteLine(Stringify(value));
            }
            catch(RuntimeError error)
            {
                Jox.RuntimeError(error);
            }
        }

        private static string Stringify(object obj)
        {
            if (obj == null) return "nil";
            if (obj is double dub)
            {
                string text = dub.ToString();
                if (text.EndsWith(".0"))
                    text = text[0..^2];

                return text;
            }

            return obj.ToString();
        }
        
        #region Visitor Implementation

        public object VisitBinaryExpr(IExpr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.@operator.type)
            {
                //  Equality Ops
                case TokenType.BANG_EQUAL: return !Equals(left, right);
                case TokenType.EQUAL_EQUAL: return Equals(left, right);

                //  Comparison Ops
                case TokenType.GREATER:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left > (double)right;
                
                case TokenType.GREATER_EQUAL:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left >= (double)right;
                
                case TokenType.LESS:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left < (double)right;
                
                case TokenType.LESS_EQUAL:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left <= (double)right;

                //  Arithmetic Ops
                case TokenType.MINUS:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left - (double)right;
                
                case TokenType.SLASH:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    if ((double)right == 0d) throw new RuntimeError(expr.@operator, "Division by zero.");

                    return (double)left / (double)right;
                
                case TokenType.STAR:
                    CheckOperandsAreNumbers(expr.@operator, left, right);
                    return (double)left * (double)right;

                case TokenType.PLUS:
                    if (left is double ld && right is double rd) return ld + rd;
                    if (left is string ls && right is string rs) return ls + rs;
                    throw new RuntimeError(expr.@operator, "Operands must be two numbers or two strings.");

                default:
                    return null;
            }
        }

        public object VisitUnaryExpr(IExpr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.@operator.type)
            {
                case TokenType.MINUS:
                    CheckOperandIsNumber(expr.@operator, right);
                    return -(double)right;

                case TokenType.BANG:
                    return !IsTruthy(right);

                default: 
                    return null;
            }
        }

        public object VisitGroupingExpr(IExpr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(IExpr.Literal expr)
        {
            return expr.value;
        }

        private object Evaluate(IExpr expr)
        {
            return expr.Accept(this);
        }

        private static bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool b) return b;
            return true;
        }

        private static void CheckOperandIsNumber(Token @operator, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private static void CheckOperandsAreNumbers(Token @operator, object operandA, object operandB)
        {
            if (operandA is double && operandB is double) return;
            throw new RuntimeError(@operator, "Operands must be numbers.");
        }

        #endregion
    }
}
