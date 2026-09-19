//-------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Xml;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    using TestViewer.Models;
    using TestViewer.Utilities;
    using TestViewer.ViewModels;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Client Explorer tab
        /// </summary>
        private Visibility clientVisibility;

        /// <summary>
        /// Gets or sets Client Explorer tab visibility
        /// </summary>
        public Visibility ClientVisibility
        {
            get
            {
                return this.clientVisibility;
            }

            set
            {
                this.clientVisibility = value; 
                this.OnPropertyChanged("ClientVisibility");
            }
        }

        /// <summary>
        /// Property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// On property changed event
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            //MessageBox.Show(Regex.IsMatch("Congratulations, your request has been sent! Your service number is SS XX-XXX-XXX-XXX",
            //                              @"^Congratulations, your request has been sent! Your service number is SS ([A-Z]){2}(-(([A-Z]){3})){3}$")
            //                    .ToString());

            this.AddHandler(CloseableTabItem.CloseTabEvent, new RoutedEventHandler(this.CloseTab));

            this.AddStartPage();

            this.Closed += new System.EventHandler(MainWindow_Closed);

            this.logger.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;

            SettingsHelper.LoadSettings();

            this.TestCaseName.ToolTip =
@"Use keywords to filter test cases, examples: 
1. [keyword] : smart
2. [keyword1 & keyword2] : smartglass & bailin.wei
3. [keyword1 | keyword2] : ShapeGame | speech
4. [testcasename1,
    testcasename2,
    testcasename3,
    ...]
5. Add [Ignore] for filtering ignored test cases
";

            //this.ExplorerContainer.SelectedIndex = 2;

            //this.ExplorerContainer.SelectedIndex = 1;
            //this.ExplorerContainer.Items.Refresh();
        }

        void MainWindow_Closed(object sender, System.EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// New project for usercontrol
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void NewProjectForUserControl(object sender, RoutedEventArgs e)
        {
            this.NewProject();
        }

        /// <summary>
        /// Open project for usercontrol
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void OpenProjectForUserControl(object sender, RoutedEventArgs e)
        {
            this.OpenProject();
        }

        /// <summary>
        /// Load tests for user control
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void LoadTestsForUserControl(object sender, RoutedEventArgs e)
        {
            // copy test bits first
            string sourceUrl = TestCasesViewModel.SourceTestDllUrl;
            string dllFileName = sourceUrl.Substring(sourceUrl.LastIndexOf("\\") + 1);
            string testSettingsFileName = TestCasesViewModel.SourceTestSettingsUrl.Substring(TestCasesViewModel.SourceTestSettingsUrl.LastIndexOf("\\") + 1);
            string sourceDirectory = sourceUrl.Substring(0, sourceUrl.LastIndexOf("\\")) + "\\*";
            string destDirectory = Environment.CurrentDirectory + "\\TestBits" + DateTime.Now.ToString("_yyyy_MM_dd_hh_mm_ss");
            
            TestCasesViewModel.LocalTestDllUrl = destDirectory + "\\" + dllFileName;
            TestCasesViewModel.LocalTestSettingsUrl = destDirectory + "\\" + testSettingsFileName;

            this.statusBarText.Text = string.Format("Copying test bits from {0} to {1} ...", sourceDirectory, destDirectory);
            LoggerViewModel.Log(string.Format("Copying test bits from {0} to {1} ...", sourceDirectory, destDirectory));

            /*
             * /E Copy folders and subfolders, including Empty folders. 
             * /Y (Windows 2000 only) Suppress prompt to confirm overwriting a file.
             * /I If in doubt always assume the destination is a folder e.g. when the destination does not exist.
             */

            string cmdArgs = "@/k ECHO OFF ";
            cmdArgs += @"& xcopy """ + sourceDirectory + @""" """ + destDirectory + @""" /E /I /Y";
            if (TestCasesViewModel.SourceTestSettingsUrl != "")
            {
                cmdArgs += "& xcopy \"" + TestCasesViewModel.SourceTestSettingsUrl + "\" \"" + destDirectory + "\" ";
                LoggerViewModel.Log(string.Format("Copying test settings file from {0} to {1} ...", TestCasesViewModel.SourceTestSettingsUrl, TestCasesViewModel.LocalTestSettingsUrl));
            }
            cmdArgs += " & exit";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";
            psi.Arguments = cmdArgs;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process proc = new Process();
            proc.EnableRaisingEvents = true;
            proc.StartInfo = psi;

            proc.Exited += (psender, pe) =>
            {
                int exitCode = (psender as Process).ExitCode;

                if (!exitCode.ToString().Equals("0"))
                {
                    this.statusBarText.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        this.statusBarText.Text = "Copy file error:";
                        LoggerViewModel.Log(cmdArgs);
                    }));

                    return;
                }

                this.statusBarText.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    this.statusBarText.Text = string.Format("Loading assembly {0}...", dllFileName);
                    LoggerViewModel.Log(string.Format("Loading assembly {0}...", dllFileName));
                }));

                this.testCasesViewModel.LoadAssembly();
                

                this.priorityContainer.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    this.priorityContainer.Children.Clear();
                    // Load priorities
                    foreach (var i in this.testCasesViewModel.GetPriorities())
                    {
                        var cb = new CheckBox
                        {
                            IsChecked = true,
                            Content = string.Format("P{0}", i),
                            Margin = new Thickness(0, 0, 10, 0),
                            Tag = i
                        };
                        this.priorityContainer.Children.Add(cb);

                        if (autoDetect.IsChecked == true)
                        {
                            List<KeyValuePair<string, int>> list = testCasesViewModel.GetTop3CustomTestPriorities();

                            this.CustomTestProperty1.Text = list.Count >= 1 ? list[0].Key : string.Empty;
                            this.CustomTestProperty2.Text = list.Count >= 2 ? list[1].Key : string.Empty;
                            this.CustomTestProperty3.Text = list.Count >= 3 ? list[2].Key : string.Empty;
                        }
                        
                        testCasesViewModel.LoadTests(
                            this.CustomTestProperty1.Text, 
                            this.CustomTestProperty2.Text,
                            this.CustomTestProperty3.Text);
                        this.InitialFilter();
                    }
                }));

                this.statusBarText.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    this.statusBarText.Text = "Ready";
                    LoggerViewModel.Log(string.Format("Load assembly {0} completed.", dllFileName));

                    this.ExplorerContainer.SelectedIndex = 1;
                }));
            };

            proc.Start();
        }

        /// <summary>
        /// Load .trx file for user control
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void LoadTrxForUserControl(object sender, RoutedEventArgs e)
        {
            AddTrxViewerTab();
        }

        /// <summary>
        /// View test results details for usercontrol
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void ViewTestResultsDetailsForUserControl(object sender, RoutedEventArgs e)
        {
            string testCaseName = ((sender as TestMonitor).testPassList.SelectedItem as TestCase).Name;

            string tabName = GetShorterTabName(testCaseName);

            this.AddMonitorWindow(tabName, new TestResultsDetails(this.testCasesViewModel.GetTestCaseByName(testCaseName)));
            
            this.SelectedTabItem(tabName);
        }

        /// <summary>
        /// View trx details for usercontrol
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        public void ViewTrxDetailsForUserControl(object sender, RoutedEventArgs e)
        {
            string testCaseName = ((sender as TrxViewer).trxList.SelectedItem as TestCase).Name;

            string tabName = GetShorterTabName(testCaseName);

            this.AddMonitorWindow(tabName, new TestResultsDetails((sender as TrxViewer).TrxViewModel.GetTestCaseByName(testCaseName)));

            this.SelectedTabItem(tabName);
        }

        public void SendNameToFilterForUserControl(object sender, RoutedEventArgs e)
        {
            this.ExplorerContainer.SelectedIndex = 1;
            if(this.TestCaseName.Text.Equals(string.Empty))
            {
                this.TestCaseName.Text = (sender as TrxViewer).TrxViewModel.GetCheckedItemNames();
            }
            else
            {
                this.TestCaseName.Text += "\r\n" + (sender as TrxViewer).TrxViewModel.GetCheckedItemNames();    
            }
        }

        /// <summary>
        /// common method for a shorter tab name
        /// </summary>
        /// <param name="testCaseName"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private string GetShorterTabName(string testCaseName, bool direction = true)
        {
            const int MaxLength = 20;
            var shorttestCaseName = testCaseName.Length > MaxLength ?
                (direction ? testCaseName.Substring(0, MaxLength) + " ..." : "..." + testCaseName.Substring(testCaseName.Length - MaxLength, MaxLength))
                : 
                testCaseName;

            return shorttestCaseName + " [Results]";
        }

        /// <summary>
        /// Add start tab tab
        /// </summary>
        private void AddStartPage()
        {
            if (!TestWindowsViewModel.MonitorWindowState.StartPage)
            {
                var sp = new StartPage();
                sp.NewProject += this.NewProjectForUserControl;
                sp.OpenProject += this.OpenProjectForUserControl;
                sp.LoadTests += this.LoadTestsForUserControl;
                sp.OpenTrx += this.LoadTrxForUserControl;
                this.AddMonitorWindow("Start Page", sp);

                TestWindowsViewModel.MonitorWindowState.StartPage = true;
            }
             
            this.SelectedTabItem("Start Page");
        }

        /// <summary>
        /// Add test pass monitor tab
        /// </summary>
        private void AddTestPassMonitorTab()
        {
            if (!TestWindowsViewModel.MonitorWindowState.TestPassMonitor)
            {
                var tm = new TestMonitor();
                tm.ViewTestResultsDetailsForTestMonitor += this.ViewTestResultsDetailsForUserControl;
                this.AddMonitorWindow("Test Monitor", tm);
                TestWindowsViewModel.MonitorWindowState.TestPassMonitor = true;
            }
            
            this.SelectedTabItem("Test Monitor");
        }

        /// <summary>
        /// Add .trx viewer tab
        /// </summary>
        private void AddTrxViewerTab()
        {
            string shorterTabName = GetShorterTabName(TestCasesViewModel.SourceTrxFileUrl, false);
            var tv = new TrxViewer();
            tv.ViewTestResultsDetailsForTrxViewer += this.ViewTrxDetailsForUserControl;
            tv.SendNameToFilter += this.SendNameToFilterForUserControl;

            // Load .trx items in another thread
            (new Thread(
                delegate(){
                    LoadTrxThread(tv);
                })).Start();

            this.AddMonitorWindow(shorterTabName, tv);
        }

        private void LoadTrxThread(TrxViewer tv)
        {
            string resultFile = TestCasesViewModel.SourceTrxFileUrl;
            var nodes = XmlHelper.GetNodeListFromTrx(resultFile, "/TestRun/Results/UnitTestResult");
            var TestDefinitionsNode = XmlHelper.GetNodeFromTrx(resultFile, "/TestRun/TestDefinitions");

            var startTimeString = XmlHelper.GetAttributeValueFromTrx(resultFile, "/TestRun/Times", "start");
            var finishTimeString = XmlHelper.GetAttributeValueFromTrx(resultFile, "/TestRun/Times", "finish");
            var startTime = DateTime.Parse(startTimeString);
            var finishTime = DateTime.Parse(finishTimeString);
            TimeSpan ts = finishTime.Subtract(startTime);

            LoggerViewModel.Log(string.Format("Loading test results from {0}...", resultFile));

            foreach (XmlNode node in nodes)
            {
                var tc = new TestCase();
                var outCome = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "outcome");

                switch (outCome)
                {
                    case "Passed":
                        tc.State = TestCaseState.Passed;
                        break;
                    case "Failed":
                        tc.State = TestCaseState.Failed;
                        break;
                    case "Inconclusive":
                        tc.State = TestCaseState.Inconclusive;
                        break;
                    case "NotExecuted":
                        tc.State = TestCaseState.NotExecuted;
                        break;
                    case "Aborted":
                        tc.State = TestCaseState.Aborted;
                        break;
                    case "PassedButRunAborted":
                        tc.State = TestCaseState.PassedButRunAborted;
                        break;
                    case "Timeout":
                        tc.State = TestCaseState.Timeout;
                        break;
                    default:
                        tc.State = TestCaseState.NotSet;
                        break;
                }

                if(outCome != "Passed")
                {
                    tc.IsChecked = true;
                }

                if (tc.State == TestCaseState.NotExecuted || tc.State == TestCaseState.Aborted)
                {
                    tc.Name = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "testName");
                }
                else
                {
                    tc.Name = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "testName");
                    var duration = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "duration");
                    duration = duration == "" ? "00:00:00" : duration;
                    tc.Duration = Convert.ToDateTime(duration);
                    tc.StartTime = Convert.ToDateTime(XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "startTime"));
                    tc.EndTime = Convert.ToDateTime(XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "endTime"));

                    string tempErrorMessage = XmlHelper.GetInnerTextFromXml(node.OuterXml, "/UnitTestResult/Output/ErrorInfo/Message");
                    tc.StdOut = XmlHelper.GetInnerTextFromXml(node.OuterXml, "/UnitTestResult/Output/StdOut");
                    tc.StackTrace = XmlHelper.GetInnerTextFromXml(node.OuterXml, "/UnitTestResult/Output/ErrorInfo/StackTrace");
                    tc.ResultFiles = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult/ResultFiles/ResultFile", "path");

                    // for debug
                    //if (tc.Name == "VerifyAddProductPageRegisterInvalidSerialNumber")
                    //{
                    //    ;
                    //}

                    tc.ErrorMessage = tempErrorMessage;
                    tc.BriefErrorMessage = tempErrorMessage.Replace("\r", "").Replace("\n", "");

                    var testId = XmlHelper.GetAttributeValueFromXml(node.OuterXml, "/UnitTestResult", "testId");
                    var unitTestNode = XmlHelper.GetNodeFromTrxById(TestDefinitionsNode, string.Format("/TestDefinitions/UnitTest[@id='{0}']", testId));
                    
                    // TOTD add a checkbox on trxviewer page
                    if (true)
                    {
                        if (unitTestNode != null)
                        {
                            tc.Owner = XmlHelper.GetAttributeValueFromXml(unitTestNode.OuterXml, "/UnitTest/Owners/Owner", "name");

                            var priorityAttribute = unitTestNode.Attributes["priority"];
                            int priority;
                            tc.Priority = priorityAttribute != null && Int32.TryParse(priorityAttribute.Value, out priority) ? priority : 0;
                        }
                    }
                }

                tv.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    tv.TrxViewModel.AddTestResult(tc);
                    tv.TrxViewModel.TotalDuration = ts;
                }));
            }

            LoggerViewModel.Log(string.Format("{0} items loaded completed.", tv.trxList.Items.Count));
        }

        /// <summary>
        /// Refresh test pass monitor tab
        /// </summary>
        private void RefreshTestPassMonitorTab()
        {
            foreach (TabItem item in MonitorWindowsContainer.Items)
            {
                if (item.Header.ToString() == "Test Monitor")
                {
                    MonitorWindowsContainer.Items.Remove(item);
                    TestWindowsViewModel.MonitorWindowState.TestPassMonitor = false;
                    break;
                }
            }

            this.AddTestPassMonitorTab();
        }

        // move to top
        private ClientMonitor cm;

        /// <summary>
        /// Add client monitor tab
        /// </summary>
        private void AddClientMonitorTab()
        {
            if (!TestWindowsViewModel.MonitorWindowState.ClientMonitor)
            {
                cm = new ClientMonitor();
                this.AddMonitorWindow("Client Monitor", cm);

                TestWindowsViewModel.MonitorWindowState.ClientMonitor = true;
            }

            this.SelectedTabItem("Client Monitor");
        }

        /// <summary>
        /// Close tab
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="args">Routed event args</param>
        private void CloseTab(object source, RoutedEventArgs args)
        {
            TabItem tabItem = args.Source as TabItem;

            if (tabItem != null)
            {
                TabControl tabControl = tabItem.Parent as TabControl;
                if (tabControl != null)
                {
                    switch (tabItem.Header.ToString())
                    {
                        case "Start Page":
                            TestWindowsViewModel.MonitorWindowState.StartPage = false;
                            break;
                        case "Test Monitor":
                            TestWindowsViewModel.MonitorWindowState.TestPassMonitor = false;
                            break;
                        case "Client Monitor":
                            TestWindowsViewModel.MonitorWindowState.ClientMonitor = false;
                            break;
                        default:
                            break;
                    }

                    tabControl.Items.Remove(tabItem);
                }
            }
        }

        #region Test Case Filter

        /// <summary>
        /// Initilizes filter
        /// </summary>
        private void InitialFilter()
        {
            bool isInconclusive = (bool)this.Exclusive.IsChecked;
            Dictionary<int, bool> d = new Dictionary<int, bool>();
            foreach (CheckBox b in priorityContainer.Children)
            {
                d.Add((int)b.Tag, (bool)b.IsChecked);
            }

            this.testCasesViewModel.FilterTestCase(d, isInconclusive, this.TestCaseName.Text);
        }

        /// <summary>
        /// Initilizes filter wrapper
        /// </summary>
        private void InitialFilterWrap()
        {
            this.InitialFilter();
            this.UpdateAllCheckBox();
        }

        /// <summary>
        /// Test case name key down event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Text changed event args</param>
        private void TestCaseNameChanged(object sender, TextChangedEventArgs e)
        {
            if (TestCasesViewModel.LocalTestDllUrl != null)
            {
                this.InitialFilterWrap();
            }
        }

        /// <summary>
        /// Priority click event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void PriorityClick(object sender, RoutedEventArgs e)
        {
            this.InitialFilterWrap();
        }

        #endregion

        #region CheckBoxes

        /// <summary>
        /// Toggle all check box
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void ToggleAllCheckBoxClick(object sender, RoutedEventArgs e)
        {
            CheckBox toggleAllCheckBox = sender as CheckBox;
            this.testCasesViewModel.ToggleAllTestsCheckedState((bool)toggleAllCheckBox.IsChecked);
        }

        /// <summary>
        /// Toggle single check box
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Routed event args</param>
        private void ToggleSingleCheckBoxClick(object sender, RoutedEventArgs e)
        {
            this.UpdateAllCheckBox();
        }

        /// <summary>
        /// Selection changed event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Selection changed event args</param>
        private void TestListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // tbd
            // UpdateAllCheckBox();
        }

        /// <summary>
        /// Update all check box
        /// </summary>
        private void UpdateAllCheckBox()
        {
            // bug
            // solution1
            //(this.FindName("AllCheckBox") as CheckBox).IsChecked = this.testCasesViewModel.IsAllTestsChecked();

            // solution2
            //ViewHelper.FindVisualChildByName<CheckBox>(TestListView, "AllCheckBox").IsChecked =
            //    this.testCasesViewModel.IsAllTestsChecked();
        }
        #endregion

        /// <summary>
        /// Dispose TBD
        /// </summary>
        internal void Dispose()
        {
            //tbd
            this.testCasesViewModel.Dispose();
        }

        #region MenuItem

        /// <summary>
        /// Open .trx file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenTestResultFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "TRX File|*.trx" + "|All Files|*.*";

            if (d.ShowDialog() == true)
            {
                TestCasesViewModel.SourceTrxFileUrl = d.FileName;
                this.LoadTrxForUserControl(sender, e);
            }
        }

        private void CreateServer(object sender, RoutedEventArgs e)
        {
            this.testCasesViewModel.CreateServer();
        }

        private void CreateClient(object sender, RoutedEventArgs e)
        {
            this.testCasesViewModel.CreateClient();
        }

        /// <summary>
        /// Menu item exit click
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void MenuItemExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Menu item client explorer click
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void MenuItemClientExplorerClick(object sender, RoutedEventArgs e)
        {
            this.ExplorerContainer.SelectedIndex = 0;
        }

        /// <summary>
        /// Menu item test case explorer click
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void MenuItemTestCaseExplorerClick(object sender, RoutedEventArgs e)
        {
            this.ExplorerContainer.SelectedIndex = 1;
        }

        /// <summary>
        /// Menu item test pass explorer click
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void MenuItemTestPassExplorerClick(object sender, RoutedEventArgs e)
        {
            this.ExplorerContainer.SelectedIndex = 2;
        }

        private void About(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        #endregion


        #region Monitor Windows

        private string SelectedMachineName { get; set; }
            
        /// <summary>
        /// Test view selected event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void TreeViewSelected(object sender, RoutedEventArgs e)
        {
            var SelectedItem = this.TestMachinesTreeView.SelectedItem as TestMachine; 
            switch (SelectedItem.Tag)
            {
                case "Root":
                    this.TestMachinesTreeView.ContextMenu = this.TestMachinesTreeView.Resources["RootContext"] as ContextMenu;
                    SelectedMachineName = "Root";
                    break; 
                case "Machine":
                    this.TestMachinesTreeView.ContextMenu = this.TestMachinesTreeView.Resources["SingleMachineContext"] as ContextMenu;
                    SelectedMachineName = SelectedItem.Name;
                    break;
            }

            this.AddClientMonitorTab();
        }

        /// <summary>
        /// Tree View context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRemoteDesktop(object sender, RoutedEventArgs e)
        {
            cm.RemoteMachineByName(SelectedMachineName);
        }

        /// <summary>
        /// Tree View context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisconnectMachine(object sender, RoutedEventArgs e)
        {
            this.testCasesViewModel.DisconnectClientMachine(SelectedMachineName);
        }

        /// <summary>
        /// Menu item star page click
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void MenuItemStartPageClick(object sender, RoutedEventArgs e)
        {
            this.AddStartPage();
        }

        /// <summary>
        /// Add monitor window
        /// </summary>
        /// <param name="monitorHeader">Monitor header</param>
        /// <param name="userControl">User control</param>
        private void AddMonitorWindow(string monitorHeader, UserControl userControl = null)
        {
            CloseableTabItem theTabItem = new CloseableTabItem();
            theTabItem.Header = monitorHeader;
            theTabItem.Content = userControl;

            this.MonitorWindowsContainer.Items.Add(theTabItem);
            this.MonitorWindowsContainer.SelectedIndex = MonitorWindowsContainer.Items.Count - 1;
        }

        /// <summary>
        /// Selected tab item
        /// </summary>
        /// <param name="tabItemHeader">Tab item header</param>
        private void SelectedTabItem(string tabItemHeader)
        {
            foreach (TabItem ti in MonitorWindowsContainer.Items)
            {
                if (ti.Header.ToString() == tabItemHeader)
                {
                    ti.IsSelected = true;
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// Can new test pass execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Can execute routed event args</param>
        private void CanNewTestPassExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// New test pass execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Executed routed event args</param>
        private void NewTestPassExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.NewProject();
        }

        /// <summary>
        /// Settings execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Executed routed event args</param>
        private void SettingsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var ntp = new Settings();
            ntp.ShowDialog();
        }

        /// <summary>
        /// Run selection execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Execute routed event args</param>
        private void RunSelectionExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.ScheduleTestPass(this.testCasesViewModel.RunLocally);
        }

        /// <summary>
        /// Run selection on clients execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Execute routed event args</param>
        private void RunSelectionOnClientsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.ScheduleTestPass(this.testCasesViewModel.RunOnClients);
        }

        private void ScheduleTestPass(Action action)
        {
            if (this.testCasesViewModel.GetCheckedTestCasesCount() == 0)
            {
                MessageBox.Show("Please select a test case!");
            }
            else
            {
                var settingsDialog = new ScheduleSettings();
                if (settingsDialog.ShowDialog() == true)
                {
                    this.RefreshTestPassMonitorTab();

                    this.testCasesViewModel.InitializeTestRun();

                    action();
                }
            }
        }

        /// <summary>
        /// Export selection to Excel execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Execute routed event args</param>
        private void ExportSelectionToExcelExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.testCasesViewModel.ExportSelectionToExcel();
        }

        /// <summary>
        /// Remove all logs execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Execute routed event args</param>
        private void RemoveAllLogsExecute(object sender, ExecutedRoutedEventArgs e)
        {
            LoggerViewModel.Clear();
        }

        /// <summary>
        /// New a project
        /// </summary>
        private void NewProject()
        {
            //if (this.ClientVisibility.Equals(Visibility.Collapsed))
            //{
            //    this.ClientVisibility = System.Windows.Visibility.Visible;
            //}
            //else
            //{
            //    this.ClientVisibility = System.Windows.Visibility.Collapsed;
            //}

            var ntp = new NewTestPass();
            ntp.ShowDialog();
        }

        /// <summary>
        /// Open a project
        /// </summary>
        private void OpenProject()
        {
            this.testCasesViewModel.CreateClient();
            //NewTestPass ntp = new NewTestPass();
            //ntp.ShowDialog();
        }

        private void FocusOutput(object sender, RoutedEventArgs e)
        {
            if (this.LeftGrid.RowDefinitions[1].Height != new GridLength(33))
            {
                this.LeftGrid.RowDefinitions[1].Height = new GridLength(33);
            }
            else
            {
                this.LeftGrid.RowDefinitions[1].Height = new GridLength(200);
            }
        }

        void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            loggerWrapper.ScrollToBottom();
        }

        private void Sort(object sender, MouseButtonEventArgs e)
        {
            var column = (sender as TextBlock).Text;
            this.testCasesViewModel.Sort(column);
        }
    }
}