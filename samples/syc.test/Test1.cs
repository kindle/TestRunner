using System.Diagnostics;

namespace syc.test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        [Owner("Bailin")]
        [Priority(0)]
        [Description("Checks that the RIM and SYCCPD processes are running.")]
        public void RIM_SYC_Processes_Check()
        {
            string[] requiredProcesses = 
                { "RIM", "SYCCPD1", "SYCCPD2", "SYCCPD3", "SYCCPD4" };
            string missingProcesses = string.Empty;

            foreach (string processName in requiredProcesses)
            {
                if (Process.GetProcessesByName(processName).Length == 0)
                {
                    missingProcesses += missingProcesses.Length == 0 ? processName : ", " + processName;
                }
            }

            Assert.IsTrue(missingProcesses.Length == 0, "The following required processes are not running: " + missingProcesses + ".");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(1)]
        [Description("Tests that string comparison returns equal.")]
        public void StringComparison_ReturnsEqual()
        {
            Assert.AreEqual("zerocurve", "zero" + "curve");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(2)]
        [Description("Tests an intentional failure scenario.")]
        public void IntentionalFailure_ReturnsFailure()
        {
            Assert.AreEqual(5, 2 + 2);
        }
    }
}
