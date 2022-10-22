namespace InZertion.Lexing;

public enum TokenType
{
    // single character tokens
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Comma,
    Dot,
    Minus,
    Plus,
    Semicolon,
    Slash,
    Star,

    // one or two character tokens
    Bang,
    BangEqual,
    Equal,
    EqualEqual,
    Greater,
    GreaterEqual,
    Less,
    LessEqual,
        
    // MathEquals 
    PlusEqual,
    MinusEqual,
    SlashEqual,
    StarEqual,

    // literals, add more
    Identifier,
    String,
    Number,

    //keywords
    And,
    Break,
    Class,
    Else,
    False,
    Function,
    For,
    If,
    Null,
    Or,
    Print,
    Return,
    Super,
    This,
    True,
    While,
    
    // dynamic stack
    Var,
    Dyna,
    
    // user heap
    Data,
    
    // types
    Array,
    Stack,
    Queue,
    
    
    Eof
}