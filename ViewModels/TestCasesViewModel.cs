//-------------------------------------------------------------------------------------------------
// <copyright file="TestCasesViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System.Windows;

namespace TestViewer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Data;
    using System.Windows.Media;

    using TestViewer.Models;
    using TestViewer.Utilities;

    /// <summary>
    /// Test cases view model
    /// </summary>
    internal class TestCasesViewModel
    {
        /// <summary>
        /// Test assembly
        /// </summary>
        private Assembly testAssembly;

        /// <summary>
        /// Tcp server helper
        /// </summary>
        private TcpServerHelper tcpServerHelper;

        /// <summary>
        /// Tcp client helper
        /// </summary>
        private TcpClientHelper tcpClientHelper;

        /// <summary>
        /// Initializes a new instance of the TestCasesViewModel class
        /// </summary>
        public TestCasesViewModel()
        {
            TestCasesModel = new TestCasesModel();
            this.TestCasesModelICollectionView = CollectionViewSource.GetDefaultView(this.TestCasesModel);
            sortOrderBy = new SortOrderBy();
        }

        #region run settings

        public static bool? AutoSendEmail = false;
        public static int CurrentRerunTimes = 0;
        public static int AutoRerunTimes = 0;

        public static string EmailSubject = "Test Report";
        public static string EmailSignature = "Test Team";

        #endregion

        /// <summary>
        /// Gets or sets .trx file path
        /// </summary>
        public static string SourceTrxFileUrl { get; set; }

        /// <summary>
        /// Gets or sets source test dll path
        /// </summary>
        public static string SourceTestDllUrl { get; set; }

        /// <summary>
        /// Gets or sets local test dll path
        /// </summary>
        public static string LocalTestDllUrl { get; set; }

        /// <summary>
        /// Gets or sets local testsettings path
        /// </summary>
        public static string LocalTestSettingsUrl { get; set; }

        /// <summary>
        /// Gets or sets test result output folder path
        /// </summary>
        public static string TestResultOutputFolder { get; set; }

        /// <summary>
        /// Gets or sets test bits folder path
        /// </summary>
        public static string TestBitFolder { get; set; }

        /// <summary>
        /// Gets or sets server IP address
        /// </summary>
        public static string ServerIPAddress { get; set; }

        /// <summary>
        /// Gets or sets server port
        /// </summary>
        public static string ServerPort { get; set; }

        /// <summary>
        /// Gets or sets source testsettings path
        /// </summary>
        public static string SourceTestSettingsUrl { get; set; }

        /// <summary>
        /// Gets or sets the data source for Test Depot
        /// </summary>
        public ICollectionView TestCasesModelICollectionView { get; set; }

        /// <summary>
        /// Gets the filtered data source for Test Monitor
        /// </summary>
        public ICollectionView TestCasesModelScheduledICollectionView
        {
            get
            {
                TestCasesModel testCasesModelScheduled = new TestCasesModel();
                foreach (TestCase t in this.TestCasesModelICollectionView)
                {
                    if (t.IsChecked)
                    {
                        testCasesModelScheduled.Add(t);
                    }
                }

                return CollectionViewSource.GetDefaultView(testCasesModelScheduled);
            }
        }

        /// <summary>
        /// Gets or sets the original data source
        /// </summary>
        private TestCasesModel TestCasesModel { get; set; }

        /// <summary>
        /// Create server
        /// </summary>
        public void CreateServer()
        {
            this.tcpServerHelper = new TcpServerHelper(this.TestCasesModelICollectionView);
        }

        /// <summary>
        /// Create client
        /// </summary>
        public void CreateClient()
        {
            this.tcpClientHelper = new TcpClientHelper();
        }

        /// <summary>
        /// Disconnected a single client machine
        /// </summary>
        /// <param name="machineName"></param>
        public void DisconnectClientMachine(string machineName)
        {
            this.tcpServerHelper.BootClientMachine(machineName);
        }

        /// <summary>
        /// Dispose the tcp related objects
        /// </summary>
        public void Dispose()
        {
            if (this.tcpServerHelper != null)
            {
                this.tcpServerHelper.Dispose();
            }

            if (this.tcpClientHelper != null)
            {
                this.tcpClientHelper.Dispose();
            }
        }

        /// <summary>
        /// Filter test case
        /// </summary>
        /// <param name="p0">P0 test case</param>
        /// <param name="p1">P1 test case</param>
        /// <param name="p2">P2 test case</param>
        /// <param name="isInconclusive">Not included test case</param>
        /// <param name="testCaseName">Test case name</param>
        internal void FilterTestCase(bool p0, bool p1, bool p2, bool isInconclusive, string testCaseName)
        {
            this.TestCasesModelICollectionView.Filter = 
                arg =>
                {
                    TestCase t = arg as TestCase;

                    if (t == null)
                    {
                        throw new NullReferenceException();
                    }

                    if ((p0 && t.Priority.Equals(0)) ||
                        (p1 && t.Priority.Equals(1)) ||
                        (p2 && t.Priority.Equals(2)))
                    {
                        return FuzzyQueryHelper.FuzzyQuery(t, testCaseName, isInconclusive);
                    }
                         
                    return false;
                };

            foreach (TestCase t in TestCasesModel)
            {
                t.IsChecked = false;
            }
        }

        /// <summary>
        /// Filter test case
        /// </summary>
        /// <param name="priorityPairs">Priority key/value pairs</param>
        /// <param name="isInconclusive">Not included test case</param>
        /// <param name="testCaseName">Test case name</param>
        internal void FilterTestCase(Dictionary<int, bool> priorityPairs, bool isInconclusive, string testCaseName)
        {
            this.TestCasesModelICollectionView.Filter =
                arg =>
                {
                    TestCase t = arg as TestCase;

                    if (t == null)
                    {
                        throw new NullReferenceException();
                    }

                    if (PriorityFiler(priorityPairs, t))
                    {
                        return FuzzyQueryHelper.FuzzyQuery(t, testCaseName, isInconclusive);
                    }

                    return false;
                };

            foreach (TestCase t in TestCasesModel)
            {
                t.IsChecked = false;
            }
        }

        /// <summary>
        /// Filter tests according to checked properties
        /// </summary>
        /// <param name="priorityPairs">Priority pairs</param>
        /// <param name="t">Test case</param>
        /// <returns>True as leave, false as abandon</returns>
        private static bool PriorityFiler(Dictionary<int, bool> priorityPairs, TestCase t)
        {
            bool flag = false;

            foreach (KeyValuePair<int, bool> p in priorityPairs)
            {
                flag = flag || (p.Value && t.Priority.Equals(p.Key));
            }

            return flag;
        }

        /// <summary>
        /// Toggle all tests checked state
        /// </summary>
        /// <param name="isChecked">Is checked</param>
        internal void ToggleAllTestsCheckedState(bool isChecked)
        {
            foreach (TestCase t in this.TestCasesModelICollectionView)
            {
                t.IsChecked = isChecked;
            }
        }

        /// <summary>
        /// Is all tests checked
        /// </summary>
        /// <returns>Bool value</returns>
        internal bool IsAllTestsChecked()
        {
            bool temp = true;
            foreach (TestCase t in this.TestCasesModelICollectionView)
            {
                if (!t.IsChecked)
                {
                    temp = false;
                    break;
                }
            }

            return temp;
        }

        /// <summary>
        /// Initialize test run both for locally run and distributed run
        /// </summary>
        internal void InitializeTestRun()
        {
            // Initialize vars for each scheduled run
            CurrentRerunTimes = 0;

            // Other initialize work
            foreach (TestCase t in this.TestCasesModelICollectionView)
            {
                if (t.IsChecked)
                {
                    t.State = TestCaseState.Pending;
                    t.StartTime = null;
                    t.EndTime = null;
                    t.Duration = null;
                    t.RerunTimes = 0;
                    t.ErrorMessage = null;
                    t.BriefErrorMessage = null;
                    t.RerunCompleted = false;
                }
            }

        }

        /// <summary>
        /// Get checked tests count
        /// </summary>
        /// <returns>How many tests will be run</returns>
        internal int GetCheckedTestCasesCount()
        {
            int count = 0;
            foreach (TestCase t in this.TestCasesModelICollectionView)
            {
                if (t.IsChecked)
                {
                    // we do not run ignore tests
                    if (t.Ignore)
                    {
                        t.IsChecked = false;
                        continue;
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Export selection to Excel
        /// </summary>
        internal void ExportSelectionToExcel()
        {
            OfficeHelper.ExportSelectionToExcel(this.TestCasesModelICollectionView);
        }

        internal void ResetNotPassedTests()
        {
            foreach (TestCase t in this.TestCasesModelICollectionView)
            {
                if (t.State == TestCaseState.Failed || 
                    //t.State == TestCaseState.Running || // for unknown error
                    t.State == TestCaseState.Inconclusive || 
                    t.State == TestCaseState.Timeout)
                {
                    t.State = TestCaseState.Pending;
                    t.RerunTimes++;
                }
            }
        }

        /// <summary>
        /// Run selected tests
        /// </summary>
        internal void RunLocally()
        {
            this.RunTestPass();
        }

        /// <summary>
        /// Run test pass
        /// </summary>
        internal void RunTestPass()
        {
            var countPending = this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Pending);

            if (countPending > 0)
            {
                foreach (TestCase t in this.TestCasesModelICollectionView)
                {
                    if (t.State == TestCaseState.Pending)
                    {
                        this.RunTestByID(LocalTestDllUrl, t);
                        break;
                    }
                }
            }
            else
            {
                // Rerun controller
                if (CurrentRerunTimes < AutoRerunTimes)
                {
                    CurrentRerunTimes++;

                    this.ResetNotPassedTests();
                    this.RunTestPass();
                }
            }

            if (this.TestCasesModelScheduledICollectionView.Cast<TestCase>().Count(tc => !tc.RerunCompleted) == 0 && AutoSendEmail == true)
            {
                // passed in the last
                OfficeHelper.SendMail(this.TestCasesModelScheduledICollectionView, -1);
            }
        }

        /// <summary>
        /// Run selected tests on clients
        /// </summary>
        internal void RunOnClients()
        {
            // TODO auto rerun
            // Start run in another thread
            new Thread(new ThreadStart(RunTestsOnClientsWorkerThread)).Start();

            //new Thread(new ThreadStart(TestPassWatcherThread)).Start();
            //Thread a = new Thread(RunTestsOnClientsWorkerThread);
            //a.Start();
            //a.
        }

        //private void TestPassWatcherThread()
        //{
        //    while(this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Pending) == 0)
        //    {

        //    }
        //}

        private void RunTestsOnClientsWorkerThread()
        {
            // need to copy test bits first
            foreach (TestMachine tm in TestMachinesViewModel.ClientsModel)
            {
                tm.IsTestBitsUpdated = false;
            }

            while (!IsZeroScheduledTest)
            {
                try
                {
                    if (TestMachinesViewModel.ClientsModel.Count(a => a.IsChecked) == 0)
                    {
                        //this.tcpServerHelper.Log("No client machine available...", Colors.Red);
                        LoggerViewModel.Log("No client machine is available ...", Colors.Red);
                        break;
                    }

                    string machineName = string.Empty;

                    
                    // find a free client
                    foreach (TestMachine tm in TestMachinesViewModel.ClientsModel)
                    {
                        if (tm.State == TestMachineState.Free && tm.IsChecked)
                        {
                            tm.State = TestMachineState.Busy;

                            machineName = tm.Name;

                            // Update test bits
                            if (tm.IsTestBitsUpdated == true)
                            {
                                //this.tcpServerHelper.Log("1 test bits updated ...");
                                //LoggerViewModel.Log("1 test bits updated ...");
                            }
                            else
                            {
                                #region

                                int free = 0;
                                int busy = 0;
                                int disconnected = 0;
                                foreach (var c in TestMachinesViewModel.ClientsModel)
                                {
                                    if (c.State == TestMachineState.Free)
                                    {
                                        free++;
                                    }
                                    else if (c.State == TestMachineState.Busy)
                                    {
                                        busy++;
                                    }
                                    else if (c.State == TestMachineState.Disconnected)
                                    {
                                        disconnected++;
                                    }
                                }

                                //LoggerViewModel.Log(string.Format("clients model count:{0}, free:{1}, busy:{2}, disconnected:{3}", TestMachinesViewModel.ClientsModel.Count, free, busy, disconnected));
                                //LoggerViewModel.Log("2 test bit not updated...");

                                #endregion

                                this.tcpServerHelper.SendMessageToClientByMachineName(machineName, "UPDATETESTBITS$" + SourceTestDllUrl + "$" + SourceTestSettingsUrl);
                                machineName = string.Empty;
                            }

                            if (!machineName.Equals(string.Empty))
                            {
                                var countPendingOrRunning = this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => 
                                    tc.State == TestCaseState.Pending || tc.State == TestCaseState.Running);

                                if (countPendingOrRunning == 0)
                                {
                                    // Rerun controller
                                    if (CurrentRerunTimes < AutoRerunTimes)
                                    {
                                        CurrentRerunTimes++;
                                        //LoggerViewModel.Log(string.Format("current rerun times(after++):{0}.... ", CurrentRerunTimes), Colors.Red);

                                        // change or failed or inconclusive to pending
                                        foreach (TestCase t in this.TestCasesModelICollectionView)
                                        {
                                            if (t.State == TestCaseState.Failed || t.State == TestCaseState.Inconclusive)
                                            {
                                                t.State = TestCaseState.Pending;
                                                t.RerunTimes++;
                                            }
                                        }
                                    }
                                    else
                                    { 
                                        // send a email
                                    }
                                }


                                var countPending = this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc =>
                                    tc.State == TestCaseState.Pending);

                                if (countPending == 0)
                                {
                                    tm.State = TestMachineState.Free;
                                }
                                else
                                {
                                    // Find a pending test to run
                                    foreach (TestCase t in this.TestCasesModelICollectionView)
                                    {
                                        if (t.State == TestCaseState.Pending)
                                        {
                                            t.State = TestCaseState.Running;
                                            t.ClientMachineName = machineName;
                                            this.tcpServerHelper.SendMessageToClientByMachineName(machineName, "RUNTEST$" + t.ID);
                                            break;
                                        }
                                    }

                                }
                            }

                            //sleep to avoid copy test bits to the same client twice
                            Thread.Sleep(1000);
                            break;
                        }
                    }

                    //if (!machineName.Equals(string.Empty))
                    //{
                    //    // find a pending test
                    //    foreach (TestCase t in this.TestCasesModelICollectionView)
                    //    {
                    //        if (t.State == TestCaseState.Pending)
                    //        {
                    //            t.State = TestCaseState.Running;
                    //            t.ClientMachineName = machineName;
                    //            this.tcpServerHelper.SendMessageToClientByMachineName(machineName, "RUNTEST$" + t.ID);

                    //            break;
                    //        }
                    //    }

                        //var countPending = this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Pending);

                        //if (countPending > 0)
                        //{
                        //    // find a pending test
                        //    foreach (TestCase t in this.TestCasesModelICollectionView)
                        //    {
                        //        if (t.State == TestCaseState.Pending)
                        //        {
                        //            t.State = TestCaseState.Running;
                        //            t.ClientMachineName = machineName;
                        //            this.tcpServerHelper.SendMessageToClientByMachineName(machineName, "RUNTEST$" + t.ID);

                        //            break;
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    // Rerun controller
                        //    if (CurrentRerunTimes <= AutoRerunTimes)
                        //    {
                        //        this.ResetNotPassedTests();

                        //        // Free client bad.....!!!!!!!!!!!!!!
                        //        foreach (TestMachine tm in TestMachinesViewModel.ClientsModel)
                        //        {
                        //            if (tm.Name == machineName)
                        //            {
                        //                tm.State = TestMachineState.Free;
                        //                break;
                        //            }
                        //        }
                        //        CurrentRerunTimes++;
                        //    }
                        //}
                    //}
                }
                // bug of VS
                catch (TargetInvocationException ex)
                {
                    LoggerViewModel.Log(string.Format("Throw a targetInvocationException [TestCasesViewModel]: {0}.", ex.Message), Colors.Red);
                }
                catch (Exception ex)
                {
                    LoggerViewModel.Log(string.Format("Throw an exception [TestCasesViewModel]: {0}.", ex.Message), Colors.Red);
                }
            }
        }

        internal bool IsZeroScheduledTest
        {
            get
            {
                return this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc =>
                    tc.State != TestCaseState.NotSet &&
                    tc.State != TestCaseState.Passed &&
                    tc.RerunTimes <= AutoRerunTimes) == 0;

                //return this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Pending) == 0;
                //foreach (TestCase t in this.TestCasesModelICollectionView)
                //{
                //    if (t.State != TestCaseState.Passed && t.RerunTimes < AutoRerunTimes)
                //    {
                //        t.State = TestCaseState.Pending;
                //        break;
                //    }
                //}

                //return this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => 
                //    tc.State != TestCaseState.NotSet &&
                //    tc.State != TestCaseState.Passed && 
                //    tc.RerunTimes < AutoRerunTimes) == 0;

                ////bool isZeroScheduledTest = this.TestCasesModelICollectionView.Cast<TestCase>().Count(tc => tc.State == TestCaseState.Pending) == 0;

                ////if (isZeroScheduledTest)
                ////{
                ////    if (CurrentRerunTimes <= AutoRerunTimes)
                ////    {
                ////        this.ResetNotPassedTests();
                ////        isZeroScheduledTest = false;
                ////        CurrentRerunTimes++;
                ////    }
                ////}

                ////return isZeroScheduledTest;

                //bool isZeroScheduledTest = true;

                //foreach (TestCase t in this.TestCasesModelICollectionView)
                //{
                //    if (t.State == TestCaseState.Pending)
                //    {
                //        isZeroScheduledTest = false;
                //        break;
                //    }
                //}

                //return isZeroScheduledTest;
            }
        }

        /// <summary>
        /// Run test by unique ID
        /// </summary>
        /// <param name="localTestDllUrl">Local test dll url</param>
        /// <param name="tcs">Test case</param>
        internal void RunTestByID(string localTestDllUrl, TestCase tcs)
        {
            // make sure it won't affect next test case
            tcs.StartTime = null;
            tcs.EndTime = null;

            tcs.State = TestCaseState.Running;
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";

            // leave it here for test only
            // string cmdArgs = "@/k ECHO OFF & exit";
            string cmdArgs = "@/k ECHO OFF ";

            //if (Environment.Is64BitOperatingSystem)
            //{
            //    cmdArgs += @"& call ""C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"" ";
            //}
            //else
            //{
            //    cmdArgs += @"& call ""C:\\Program Files\\Microsoft Visual Studio 11.0\\VC\\vcvarsall.bat"" ";
            //}
            //MessageBox.Show(MsTestHelper.GetCommandPromptPath());
            //cmdArgs += @"& call """ + MsTestHelper.GetCommandPromptPath() + @""" ";
            //MessageBox.Show(cmdArgs);

            // set mstest working directory
            // move to create property exact a setting option for it in the future
            // TestResultOutputFolder = @"c:\TestResults";

            if (!System.IO.Directory.Exists(TestResultOutputFolder))
            {
                System.IO.Directory.CreateDirectory(TestResultOutputFolder);
            }

            Environment.CurrentDirectory = TestResultOutputFolder;

            string resultFile = Environment.UserName + "_" + Environment.MachineName + " " + DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss") + ".trx";
            cmdArgs += @"& """ + MsTestHelper.GetMsTestPath() + @""" /testcontainer:""" + localTestDllUrl + @""" /test:" + tcs.ID + @" /unique /resultsfile:""" + resultFile + @""" ";
            if (!SourceTestSettingsUrl.Trim().Equals(string.Empty))
            {
                cmdArgs += @"/testsettings:""" + SourceTestSettingsUrl + @"""";
            }
            cmdArgs += "& exit";

            psi.Arguments = cmdArgs;
            
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process myProcess = new Process();
            myProcess.EnableRaisingEvents = true;
            myProcess.Exited += (sender, e) =>
            {
                tcs.EndTime = DateTime.Now;
                tcs.Duration = new DateTime(tcs.EndTime.Value.Subtract(tcs.StartTime.Value).Ticks);
                tcs.ResultFile = resultFile;

                if (!System.IO.File.Exists(resultFile))
                {
                    tcs.State = TestCaseState.Failed;
                    tcs.ErrorMessage = "Result file not generated.";
                }
                else
                {
                    var logColor = Colors.Black;
                    var testID = XmlHelper.GetAttributeValueFromTrx(resultFile, "/TestRun/Results/UnitTestResult", "testName");
                    var outCome = XmlHelper.GetAttributeValueFromTrx(resultFile, "/TestRun/Results/UnitTestResult", "outcome");
                    switch (outCome)
                    {
                        case "Aborted":
                            tcs.State = TestCaseState.Aborted;
                            break;
                        case "Failed":
                            tcs.State = TestCaseState.Failed;
                            logColor = Colors.Red;
                            break;
                        case "Inconclusive":
                            tcs.State = TestCaseState.Inconclusive;
                            logColor = Colors.Orange;
                            break;
                        case "NotExecuted":
                            tcs.State = TestCaseState.NotExecuted;
                            break;
                        case "Passed":
                            tcs.State = TestCaseState.Passed;
                            logColor = Colors.Green;
                            break;
                        case "PassedButRunAborted":
                            tcs.State = TestCaseState.PassedButRunAborted;
                            break;
                        case "Timeout":
                            tcs.State = TestCaseState.Timeout;
                            logColor = Colors.DeepSkyBlue;
                            break;
                        default:
                            //error
                            tcs.State = TestCaseState.NotSet;
                            logColor = Colors.Red;
                            break;
                    }

                    string tempErrorMessage = XmlHelper.GetInnerTextFromTrx(resultFile, "/TestRun/Results/UnitTestResult/Output/ErrorInfo/Message");
                    tcs.StdOut = XmlHelper.GetInnerTextFromTrx(resultFile, "/TestRun/Results/UnitTestResult/Output/StdOut");
                    tcs.StackTrace = XmlHelper.GetInnerTextFromTrx(resultFile, "/TestRun/Results/UnitTestResult/Output/ErrorInfo/StackTrace");
                    tcs.ResultFiles = XmlHelper.GetAttributeValueFromTrx(resultFile, "/TestRun/Results/UnitTestResult/ResultFiles/ResultFile", "path");
                    tcs.ErrorMessage = tempErrorMessage;
                    tcs.ClientMachineName = Environment.MachineName;

                    if(tcs.State == TestCaseState.Passed || tcs.RerunTimes == AutoRerunTimes)
                    {
                        tcs.RerunCompleted = true;
                    }

                    LoggerViewModel.Log(string.Format("{0} {1} [{2}].", testID, outCome, "LocalMachine"), logColor);
                }

                tcs.BriefErrorMessage = tcs.ErrorMessage.Replace("\r", "").Replace("\n", "");

                RunTestPass();
            };

            myProcess.StartInfo = psi;
            myProcess.Start();
            tcs.StartTime = DateTime.Now;
        }

        /// <summary>
        /// Load assembly for all operations:GetPriorities, GetTop3CustomTestPriorities, LoadTests
        /// </summary>
        internal void LoadAssembly()
        {
            testAssembly = Assembly.LoadFrom(LocalTestDllUrl);
        }

        /// <summary>
        /// Gets all priority type in current dll
        /// </summary>
        /// <returns>List int</returns>
        internal List<int> GetPriorities()
        {
            List<int> p = new List<int>();
            int tempPriority = int.MaxValue;

            foreach (Type type in testAssembly.GetTypes())
            {
                foreach (MemberInfo mi in type.GetMembers())
                {
                    IList<CustomAttributeData> customAttributesdata = mi.GetCustomAttributesData();

                    int customAttributeDataIndex = 0;

                    foreach (object attribute in mi.GetCustomAttributes(true))
                    {
                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.PriorityAttribute")
                        {
                            tempPriority = (int)customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value;
                        }
                    
                        customAttributeDataIndex++;
                    }

                    if (tempPriority != int.MaxValue)
                    {
                        if(!p.Contains(tempPriority))
                        {
                            p.Add(tempPriority);
                        }
                    }
                }
            }

            p.Sort();
            
            return p;
        }

        /// <summary>
        /// Get top 3 custom test priorities
        /// </summary>
        /// <returns></returns>
        internal List<KeyValuePair<string, int>> GetTop3CustomTestPriorities()
        {
            Dictionary<string, int> customTestPropertyDictionary = new Dictionary<string, int>();

            foreach (Type type in testAssembly.GetTypes())
            {
                foreach (MemberInfo mi in type.GetMembers())
                {
                    IList<CustomAttributeData> customAttributesdata = mi.GetCustomAttributesData();

                    int customAttributeDataIndex = 0;

                    foreach (object attribute in mi.GetCustomAttributes(true))
                    {
                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute")
                        {
                            string costumTestProperty = customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString();
                            if (!customTestPropertyDictionary.Keys.Contains(costumTestProperty))
                            {
                                customTestPropertyDictionary.Add(costumTestProperty, 0);
                            }
                            else
                            {
                                customTestPropertyDictionary[costumTestProperty]++;
                            }
                        }

                        customAttributeDataIndex++;
                    }
                }
            }
            List<KeyValuePair<string, int>> customTestPropertyList = customTestPropertyDictionary.ToList();
            customTestPropertyList.Sort(
                delegate(KeyValuePair<string, int> firstPair, KeyValuePair<string, int> secondPair)
                {
                    return firstPair.Value.CompareTo(secondPair.Value) * -1;
                });

            return customTestPropertyList;
        }

        /// <summary>
        /// Load tests
        /// </summary>
        internal void LoadTests(string custom1, string custom2, string custom3)
        {
            this.TestCasesModel.Clear();

            foreach (Type type in testAssembly.GetTypes())
            {
                foreach (MemberInfo mi in type.GetMembers())
                {
                    TestCase testCase = new TestCase();

                    IList<CustomAttributeData> customAttributesdata = mi.GetCustomAttributesData();

                    int customAttributeDataIndex = 0;

                    foreach (object attribute in mi.GetCustomAttributes(true))
                    {
                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute")
                        {
                            testCase.Name = mi.Name;
                            testCase.ID = string.Join(".", mi.DeclaringType.FullName, mi.Name);
                        }

                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.PriorityAttribute")
                        {
                            testCase.Priority = (int)customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value;
                        }

                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.OwnerAttribute")
                        {
                            testCase.Owner = customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString();
                        }

                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute")
                        {
                            testCase.Ignore = true;
                        }

                        if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute")
                        {
                            testCase.Description = customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString().Replace("\r\n", "");
                        }

                        // Custom test properties
                        if (custom1 != string.Empty)
                        {
                            if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute")
                            {
                                //"CaseID", "JobId" or "Locale"
                                if (customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString().Equals(custom1))
                                {
                                    testCase.Custom1 = customAttributesdata[customAttributeDataIndex].ConstructorArguments[1].Value.ToString();
                                }
                            }
                        }

                        if (custom2 != string.Empty)
                        {
                            if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute")
                            {
                                //"CaseID", "JobId" or "Locale"
                                if (customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString().Equals(custom2))
                                {
                                    testCase.Custom2 = customAttributesdata[customAttributeDataIndex].ConstructorArguments[1].Value.ToString();
                                }
                            }
                        }

                        if (custom3 != string.Empty)
                        {
                            if (attribute.ToString() == "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute")
                            {
                                //"CaseID", "JobId" or "Locale"
                                if (customAttributesdata[customAttributeDataIndex].ConstructorArguments[0].Value.ToString().Equals(custom3))
                                {
                                    testCase.Custom3 = customAttributesdata[customAttributeDataIndex].ConstructorArguments[1].Value.ToString();
                                }
                            }
                        }

                        customAttributeDataIndex++;
                    }

                    if (testCase.Name != null)
                    {
                        TestCasesModel.Add(testCase);
                    }
                }
            }
        }

        internal void Sort (string column)
        {
            List<TestCase> sortList = null;
            switch (column)
            {
                case "Name":
                    sortList = this.sortOrderBy.isNameAscending ?
                        this.TestCasesModel.OrderBy(x => x.Name).ToList() : this.TestCasesModel.OrderByDescending(x => x.Name).ToList();
                    this.sortOrderBy.isNameAscending = !this.sortOrderBy.isNameAscending;
                    break;
                case "Custom3"://todo
                    sortList = this.sortOrderBy.isStateAscending ?
                        this.TestCasesModel.OrderBy(x => x.State).ToList() : this.TestCasesModel.OrderByDescending(x => x.State).ToList();
                    this.sortOrderBy.isStateAscending = !this.sortOrderBy.isStateAscending;
                    break;
                case "P":
                    sortList = this.sortOrderBy.isPriorityAscending ?
                        this.TestCasesModel.OrderBy(x => x.Priority).ToList() : this.TestCasesModel.OrderByDescending(x => x.Priority).ToList();
                    this.sortOrderBy.isPriorityAscending = !this.sortOrderBy.isPriorityAscending;
                    break;
                case "Owner":
                    sortList = this.sortOrderBy.isOwnerAscending ?
                        this.TestCasesModel.OrderBy(x => x.Owner).ToList() : this.TestCasesModel.OrderByDescending(x => x.Owner).ToList();
                    this.sortOrderBy.isOwnerAscending = !this.sortOrderBy.isOwnerAscending;
                    break;
                case "Description":
                    sortList = this.sortOrderBy.isDescriptionAscending ?
                        this.TestCasesModel.OrderBy(x => x.Description).ToList() : this.TestCasesModel.OrderByDescending(x => x.Description).ToList();
                    this.sortOrderBy.isDescriptionAscending = !this.sortOrderBy.isDescriptionAscending;
                    break;
                case "Custom1":
                    sortList = this.sortOrderBy.isCustom1Ascending ?
                        this.TestCasesModel.OrderBy(x => x.Custom1).ToList() : this.TestCasesModel.OrderByDescending(x => x.Custom1).ToList();
                    this.sortOrderBy.isCustom1Ascending = !this.sortOrderBy.isCustom1Ascending;
                    break;
                case "Custom2":
                    sortList = this.sortOrderBy.isCustom2Ascending ?
                        this.TestCasesModel.OrderBy(x => x.Custom2).ToList() : this.TestCasesModel.OrderByDescending(x => x.Custom2).ToList();
                    this.sortOrderBy.isCustom2Ascending = !this.sortOrderBy.isCustom2Ascending;
                    break;
                case "state"://todo
                    sortList = this.sortOrderBy.isCustom3Ascending ?
                        this.TestCasesModel.OrderBy(x => x.Custom3).ToList() : this.TestCasesModel.OrderByDescending(x => x.Custom3).ToList();
                    this.sortOrderBy.isCustom3Ascending = !this.sortOrderBy.isCustom3Ascending;
                    break;
                default:
                    // error
                    break;
            }

            this.TestCasesModel.Clear();
            sortList.ForEach(this.TestCasesModel.Add);
        }

        private SortOrderBy sortOrderBy { get; set; }

        internal class SortOrderBy
        {
            internal bool isNameAscending = false;
            internal bool isStateAscending = true;
            internal bool isPriorityAscending = false;
            internal bool isOwnerAscending = false;
            internal bool isDescriptionAscending = false;
            internal bool isCustom1Ascending = false;
            internal bool isCustom2Ascending = false;
            internal bool isCustom3Ascending = false;
        }

        /// <summary>
        /// Get test case by name
        /// </summary>
        /// <param name="testCaseName">Test case name</param>
        /// <returns>Test case object</returns>
        internal TestCase GetTestCaseByName(string testCaseName)
        {
            return this.TestCasesModelICollectionView.Cast<TestCase>().FirstOrDefault(t => t.Name.Equals(testCaseName));
        }
    }
}
