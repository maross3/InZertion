namespace InZertionTests;

[TestClass]
public class UnitTest1
{
    [TestMethod] // fetching script
    public void ReadAllText_Test()
    {
        var text = Program.FetchTestScript(@"ScannerScript.izr");   
        Assert.IsNotNull(text);
    }

    [TestMethod] // testing scanner count functionality
    public void ScannerTokenListCount_Test()
    {
        var tokenList = Program.ScanTestScript(@"ScannerScript.izr");
        Assert.IsTrue(tokenList.Count == 16);
    }
    
    [TestMethod] // todo test scanner effectiveness
    public void ScannerAccuracy_Test()
    {
        
    }
}