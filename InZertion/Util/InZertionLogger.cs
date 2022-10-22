using InZertion.Lexing;

namespace InZertion;

internal class InZertionLogger
{
    public static void Error(int line, string unterminatedStringFound)
    {
        // report line errors here
    }

    public static void Error(Token token, string unterminatedStringFound)
    {
        // report token errors here
    }
}