namespace vah2w
{
    [TestClass]
    public sealed class Test1
    {

        [TestMethod]
        [Owner("Bailin")]
        [Priority(0)]
        [Description("Checks that the VAH crash dump folder contains no crash dump files.")]
        public void CrashDumpFolder_ContainsNoDumpFiles()
        {
            const string crashDumpFolder = @"D:\ThomsonReuters\VAH\logs\crashdumps";
            string[] crashDumpFiles = Directory.GetFiles(crashDumpFolder, "*.dmp", SearchOption.AllDirectories);

            Assert.AreEqual(0, crashDumpFiles.Length,
                $"Crash dump files found:{Environment.NewLine}{string.Join(Environment.NewLine, crashDumpFiles)}");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(0)]
        [Description("Checks that the Adfin Bin folder contains DLL files.")]
        public void AdfinBinFolder_ContainsDllFiles()
        {
            const string adfinBinFolder = @"D:\ThomsonReuters\VAH\3rdParty\Adfin\Bin";
            string[] dllFiles = Directory.GetFiles(adfinBinFolder, "*.dll");

            Assert.IsTrue(dllFiles.Length > 0, $"No DLL files found in {adfinBinFolder}.");
        }
    }
}
