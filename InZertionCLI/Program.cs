using InZertion.Interpreting;
using InZertion.Lexing;
using InZertion.Syntax;

public static class Program
{
    private static readonly DirectoryInfo _SDir = TryGetSolutionDirectoryInfo();
    private static Scanner _SScanner;

    static void Main()
    {
        Console.WriteLine("Test Script Name to Run:");
        var userChoice = Console.ReadLine();

        if (userChoice?.ToLower() == "repl")
            Console.WriteLine("Repl coming soon-ish.");
        else
        {
            var args = userChoice.Split(" ");
            if (args.Length > 0)
            {
                if (args[^1] == "scan") ScanVerbose(args[0].ToLower());
                if (args[^1] == "parse") ParseVerbose(args[0].ToLower());
            }
            else
            {
                
            }

        }
    }

    private static void ScanVerbose(string script)
    {
            try
            {
                var tokenList = ScanTestScript(script);
                Console.WriteLine();
                Console.WriteLine();
                
                Console.WriteLine(@"==========*/ Scanning successful! \*==========");
                Console.WriteLine("               Token Results                   ");
                Console.WriteLine("==============================================\n");
                
                foreach (var tkn in tokenList)
                    Console.WriteLine(tkn);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: \n" + e.Message);
            }
    }

    private static void ParseVerbose(string script)
    {
        try
        {
            var stmtList = ParseTestScript(script);

            Console.WriteLine(@"==========*/ Parsing successful!  \*==========");
            Console.WriteLine("                Stmt  Results                   ");
            Console.WriteLine("==============================================\n");
            foreach (var stmt in stmtList)
                Console.WriteLine(stmt);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: \n" + e.Message);
        }
    }
    
    public static string FetchTestScript(string scriptToFetch) =>
        _SDir + @"\InZertionCLI\TestScripts\" + scriptToFetch;

    public static List<Token> ScanTestScript(string toScanScript)
    {
        var text = File.ReadAllText(FetchTestScript(toScanScript));
        _SScanner = new Scanner(text);
        return _SScanner.ScanTokens();
    }

    private static List<Stmt> ParseTestScript(string script)
    {
        var tokens = ScanTestScript(script);
        var parser = new Parser(tokens);
        return parser.Parse();
    }
    
    private static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
    {
        var directory = new DirectoryInfo(
            currentPath ?? Directory.GetCurrentDirectory());

        while (directory != null && !directory.GetFiles("*.sln").Any())
            directory = directory.Parent;
        
        return directory;
    }
}