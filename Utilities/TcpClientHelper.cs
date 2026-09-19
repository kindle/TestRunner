//-------------------------------------------------------------------------------------------------
// <copyright file="TcpClientHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using Microsoft.Win32;

namespace TestViewer.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Windows.Media;
    
    using TestViewer.ViewModels;

    /// <summary>
    /// Tcp client helper class
    /// </summary>
    public class TcpClientHelper
    {
        private TcpClient tcp;
        private Thread thread;
        private Encoding encoding = Encoding.UTF8;
        private NetworkStream ns;
        private bool threadStopper;

        public TcpClientHelper()
        {
            tcp = new TcpClient();

            if (!tcp.Connected)
            {
                try
                {
                    // check ip and port are set correctly before start this!!!
                    tcp.Connect(TestCasesViewModel.ServerIPAddress, int.Parse(TestCasesViewModel.ServerPort));
                    thread = new Thread(new ThreadStart(WorkerThread));
                    ns = tcp.GetStream();

                    thread.Start();
                }
                catch
                {
                    LoggerViewModel.Log("Cannot connect to the server, check ip/port are correct, network cable and so on!", Colors.Red);
                }
                finally
                {
                    if (tcp.Connected)
                    {
                        // client info: machinename/user/IP/OS
                        SendMessageToServer("CLIENTINFO$" + string.Join(",", 
                            Environment.MachineName, 
                            GetClientInfo() + " " + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")));
                    }
                }
            }
        }

        private string GetClientInfo()
        {
            string returnValue = "IE" + GetIEVersion() + " " + Environment.OSVersion.VersionString;
            
            // todo: show win7,win8,server2008 later
            
            // Server 2008
            /*if (osVersion.Version.ToString().Contains("6.1.76"))
            {
                returnValue += "Server2008 ";
                if (osVersion.ToString().Contains("Service Pack 1"))
                {
                    returnValue += "SP1 ";
                }
            }
            else
            {
                returnValue += osVersion.ToString();
            }*/


            // win7
            //if (osVersion.ToString().Contains(""))
            //{
            //   returnValue = "Win7";
            //}

            return returnValue;
        }

        private string GetIEVersion()
        {
            string key = @"Software\Microsoft\Internet Explorer";
            RegistryKey dkey = Registry.LocalMachine.OpenSubKey(key, false);
            string data = dkey.GetValue("Version").ToString().Split('.')[0];
            return data;
        }

        private void WorkerThread()
        {
            byte[] recvBytes;
            int BufferSize = 256;
            threadStopper = false;
            while (!threadStopper)
            {
                if (ns.DataAvailable)
                {
                    List<byte> list = new List<byte>();

                    recvBytes = new byte[BufferSize];

                    int endFlag = 0;
                    do
                    {
                        int len = ns.Read(recvBytes, 0, BufferSize);
                        if (len == BufferSize)
                        {
                            list.AddRange(recvBytes);
                            endFlag = 1;
                        }
                        else
                        {
                            //this is the end 
                            for (int index = 0; index < len; index++)
                                list.Add(recvBytes[index]);
                            endFlag = 0;
                        }
                    } while (endFlag == 1);

                    recvBytes = list.ToArray();

                    //client send out msg
                    string recvMsg = encoding.GetString(recvBytes);

                    if (recvMsg.IndexOf("UPDATETESTBITS$") == 0)
                    {
                        LoggerViewModel.Log("UPDATETESTBITS...");
                        string[] recvStr = recvMsg.Split('$');
                        LoggerViewModel.Log("UPDATETESTBITS:" + recvStr[1]);
                        this.UpdateTestBits(recvStr[1], recvStr[2]/*.Replace("@","\\")*/);
                    }
                    else if (recvMsg.IndexOf("RUNTEST$") == 0)
                    {
                        string[] recvStr = recvMsg.Split('$');
                        this.RunTestCase(recvStr[1]);
                        LoggerViewModel.Log("Start execution: " + recvStr[1]);
                    }
                    else if (recvMsg.IndexOf("CLIENTNAME$") == 0)
                    {
                        ;
                    }
                    else
                    {
                        //Log(recvMsg);
                    }
                    LoggerViewModel.Log(recvMsg);
                }
                Thread.Sleep(100);
            }
        }
        
        private void UpdateTestBits(string dllUrl, string testSettingsUrl)
        {
            TestCasesViewModel.SourceTestDllUrl = dllUrl;
            TestCasesViewModel.SourceTestSettingsUrl = testSettingsUrl;

            string sourceUrl = TestCasesViewModel.SourceTestDllUrl;
            string dllFileName = sourceUrl.Substring(sourceUrl.LastIndexOf("\\") + 1);
            string testSettingsFileName = TestCasesViewModel.SourceTestSettingsUrl.Substring(TestCasesViewModel.SourceTestSettingsUrl.LastIndexOf("\\") + 1);
            string sourceDirectory = sourceUrl.Substring(0, sourceUrl.LastIndexOf("\\")) + "\\*";
            string destDirectory = System.Environment.CurrentDirectory + "\\TestBits" + DateTime.Now.ToString("_yyyy_MM_dd_hh_mm_ss");

            TestCasesViewModel.LocalTestDllUrl = destDirectory + "\\" + dllFileName;
            TestCasesViewModel.LocalTestSettingsUrl = destDirectory + "\\" + testSettingsFileName;

            /*
             * /E Copy folders and subfolders, including Empty folders. 
             * /Y (Windows 2000 only) Suppress prompt to confirm overwriting a file.
             * /I If in doubt always assume the destination is a folder e.g. when the destination does not exist.
             */

            string cmdArgs = "@/k ECHO OFF ";
            cmdArgs += "& xcopy \"" + sourceDirectory + "\" \"" + destDirectory + "\" /E /I /Y ";
            if (TestCasesViewModel.SourceTestSettingsUrl != "")
            {
                cmdArgs += "& xcopy \"" + TestCasesViewModel.SourceTestSettingsUrl + "\" \"" + destDirectory + "\" ";
                LoggerViewModel.Log(string.Format("Copying test settings file from {0} to {1} ...", TestCasesViewModel.SourceTestSettingsUrl, TestCasesViewModel.LocalTestSettingsUrl));
            }
            cmdArgs += "& exit";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";
            psi.Arguments = cmdArgs;
            psi.UseShellExecute = false; 
            psi.CreateNoWindow = true;

            Process proc = new Process();
            
            proc.EnableRaisingEvents = true;
            proc.StartInfo = psi;

            proc.Exited += (sender, e) =>
            {
                int exitCode = (sender as Process).ExitCode;

                if (!exitCode.ToString().Equals("0"))
                {
                    //throw new ApplicationException("Copy file error!");
                    SendMessageToServer("CLIETNCOPYFILEERROR$" + sourceDirectory);
                    return;
                }

                SendMessageToServer("TESTBITSCOPYCOMPLETE$");
                
            };

            proc.Start();

            SendMessageToServer("TOSERVERMESSAGE$" + string.Format("Copying test bits from {1} to [{0}]\\{2}...", Environment.MachineName, sourceDirectory, destDirectory));
        }

        private void RunTestCase(string id)
        {
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
            //    cmdArgs += @"& call ""C:\Program Files\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"" ";
            //}

            //cmdArgs += @"& call """ + MsTestHelper.GetCommandPromptPath() + @""" ";

            // set mstest working directory
            // move to create property exact a setting option for it in the future
            string testResultOutputFolder = SettingsHelper.GetDefaultTestResultFolder();
            //@"c:\TestResults";

            if (!System.IO.Directory.Exists(testResultOutputFolder))
            {
                System.IO.Directory.CreateDirectory(testResultOutputFolder);
            }

            Environment.CurrentDirectory = testResultOutputFolder;

            //const string WorkingDirectory = @"C:\Users\v-bawei\Desktop\Release\haisha\";
            //const string TestDll = "KinectCodedUITest.dll";
            
            string resultFile = Environment.UserName + "_" + Environment.MachineName + " " + DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss") + ".trx";
            //cmdArgs += "& mstest /testcontainer:" + WorkingDirectory + TestDll + " /test:" + command + " /unique /resultsfile:\"" + resultFile + "\" ";
            cmdArgs += @"& """ + MsTestHelper.GetMsTestPath() + @""" /testcontainer:""" + TestCasesViewModel.LocalTestDllUrl + @""" /test:" + id + @" /unique /resultsfile:""" + resultFile + @""" ";

            if (System.IO.File.Exists(TestCasesViewModel.LocalTestSettingsUrl))
            {
                cmdArgs += @"/testsettings:""" + TestCasesViewModel.LocalTestSettingsUrl + @"""";
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
                string resultInnerXml = "";

                if (!System.IO.File.Exists(resultFile))
                {
                    SendMessageToServer("TRXNOTGENERATED$");
                }
                else
                {
                    resultInnerXml = XmlHelper.GetInnerXmlFromTrx(resultFile, "/TestRun/Results");
                }
                //LoggerViewModel.Log(resultInnerXml);
                //int exitCode = (sender as Process).ExitCode;

                // tell server the result
                SendMessageToServer("RUNTESTRESULT$" + resultInnerXml);
                SendMessageToServer("RUNTESTFINISH$" + "");
            };

            myProcess.StartInfo = psi;
            myProcess.Start();
            
        }

        public void SendMessageToServer(string msg)
        {
            if (!tcp.Connected) return;

            byte[] bytes = encoding.GetBytes(msg);
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(msg);
            try
            {
                ns.Write(bytes, 0, bytes.Length);
            }
            catch// (Exception ex)
            {
                LoggerViewModel.Log("Can not connect to the Server, please check the following, refer to remote....!");
                //Log(ex.Message);
            }
            finally
            {
                //client receive msg
                string sentMsg = encoding.GetString(bytes);
                if (sentMsg.IndexOf("CLIENTNAME$") == 0 ||
                    sentMsg.IndexOf("JUMPACTION$") == 0)
                {
                    ;
                }
                //else
                //    Log(Settings.ItemCollection.PlayerNickName + ": " + sentMsg);
            }

            Thread.Sleep(1000);
        }

        public void Dispose()
        {
            SendMessageToServer("DISCONNECT$");
            tcp.Close();
            threadStopper = true;
        }
    }
}
