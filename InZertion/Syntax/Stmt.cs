using InZertion.Lexing;
namespace InZertion.Syntax;

// todo contemplate pro/con of Stmt/Expr being typed records
public abstract class Stmt
{
    internal abstract T Accept<T>(IVisitor<T> visitor);

    internal interface IVisitor<out T>
    {
        T VisitBlockStmt(Block stmt);
        T VisitExpressionStmt(Expression stmt);
        T VisitIfStatement(If stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitWhileStmt(While stmt);
        T VisitFunctionStmt(Function stmt);
        T VisitReturnStmt(Return stmt);
    }

    public class Block : Stmt
    {
        internal List<Stmt> statements;

        internal Block(List<Stmt> statements) =>
            this.statements = statements;


        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitBlockStmt(this);
    }

    public class Expression : Stmt
    {
        internal Expr expression;

        internal Expression(Expr expression) =>
            this.expression = expression;

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitExpressionStmt(this);
    }

    public class If : Stmt
    {
        internal readonly Expr condition;
        internal readonly Stmt thenBranch;
        internal readonly Stmt elseBranch;

        internal If(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitIfStatement(this);
    }

    public class Print : Stmt
    {
        internal Expr expression;

        internal Print(Expr expression) =>
            this.expression = expression;

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitPrintStmt(this);
    }

    public class Var : Stmt
    {
        internal Expr initializer;
        internal Token name;

        internal Var(Token name, Expr initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitVarStmt(this);
    }

    public class While : Stmt
    {
        internal Expr condition;
        internal Stmt body;

        internal While(Expr condition, Stmt body)
        {
            this.condition = condition;
            this.body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitWhileStmt(this);
    }

    public class Function : Stmt
    {
        internal List<Stmt> body;
        internal List<Token> parameters;
        internal Token name;

        internal Function(Token name, List<Token> parameters, List<Stmt> body)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
        }

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFunctionStmt(this);
        }
    }

    public class Return : Stmt
    {
        internal Token keyword;
        internal Expr value;

        internal Return(Token keyword, Expr value)
        {
            this.keyword = keyword;
            this.value = value;
        }

        internal override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitReturnStmt(this);
    }
}