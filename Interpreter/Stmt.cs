namespace Basic.Interpreter
{
    internal abstract class Stmt
    {

        private static Stmt.End _end = new Stmt.End();
        public int LineNumber { get; set; }
        public Stmt NextStatement { get; private set; } = _end;

        public interface Visitor<T>
        {
            T VisitAssignStmt (Assign stmt);
            T VisitDimStmt (Dim stmt);
            T VisitForStmt (For stmt);
            T VisitGotoStmt (Goto stmt);
            T VisitGosubStmt (Gosub stmt);
            T VisitIfStmt (If stmt);
            T VisitNewStmt (New stmt);
            T VisitNextStmt (Next stmt);
            T VisitOnStmt (On stmt);
            T VisitPrintStmt (Print stmt);
            T VisitReadStmt (Read stmt);
            T VisitReturnStmt (Return stmt);
            T VisitRunStmt (Run stmt);
            T VisitLetStmt (Let stmt);
            T VisitNoopStmt (Noop stmt);
            T VisitEndStmt (End stmt);
            T VisitUserStmt (User stmt);
        }

        public class Assign : Stmt
        {

            public Expr.Variable Variable { get; }
            public Expr Value { get; }

            public Assign (Expr.Variable variable, Expr value)
            {
                Variable = variable;
                Value = value;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitAssignStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Dim : Stmt
        {

            public List<Expr.Variable> Variable { get; }

            public Dim (List<Expr.Variable> variable)
            {
                Variable = variable;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitDimStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class For : Stmt
        {

            public Token Stmt { get; }
            public Expr.Variable Variable { get; }
            public Expr Start { get; }
            public Expr Target { get; }
            public Expr Step { get; }
            public List<Stmt> Block { get; }

            public For (Token stmt, Expr.Variable variable, Expr start, Expr target, Expr step, List<Stmt> block)
            {
                Stmt = stmt;
                Variable = variable;
                Start = start;
                Target = target;
                Step = step;
                Block = block;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitForStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Goto : Stmt
        {

            public Token Stmt { get; }
            public Expr Label { get; }

            public Goto (Token stmt, Expr label)
            {
                Stmt = stmt;
                Label = label;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGotoStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Gosub : Stmt
        {

            public Token Stmt { get; }
            public Expr Label { get; }

            public Gosub (Token stmt, Expr label)
            {
                Stmt = stmt;
                Label = label;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGosubStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class If : Stmt
        {

            public Expr Condition { get; }
            public List<Stmt> IfTrue { get; }
            public List<Stmt> IfFalse { get; }

            public If (Expr condition, List<Stmt> ifTrue, List<Stmt> ifFalse)
            {
                Condition = condition;
                IfTrue = ifTrue;
                IfFalse = ifFalse;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
                if (IfTrue.Any()) IfTrue.Last().SetNextStatement(nextStatement);
                if (IfFalse.Any()) IfFalse.Last().SetNextStatement(nextStatement);
            }

        }

        public class New : Stmt
        {


            public New ()
            {
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitNewStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Next : Stmt
        {

            public Stmt.For ForStatement { get; }

            public Next (Stmt.For forStatement)
            {
                ForStatement = forStatement;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitNextStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class On : Stmt
        {

            public Token Name { get; }
            public Expr Expression { get; }
            public Token Keyword { get; }
            public List<Expr> Targets { get; }

            public On (Token name, Expr expression, Token keyword, List<Expr> targets)
            {
                Name = name;
                Expression = expression;
                Keyword = keyword;
                Targets = targets;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitOnStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Print : Stmt
        {

            public PrintUserStatement UserStatement { get; }
            public Token Stmt { get; }
            public List<Expr> Expressions { get; }
            public bool NewLine { get; }

            public Print (PrintUserStatement userStatement, Token stmt, List<Expr> expressions, bool newLine)
            {
                UserStatement = userStatement;
                Stmt = stmt;
                Expressions = expressions;
                NewLine = newLine;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Read : Stmt
        {

            public List<Expr.Variable> Variables { get; }

            public Read (List<Expr.Variable> variables)
            {
                Variables = variables;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitReadStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Return : Stmt
        {

            public Token Stmt { get; }

            public Return (Token stmt)
            {
                Stmt = stmt;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Run : Stmt
        {


            public Run ()
            {
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitRunStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Let : Stmt
        {

            public Token Name { get; }
            public Expr Initializer { get; }

            public Let (Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLetStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class Noop : Stmt
        {


            public Noop ()
            {
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitNoopStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class End : Stmt
        {


            public End ()
            {
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitEndStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public class User : Stmt
        {

            public Token Name { get; }
            public GenericUserStatement UserStatement { get; }
            public int Pattern { get; }
            public List<Expr> Expressions { get; }

            public User (Token name, GenericUserStatement userStatement, int pattern, List<Expr> expressions)
            {
                Name = name;
                UserStatement = userStatement;
                Pattern = pattern;
                Expressions = expressions;
            }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitUserStmt(this);
            }

            public override void SetNextStatement(Stmt nextStatement)
            {
                NextStatement = nextStatement;
            }

        }

        public abstract T Accept<T>(Visitor<T> visitor);
        public abstract void SetNextStatement(Stmt nextStatement);

    }
}
