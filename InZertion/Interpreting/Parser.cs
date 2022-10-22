using InZertion.Lexing;
using InZertion.Syntax;

namespace InZertion.Interpreting;

public class Parser
{
    private class ParseError : Exception
    {
    }

    private readonly List<Token> _tokens;
    private int _current;

    public Parser(List<Token> tokens) =>
        _tokens = tokens;


    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.Function)) return Function("function");
            return Match(TokenType.Var) ? VarDeclaration() : Statement();
        }
        catch (ParseError error)
        {
            Synchronize();
            return null;
        }
    }

    private Expr Expression() =>
        Assignment();

    private Expr Assignment()
    {
        var expr = Or();
        if (!Match(TokenType.Equal) && !IsMathEquals())
            return expr;

        var equals = Previous();

        if (IsMathEquals(equals.type))
            return MathEquals(expr, equals);
                
        var value = Assignment();
        if (expr is Expr.Variable variable)
        {
            var name = variable.name;
            return new Expr.Assign(name, value);
        }

        Error(equals, "Invalid assignment target.");
        return expr;
    }

    private bool IsMathEquals(TokenType token) =>
        token is TokenType.StarEqual or
            TokenType.SlashEqual or
            TokenType.PlusEqual or
            TokenType.MinusEqual;

    private bool IsMathEquals() =>
        Match(TokenType.StarEqual) ||
        Match(TokenType.SlashEqual) ||
        Match(TokenType.PlusEqual) ||
        Match(TokenType.MinusEqual);

    private Stmt.Function Function(string kind)
    {
        var name = Consume(TokenType.Identifier,
            $"Expected {kind} + name.");
        Consume(TokenType.LeftParen,
            "Expected a '(' after function name");
        var parameters = new List<Token>();

        if (Check(TokenType.Identifier))
            parameters.Add(Consume(TokenType.Identifier,
                @"Invalid parameter found!"));

        while (Match(TokenType.Comma))
        {
            if (Check(TokenType.RightParen)) break;

            if (parameters.Count >= 255)
            {
                Error(Peek(),
                    "Can't have more than 255 parameters.");
            }
            else
            {
                parameters.Add(Consume(TokenType.Identifier,
                    "Expected a parameter name."));
            }
        }

        Consume(TokenType.RightParen,
            "Expected a ')' after parameters.");
        Consume(TokenType.LeftParen,
            "Expected a '{' before " + kind + " body.");
        var body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private Expr MathEquals(Expr expr, Token equals)
    {
        if (expr is Expr.Variable newVar)
            return new Expr.Assign(newVar.name,
                new Expr.Binary(newVar, equals, Term()));
        return expr;
    }

    private Expr Or()
    {
        var expr = And();
        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Break)) return BreakStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.While)) return WhileStatement();
        return Match(TokenType.LeftBrace) ? new Stmt.Block(Block()) : ExpressionStatement();
    }

    // check functionality of this.
    // I don't think this will break out of just one scope
    // but rather exit a function. Did this for notes
    private Stmt BreakStatement()
    {
        var keyword = Previous();
        Consume(TokenType.Semicolon,
            "Expected a ';' after return value.");
        return new Stmt.Return(keyword, null);
    }

    private Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr value = null;

        if (!Check(TokenType.Semicolon))
            value = Expression();

        Consume(TokenType.Semicolon,
            "Expected a ';' after return value.");

        return new Stmt.Return(keyword, value);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expected a '(' after for.");

        Stmt initializer;
        if (Match(TokenType.Semicolon)) initializer = null;
        else if (Match(TokenType.Var)) initializer = VarDeclaration();
        else initializer = ExpressionStatement();

        Expr condition = null;
        if (!Check(TokenType.Semicolon)) condition = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after loop condition.");

        Expr increment = null;
        if (!Check(TokenType.RightParen)) increment = Expression();
        Consume(TokenType.RightParen, "Expected ')' after for clauses.");

        var body = Statement();

        if (increment == null) return body;
        var bodyList = new List<Stmt>
        {
            body,
            new Stmt.Expression(increment)
        };
        body = new Stmt.Block(bodyList);

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer == null) return body;
        var nextBody = new List<Stmt>
        {
            initializer,
            body
        };
        body = new Stmt.Block(nextBody);

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after if.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expected a ')' after condition expression.");

        var thenBranch = Statement();

        Stmt elseBranch = null;
        if (Match(TokenType.Else))
            elseBranch = Statement();

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value");
        return new Stmt.Print(value);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen,
            "Expected an exit condition after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen,
            "Expected a ')' after condition expression.");
        var body = Statement();
        return new Stmt.While(condition, body);
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected a variable name");

        Expr initializer = null;

        if (Match(TokenType.Equal))
            initializer = Expression();

        Consume(TokenType.Semicolon, "Expected ';' after a variable declaration");
        return new Stmt.Var(name, initializer);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after an expression");
        return new Stmt.Expression(expr);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
            statements.Add(Declaration());

        Consume(TokenType.RightBrace, "Expected '}' after block");
        return statements;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        // will always return comparison
        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Slash, TokenType.Star))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
                expr = FinishCall(expr);
            else break;
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var args = new List<Expr>();

        if (args.Count > 255)
            Error(Peek(),
                "Maximum supported function arguments is 255.");

        if (!Check(TokenType.RightParen))
        {
            args.Add(Expression());
            // todo, this is where the match messes up on second fib
            while (Match(TokenType.Comma))
                args.Add(Expression());
        }

        var paren = Consume(TokenType.RightParen,
            "Expected a ')' after function arguments.");

        return new Expr.Call(callee, paren, args);
    }

    private Expr Unary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus))
            return Call();

        var op = Previous();
        var right = Unary();

        return new Expr.Unary(op, right);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Null)) return new Expr.Literal(null);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(TokenType.Identifier))
        {
            return new Expr.Variable(Previous());
        }

        if (!Match(TokenType.LeftParen)) throw Error(Peek(), "Expected expression");

        var expr = Expression();

        Consume(TokenType.RightParen, "Expect ')' after expression.");
        return new Expr.Grouping(expr);
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check)) return false;

        Advance();
        return true;
    }

    private Token Consume(TokenType type, String message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() =>
        Peek().type == TokenType.Eof;


    private Token Peek() =>
        _tokens[_current];


    private Token Previous() =>
        _tokens[_current - 1];


    private static ParseError Error(Token token, string message)
    {
        InZertionLogger.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.Semicolon) return;

            switch (Peek().type)
            {
                case TokenType.Class:
                case TokenType.Function:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }
}