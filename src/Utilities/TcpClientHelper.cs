//-------------------------------------------------------------------------------------------------
// <copyright file="TcpClientHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using Microsoft.Win32;

namespace TestRunner.Utilities
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Media;
    
    using TestRunner.ViewModels;

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

        /// <summary>
        /// Gets a value indicating whether the client successfully connected to the server
        /// </summary>
        public bool IsConnected
        {
            get { return this.tcp != null && this.tcp.Connected; }
        }

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
                    thread.IsBackground = true;
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
            threadStopper = false;
            while (!threadStopper)
            {
                if (ns.DataAvailable)
                {
                    try
                    {
                        recvBytes = ReadMessage(ns);
                    }
                    catch (Exception ex)
                    {
                        LoggerViewModel.Log("Server connection closed: " + ex.Message, Colors.Red);
                        threadStopper = true;
                        tcp.Close();
                        continue;
                    }

                    if (IsTestBitsMessage(recvBytes))
                    {
                        this.ReceiveTestBits(recvBytes);
                        continue;
                    }

                    //client send out msg
                    string recvMsg = encoding.GetString(recvBytes);

                    if (recvMsg.IndexOf("RUNTEST$") == 0)
                    {
                        string[] recvStr = recvMsg.Split('$');
                        try
                        {
                            this.RunTestCase(recvStr[1]);
                            LoggerViewModel.Log("Start execution: " + recvStr[1]);
                        }
                        catch (Exception ex)
                        {
                            LoggerViewModel.Log("Failed to start test: " + ex.Message, Colors.Red);
                            SendMessageToServer("RUNTESTERROR$" + ex.Message);
                        }
                    }
                    else if (recvMsg.IndexOf("CLIENTNAME$") == 0)
                    {
                        ;
                    }
                    else if (recvMsg.IndexOf("SERVERDISCONNECT$") == 0)
                    {
                        LoggerViewModel.Log("Server disconnected.");
                        threadStopper = true;
                        tcp.Close();
                    }
                    else
                    {
                        //Log(recvMsg);
                    }
                    if (recvMsg.IndexOf("RUNTEST$") != 0)
                    {
                        LoggerViewModel.Log(recvMsg);
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static byte[] ReadMessage(NetworkStream stream)
        {
            byte[] lengthBytes = ReadExactly(stream, sizeof(int));
            int messageLength = BitConverter.ToInt32(lengthBytes, 0);
            if (messageLength < 0)
            {
                throw new InvalidDataException("Invalid TCP message length.");
            }

            return ReadExactly(stream, messageLength);
        }

        private static byte[] ReadExactly(NetworkStream stream, int count)
        {
            byte[] bytes = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int bytesRead = stream.Read(bytes, offset, count - offset);
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("The TCP connection closed during a message.");
                }

                offset += bytesRead;
            }

            return bytes;
        }

        private static bool IsTestBitsMessage(byte[] message)
        {
            byte[] prefix = Encoding.UTF8.GetBytes("TESTBITS$");
            if (message.Length < prefix.Length)
            {
                return false;
            }

            for (int index = 0; index < prefix.Length; index++)
            {
                if (message[index] != prefix[index])
                {
                    return false;
                }
            }

            return true;
        }

        private void ReceiveTestBits(byte[] message)
        {
            byte[] prefix = encoding.GetBytes("TESTBITS$");
            string destinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestBit");

            try
            {
                Directory.CreateDirectory(destinationDirectory);
                using (MemoryStream payload = new MemoryStream(message, prefix.Length, message.Length - prefix.Length))
                using (BinaryReader reader = new BinaryReader(payload, encoding))
                {
                    string dllFileName = reader.ReadString();
                    string testSettingsFileName = reader.ReadString();
                    int fileCount = reader.ReadInt32();
                    string destinationRoot = Path.GetFullPath(destinationDirectory) + Path.DirectorySeparatorChar;

                    for (int index = 0; index < fileCount; index++)
                    {
                        string relativePath = reader.ReadString();
                        long fileLength = reader.ReadInt64();
                        string destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, relativePath));
                        if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidDataException("Invalid test bit path: " + relativePath);
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        using (FileStream file = File.Create(destinationPath))
                        {
                            CopyBytes(reader.BaseStream, file, fileLength);
                        }
                    }

                    TestCasesViewModel.LocalTestDllUrl = Path.Combine(destinationDirectory, dllFileName);
                    TestCasesViewModel.LocalTestSettingsUrl = string.IsNullOrEmpty(testSettingsFileName)
                        ? string.Empty
                        : Path.Combine(destinationDirectory, testSettingsFileName);
                }

                SendMessageToServer("TOSERVERMESSAGE$" + string.Format("Received test bits via TCP at [{0}]\\{1}.", Environment.MachineName, destinationDirectory));
                SendMessageToServer("TESTBITSCOPYCOMPLETE$");
            }
            catch (Exception ex)
            {
                SendMessageToServer("CLIETNCOPYFILEERROR$" + ex.Message);
            }
        }

        private static void CopyBytes(Stream source, Stream destination, long count)
        {
            byte[] buffer = new byte[81920];
            while (count > 0)
            {
                int bytesRead = source.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                if (bytesRead == 0)
                {
                    throw new EndOfStreamException("The TCP file payload ended unexpectedly.");
                }

                destination.Write(buffer, 0, bytesRead);
                count -= bytesRead;
            }
        }

        private void RunTestCase(string id)
        {
            ProcessStartInfo psi = new ProcessStartInfo();

            // set mstest working directory
            // move to create property exact a setting option for it in the future
            string testResultOutputFolder = SettingsHelper.GetDefaultTestResultFolder();
            //@"c:\TestResults";

            if (!System.IO.Directory.Exists(testResultOutputFolder))
            {
                System.IO.Directory.CreateDirectory(testResultOutputFolder);
            }

            Environment.CurrentDirectory = testResultOutputFolder;

            string resultFileName = Environment.UserName + "_" + Environment.MachineName + " " + DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss") + ".trx";
            string resultFile = Path.Combine(testResultOutputFolder, resultFileName);
            string testApplicationPath = Path.ChangeExtension(TestCasesViewModel.LocalTestDllUrl, ".exe");

            if (File.Exists(testApplicationPath))
            {
                string testName = id.Substring(id.LastIndexOf('.') + 1);
                psi.FileName = testApplicationPath;
                psi.Arguments = string.Format(
                    "--filter \"Name={0}\" --report-trx --report-trx-filename \"{1}\" --results-directory \"{2}\"",
                    testName,
                    resultFileName,
                    testResultOutputFolder);

                if (File.Exists(TestCasesViewModel.LocalTestSettingsUrl))
                {
                    psi.Arguments += " --settings \"" + TestCasesViewModel.LocalTestSettingsUrl + "\"";
                }
            }
            else
            {
                psi.FileName = MsTestHelper.GetMsTestPath();
                psi.Arguments = "/testcontainer:\"" + TestCasesViewModel.LocalTestDllUrl + "\" /test:" + id + " /unique /resultsfile:\"" + resultFile + "\"";

                if (File.Exists(TestCasesViewModel.LocalTestSettingsUrl))
                {
                    psi.Arguments += " /testsettings:\"" + TestCasesViewModel.LocalTestSettingsUrl + "\"";
                }
            }

            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            StringBuilder processOutput = new StringBuilder();
            Process myProcess = new Process();
            myProcess.EnableRaisingEvents = true;
            myProcess.OutputDataReceived += (sender, e) => { if (e.Data != null) processOutput.AppendLine(e.Data); };
            myProcess.ErrorDataReceived += (sender, e) => { if (e.Data != null) processOutput.AppendLine(e.Data); };
            myProcess.Exited += (sender, e) =>
            {
                try
                {
                    int exitCode = (sender as Process).ExitCode;
                    string actualResultFile = resultFile;
                    if (!File.Exists(actualResultFile))
                    {
                        // mstest's new testing platform runner does not always honor --report-trx-filename,
                        // so fall back to the actual trx path reported in the process output.
                        Match match = Regex.Match(processOutput.ToString(), @"-\s*(.+\.trx)\s*$", RegexOptions.Multiline);
                        if (match.Success && File.Exists(match.Groups[1].Value.Trim()))
                        {
                            actualResultFile = match.Groups[1].Value.Trim();
                        }
                    }

                    if (!File.Exists(actualResultFile))
                    {
                        SendMessageToServer(string.Format("RUNTESTERROR$Result file was not generated for {0}. Test runner exit code: {1}. Output: {2}", id, exitCode, processOutput.ToString().Trim()));
                        return;
                    }

                    string resultInnerXml = XmlHelper.GetInnerXmlFromTrx(actualResultFile, "/TestRun/Results");
                    if (resultInnerXml == string.Empty)
                    {
                        SendMessageToServer(string.Format("RUNTESTERROR$No test result was generated for {0}. Test runner exit code: {1}.", id, exitCode));
                    }
                    else
                    {
                        LogTestResult(resultInnerXml);
                        SendMessageToServer("RUNTESTRESULT$" + resultInnerXml);
                    }
                }
                catch (Exception ex)
                {
                    SendMessageToServer("RUNTESTERROR$Failed to read test result: " + ex.Message);
                }
                finally
                {
                    SendMessageToServer("RUNTESTFINISH$");
                }
            };

            myProcess.StartInfo = psi;
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();
        }

        private static void LogTestResult(string resultsInnerXml)
        {
            LoggerViewModel.Log(resultsInnerXml);
            string testID = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "testName");
            string outCome = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "outcome");
            string errorMessage = XmlHelper.GetInnerTextFromXml(resultsInnerXml, "UnitTestResult/Output/ErrorInfo/Message");
            Color logColor = Colors.Black;

            switch (outCome)
            {
                case "Failed":
                    logColor = Colors.Red;
                    break;
                case "Inconclusive":
                    logColor = Colors.Orange;
                    break;
                case "Passed":
                    logColor = Colors.Green;
                    break;
                case "Timeout":
                    logColor = Colors.DeepSkyBlue;
                    break;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                LoggerViewModel.Log(string.Format("ErrorMessage: {0}", errorMessage), Colors.Red);
            }

            LoggerViewModel.Log(string.Format("{0} {1} [{2}].", testID, outCome, Environment.MachineName), logColor);
        }

        public void SendMessageToServer(string msg)
        {
            if (!tcp.Connected) return;

            byte[] bytes = encoding.GetBytes(msg);
            try
            {
                byte[] length = BitConverter.GetBytes(bytes.Length);
                lock (ns)
                {
                    ns.Write(length, 0, length.Length);
                    ns.Write(bytes, 0, bytes.Length);
                }
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
            threadStopper = true;
            if (tcp != null && tcp.Connected)
            {
                SendMessageToServer("DISCONNECT$");
            }

            if (tcp != null)
            {
                tcp.Close();
            }

            if (thread != null && thread.IsAlive)
            {
                thread.Join(1000);
            }
        }
    }
}
