namespace InZertion;

public class Token
{
    internal string lexeme { get; }
    internal int line { get; }
    internal object literal { get; }
    internal TokenType type { get; }

    internal Token(TokenType type, string lexeme, object literal, int line)
    {
        this.type = type;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
    }

    public override string ToString() => 
        type + " " + lexeme + " " + literal;
    
    public void Destroy()
    {
        // deallocate from stack/heap
    }
}