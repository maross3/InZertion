using InZertion;

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
            try
            {
                var tokenList = ScanTestScript(userChoice?.ToLower());
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
    }

    public static string FetchTestScript(string scriptToFetch) =>
        _SDir + @"\InZertionCLI\TestScripts\" + scriptToFetch;

    public static List<Token> ScanTestScript(string toScanScript)
    {
        var text = File.ReadAllText(FetchTestScript(toScanScript));
        _SScanner = new Scanner(text);
        return _SScanner.ScanTokens();
    }

    private static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
    {
        var directory = new DirectoryInfo(
            currentPath ?? Directory.GetCurrentDirectory());

        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory;
    }
}