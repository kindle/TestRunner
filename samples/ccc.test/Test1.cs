using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ccc.test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        [Owner("Bailin")]
        [Priority(0)]
        [Description("Checks that the RIM, CCCCPD, and CHR processes are running.")]
        public void RIM_CCC_CHR_Processes_Check()
        {
            string[] requiredProcesses = { "RIM", "CCCCPD", "CHR" };
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
        [Description("Checks today's CCCPD log for a successful CDSC calculation and bond scan.")]
        public void Calculation_Log_Check()
        {
            DateTime now = DateTime.Now;

            Assert.IsFalse(
                now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday,
                "Sun and Sat Ignore: CDSEODCONTROL=R updated on Saturday or Sunday.");

            string logPath = Path.Combine(
                @"D:\CCC\LOG",
                "ccccpd_" + now.ToString("ddd", CultureInfo.InvariantCulture) + ".log");

            Assert.IsTrue(File.Exists(logPath), "Log not found: " + logPath);

            string[] logLines = ReadAllLinesShared(logPath);

            Assert.IsTrue(
                Array.Exists(logLines, line => line.Contains("Launch Calculation of CDSC")),
                "Calculation not triggered due to no control RIC updated; please check whether CDSEODCONTROL=R has an update.");

            Assert.IsTrue(
                Array.Exists(logLines, line => line.Contains("Scanning chains")),
                "Chain scanning log entry not found.");

            Match bondMatch = Array.FindAll(logLines, line => line.Contains("FOUND"))
                .Select(line => Regex.Match(line, @"FOUND\s+(\d+)\s+bonds, chain pairs"))
                .FirstOrDefault(match => match.Success);

            Assert.IsNotNull(bondMatch, "Bond count log entry not found.");

            int bondCount = int.Parse(bondMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            Assert.IsTrue(bondCount > 75000, "Bond count was " + bondCount + "; expected more than 75000. Please check the ADS watchlist.");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(2)]
        [Description("Checks that the CCC timer file exists and contains a timestamp within five minutes of the current time.")]
        public void CDS_Timer_File_Check()
        {
            const string timerPath = @"D:\CDS\DB\timer.txt";

            Assert.IsTrue(File.Exists(timerPath), "Timer file not found: " + timerPath);

            string timerValue = File.ReadAllText(timerPath).Trim();
            DateTime timerTime;

            Assert.IsTrue(
                DateTime.TryParseExact(
                    timerValue,
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out timerTime),
                "Timer file contains an invalid timestamp: " + timerValue + ". Expected format: yyyyMMddHHmmss.");

            TimeSpan difference = (DateTime.Now - timerTime).Duration();
            Assert.IsTrue(
                difference <= TimeSpan.FromMinutes(5),
                "Timer timestamp " + timerValue + " is " + difference.TotalMinutes.ToString("F1", CultureInfo.InvariantCulture) +
                " minutes away from the current time; expected no more than 5 minutes.");
        }

        [TestMethod]
        [Owner("Bailin")]
        [Priority(2)]
        [Description("Checks that today's CDS log exists and its file size is within the expected range.")]
        public void CDS_Log_Check()
        {
            string logPath = Path.Combine(
                @"D:\CDS\Log",
                "CHR_" + DateTime.Now.ToString("dddd", CultureInfo.InvariantCulture) + ".Log");

            Assert.IsTrue(File.Exists(logPath), "Log not found: " + logPath);

            long fileSize = new FileInfo(logPath).Length;
            const long minSize = 10 * 1024;
            const long maxSize = 300 * 1024;

            Assert.IsTrue(
                fileSize >= minSize && fileSize <= maxSize,
                "Log file size was " + fileSize + " bytes; expected between " + minSize + " and " + maxSize + " bytes: " + logPath);
        }

        // Reads all lines while allowing the writer process to keep its handle open.
        private static string[] ReadAllLinesShared(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}
