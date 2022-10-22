namespace InZertion.Lexing;

public class Scanner
{
    private static Dictionary<string, TokenType> _keywords;
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private int _current;
    private int _line = 1;
    private int _start;

    public Scanner(string source)
    {
        _source = source;


        _keywords = new Dictionary<string, TokenType>
        {
            {"and", TokenType.And},
            {"class", TokenType.Class},
            {"dyna", TokenType.Dyna},
            {"data", TokenType.Data},
            {"else", TokenType.Else},
            {"false", TokenType.False},
            {"for", TokenType.For},
            {"function", TokenType.Function},
            {"if", TokenType.If},
            {"null", TokenType.Null},
            {"or", TokenType.Or},
            {"print", TokenType.Print},
            {"return", TokenType.Return},
            {"super", TokenType.Super},
            {"this", TokenType.This},
            {"true", TokenType.True},
            {"var", TokenType.Var},
            {"while", TokenType.While}
        };
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();

        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(Match('=') ? TokenType.MinusEqual : TokenType.Minus);
                break;
            case '+':
                AddToken(Match('=') ? TokenType.PlusEqual : TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(Match('=') ? TokenType.StarEqual : TokenType.Star);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/') || Match('*'))
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                else if (Match('='))
                    AddToken(TokenType.SlashEqual);
                else
                    AddToken(TokenType.Slash);
                break;
            case '&':
                if (Match('&'))
                    AddToken(TokenType.And);
                break;
            case '|':
                if (Match('|'))
                    AddToken(TokenType.Or);
                break;
            case ' ':
            case '\r':
            case '\t':
                // ignore whitespace
                break;
            case '\n':
                _line++;
                break;
            case '"':
                IsString();
                break;

            default:
                if (IsDigit(c)) Number();
                else if (IsAlpha(c)) Identifier();
                else InZertionLogger.Error(_line, "Unexpected character: " + c);

                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var text = _source.Substring(_start, _current - _start);

        TokenType type;
        try
        {
            type = _keywords[text];
        }
        catch
        {
            type = TokenType.Identifier;
        }

        AddToken(type);
    }

    private static bool IsAlpha(char c) =>
        c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '-';

    private static bool IsAlphaNumeric(char c) =>
        IsAlpha(c) || IsDigit(c);

    private bool IsAtEnd() =>
        _current >= _source.Length;

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        var doubleStr = _source.Substring(_start, _current - _start);
        AddToken(TokenType.Number,
            Convert.ToDouble(doubleStr));
    }

    private void IsString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        // todo test unterminated string error
        if (IsAtEnd())
        {
            InZertionLogger.Error(_line, "Unterminated string found!");
            return;
        }

        Advance();

        var value = _source.Substring(_start + 1, _current - 1 - _start - 1);

        AddToken(TokenType.String, value);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek() =>
        IsAtEnd() ? '\0' : _source[_current];

    private char PeekNext() =>
        _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

    private static bool IsDigit(char c) =>
        c is >= '0' and <= '9';

    private char Advance() =>
        _source[_current++];

    private void AddToken(TokenType type) =>
        AddToken(type, null);

    private void AddToken(TokenType type, object literal)
    {
        var text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }
}