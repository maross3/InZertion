using InZertion.Lexing;
namespace InZertion.Syntax;
internal abstract class Expr
{
    internal Token name;

    protected Expr(Token name)
    {
        this.name = name;
    }

    protected Expr()
    {
    }

    internal abstract T Accept<T>(IVisitor<T> visitor);

    internal interface IVisitor<out T>
    {
        T VisitAssignExpr(Assign expr);
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
        T VisitVariableExpr(Variable expr);
        T VisitLogicalExpr(Logical logical);
        T VisitCallExpr(Call fn);
    }

    public class Assign : Expr
    {
        internal readonly Expr value;

        public Assign(Token name, Expr value) : base(name)
        {
            this.value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssignExpr(this);
        }
    }

    public class Binary : Expr
    {
        internal readonly Expr left;
        internal readonly Token op;
        internal readonly Expr right;

        internal Binary(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class Logical : Expr
    {
        internal readonly Expr left;
        internal readonly Token op;
        internal readonly Expr right;

        internal Logical(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitLogicalExpr(this);
    }

    public class Grouping : Expr
    {
        internal readonly Expr expression;

        internal Grouping(Expr expression)
        {
            this.expression = expression;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Literal : Expr
    {
        internal readonly object value;

        internal Literal(object value)
        {
            this.value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class Unary : Expr
    {
        internal readonly Token op;
        internal readonly Expr right;

        internal Unary(Token op, Expr right)
        {
            this.op = op;
            this.right = right;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class Variable : Expr
    {
        internal Variable(Token name)
        {
            this.name = name;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
    }

    public class Call : Expr
    {
        internal Expr callee;
        internal Token paren;
        internal List<Expr> args;

        public Call(Expr callee, Token paren, List<Expr> args)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitCallExpr(this);
        }
    }
}