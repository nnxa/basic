namespace Basic.Interpreter
{
    internal abstract class Expr
    {

        public interface Visitor<T>
        {
            T VisitBinaryExpr (Binary expr);
            T VisitGroupingExpr (Grouping expr);
            T VisitLiteralExpr (Literal expr);
            T VisitUnaryExpr (Unary expr);
            T VisitVariableExpr (Variable expr);
        }

        public class Binary : Expr
        {

            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }

            public Binary (Expr left, Token @operator, Expr right)
            {
                Left = left;
                Operator = @operator;
                Right = right;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

        }

        public class Grouping : Expr
        {

            public Expr Expression { get; }

            public Grouping (Expr expression)
            {
                Expression = expression;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

        }

        public class Literal : Expr
        {

            public object Value { get; }

            public Literal (object value)
            {
                Value = value;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

        }

        public class Unary : Expr
        {

            public Token Operator { get; }
            public Expr Right { get; }

            public Unary (Token @operator, Expr right)
            {
                Operator = @operator;
                Right = right;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

        }

        public class Variable : Expr
        {

            public Token Name { get; }
            public List<Expr> Parameters { get; }

            public Variable (Token name, List<Expr> parameters)
            {
                Name = name;
                Parameters = parameters;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

        }

        public abstract T Accept<T>(Visitor<T> visitor);

    }
}
