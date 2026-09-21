//-------------------------------------------------------------------------------------------------
// <copyright file="TcpServerHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using TestRunner.Models;
    using TestRunner.ViewModels;
    using System.Reflection;

    /// <summary>
    /// Tcp server helper class
    /// </summary>
    public class TcpServerHelper
    {
        private bool enableCrossThreadLog = true;
        public int MaxClientNumber = 4;
        private TcpListener listener;
        private Thread thread;
        private List<TCPClientInfo> clients;
        private Encoding encoding = Encoding.UTF8;
        private bool threadStopper;

        private ICollectionView _testCasesModelICollectionView;

        public TcpServerHelper(ICollectionView testCasesModelICollectionView)
        {
            _testCasesModelICollectionView = testCasesModelICollectionView;

            clients = new List<TCPClientInfo>();
            listener = new TcpListener(IPAddress.Any, int.Parse(TestCasesViewModel.ServerPort));
            listener.Start();
            LoggerViewModel.Log(string.Format("Server created successfully! IP address: {0}, port: {1}.", SettingsHelper.GetLocalIPAddress(), TestCasesViewModel.ServerPort));
            thread = new Thread(WorkerThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void WorkerThread()
        {
            byte[] recvBytes;
            threadStopper = false;
            while (!threadStopper)
            {
                if (listener.Pending())
                {
                    TcpClient tcp = listener.AcceptTcpClient();
                    TCPClientInfo ci = new TCPClientInfo();
                    ci.Client = tcp;
                    ci.PlayerTag = "none";

                    if (clients.Count < MaxClientNumber)//tbd here!!!!!!
                    {
                        clients.Add(ci);
                    }
                    else
                    {
                        try
                        {
                            byte[] bytes = encoding.GetBytes("Server is full.");
                            SendMessageToClient(tcp, bytes);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < clients.Count; i++)
                    {
                        TcpClient client = clients[i].Client;
                        if (!client.Connected)
                        {
                            //byte[] b = encoding.GetBytes(clients[i].ClientName + " has left the game.");
                            //BroadCasting(i, b);
                            clients.RemoveAt(i);
                        }
                        else// if connected
                        {
                            NetworkStream ns = client.GetStream();
                            if (ns.DataAvailable)
                            {
                                try
                                {
                                    recvBytes = ReadMessage(ns);
                                }
                                catch (Exception ex)
                                {
                                    string machineName = clients[i].ClientName;
                                    client.Close();
                                    MarkClientDisconnected(machineName);
                                    clients.RemoveAt(i);
                                    i--;

                                    if (enableCrossThreadLog)
                                        LoggerViewModel.Log(string.Format("Client machine: {0} connection closed: {1}", machineName, ex.Message), Colors.Red);

                                    continue;
                                }

                                string recvMsg = encoding.GetString(recvBytes);


                                if (recvMsg.IndexOf("TOSERVERMESSAGE$") == 0)
                                {
                                    string message = recvMsg.Split('$')[1];
                                    
                                    if (enableCrossThreadLog)
                                        LoggerViewModel.Log(message);
                                }
                                else if (recvMsg.IndexOf("TESTBITSCOPYCOMPLETE$") == 0)
                                {
                                    string machineName = clients[i].ClientName;
                                        //recvMsg.Split('$')[1];

                                    for (int index = 0; index < TestMachinesViewModel.ClientsModel.Count; index++)
                                    {
                                        if (TestMachinesViewModel.ClientsModel[index].Name.Equals(machineName))
                                        {
                                            TestMachinesViewModel.ClientsModel[index].IsTestBitsUpdated = true;
                                            TestMachinesViewModel.ClientsModel[index].State = TestMachineState.Free;

                                            ////or will copy test bits twice
                                            //Thread.Sleep(1000);
                                            if (enableCrossThreadLog)
                                                LoggerViewModel.Log(string.Format("[{0}] copy test bits completed.", machineName));
                                            break;
                                        }
                                    } 
                                }
                                else if (recvMsg.IndexOf("CLIENTINFO$") == 0)
                                {
                                    TCPClientInfo clientInfo = new TCPClientInfo();
                                    var messages = recvMsg.Split('$')[1].Split(',');
                                    clientInfo.ClientName = messages[0];
                                    clientInfo.ClientOS = messages[1];
                                    clientInfo.Client = clients[i].Client;

                                    clients[i] = clientInfo;

                                    try
                                    {
                                        bool alreadyInMachineList = false;
                                        foreach (var machine in TestMachinesViewModel.ClientsModel)
                                        {
                                            if (machine.Name.Equals(clientInfo.ClientName))
                                            {
                                                alreadyInMachineList = true;
                                                machine.State = TestMachineState.Free;
                                                machine.IsChecked = true;
                                                break;
                                            }
                                        }

                                        if (!alreadyInMachineList)
                                        {
                                            TestMachinesViewModel.ClientsModel.Add(
                                                new TestMachine()
                                                {
                                                    Name = clientInfo.ClientName,
                                                    Tag = "Machine",
                                                    State = TestMachineState.Free,
                                                    IsChecked = true
                                                }
                                                );
                                        }

                                        TestMachineTreeViewHelper.SaveSettings();

                                        if (enableCrossThreadLog)
                                            LoggerViewModel.Log(string.Format("{0} is connected.", clientInfo.ClientName));
                                    }
                                    catch (TargetInvocationException ex)
                                    {
                                        if (enableCrossThreadLog)
                                            LoggerViewModel.Log(string.Format("Throw a targetInvocationException [TcpServerHelper][CLIENTINFO$]: {0}.", ex.Message), Colors.Red);
                                    }
                                    catch (Exception e)
                                    {
                                        //throw new Exception(e.ToString());
                                        if (enableCrossThreadLog)
                                            LoggerViewModel.Log(string.Format("Throw an exception [TcpServerHelper][CLIENTINFO$]: {0}.", e.Message), Colors.Red);
                                    }

                                    byte[] b0 = encoding.GetBytes("You've connected to the server!");
                                    SendMessageToClient(i, b0);

                                    //byte[] b1 = encoding.GetBytes(clients[i].ClientName + " connected to the server!");
                                    //BroadCasting(i, b1);

                                    //Thread.Sleep(100);
                                    //BOARDREADY$TotalPlayers,HoleStatus.Playeri
                                    //byte[] b2 = encoding.GetBytes("BOARDREADY$" + "2" + "," + clients[i].PlayerTag);
                                    //SingleCasting(i, b2);
                                }
                                else if (recvMsg.IndexOf("CLIETNCOPYFILEERROR$") == 0)
                                {
                                    var error = recvMsg.Split('$')[1];
                                    var logMessage = string.Format("Client machine: {0} failed to receive test bits: {1}.", clients[i].ClientName, error);
                                    if (enableCrossThreadLog)
                                        LoggerViewModel.Log(logMessage);

                                    // only this client's tests should be failed.
                                    foreach (TestCase t in this._testCasesModelICollectionView)
                                    {
                                        t.State = TestCaseState.Inconclusive;
                                        t.BriefErrorMessage = logMessage;
                                        t.ClientMachineName = clients[i].ClientName;
                                    }
                                }
                                else if (recvMsg.IndexOf("TRXNOTGENERATED$") == 0)
                                {
                                    foreach (TestCase t in this._testCasesModelICollectionView)
                                    {
                                        t.State = TestCaseState.Failed;
                                        t.BriefErrorMessage = "Result file not generated.";
                                        t.ClientMachineName = clients[i].ClientName;
                                    }
                                }
                                else if (recvMsg.IndexOf("RUNTESTERROR$") == 0)
                                {
                                    string errorMessage = recvMsg.Substring("RUNTESTERROR$".Length);
                                    foreach (TestCase t in this._testCasesModelICollectionView)
                                    {
                                        if (t.State == TestCaseState.Running && t.ClientMachineName == clients[i].ClientName)
                                        {
                                            t.State = TestCaseState.Failed;
                                            t.BriefErrorMessage = errorMessage;
                                            t.ErrorMessage = errorMessage;
                                            break;
                                        }
                                    }

                                    FreeClient(clients[i].ClientName);
                                    if (enableCrossThreadLog)
                                        LoggerViewModel.Log(string.Format("[{0}] failed to start test: {1}", clients[i].ClientName, errorMessage), Colors.Red);
                                }
                                else if (recvMsg.IndexOf("RUNTESTRESULT$") == 0)
                                {
                                    DealWithMessage(recvMsg, clients[i]);
                                }
                                else if (recvMsg.IndexOf("RUNTESTFINISH$") == 0)
                                {
                                    string machineName = clients[i].ClientName;
                                    FreeClient(machineName);
                                }
                                else if (recvMsg.IndexOf("DISCONNECT$") == 0)
                                {
                                    string machineName = clients[i].ClientName;
                                    clients[i].Client.Close();
                                    MarkClientDisconnected(machineName);
                                    if (enableCrossThreadLog)
                                        LoggerViewModel.Log(string.Format("Client machine: {0} disconnected.", machineName));

                                    clients.RemoveAt(i);
                                    i--;
                                }
                                else
                                {
                                    //Log(recvMsg);
                                    //byte[] b = encoding.GetBytes(clients[i].ClientName + ": " + recvMsg);
                                    //BroadCasting(i, b);
                                }

                                //LoggerViewModel.Log(recvMsg, Colors.LightSkyBlue);
                            }
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private static void MarkClientDisconnected(string machineName)
        {
            for (int index = 0; index < TestMachinesViewModel.ClientsModel.Count; index++)
            {
                if (TestMachinesViewModel.ClientsModel[index].Name.Equals(machineName))
                {
                    TestMachinesViewModel.ClientsModel[index].State = TestMachineState.Disconnected;
                    break;
                }
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

        private void FreeClient(string machineName)
        {
            for (int index = 0; index < TestMachinesViewModel.ClientsModel.Count; index++)
            {
                if (TestMachinesViewModel.ClientsModel[index].Name.Equals(machineName))
                {
                    TestMachinesViewModel.ClientsModel[index].State = TestMachineState.Free;
                    break;
                }
            }
        }

        private void DealWithMessage(string recvMsg, TCPClientInfo client)
        {
            try
            {
                string testID = "";

                string resultsInnerXml = recvMsg.Replace("RUNTESTRESULT$", "");
                LoggerViewModel.Log(resultsInnerXml);
                testID = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "testName");
                var outCome = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "outcome");
                var duration = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "duration");
                duration = duration == "" ? "00:00:00" : duration;
                var startTime = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "startTime");
                var endTime = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult", "endTime");
                var errorMessage = XmlHelper.GetInnerTextFromXml(resultsInnerXml, "UnitTestResult/Output/ErrorInfo/Message");
                var stdOut = XmlHelper.GetInnerTextFromXml(resultsInnerXml, "/UnitTestResult/Output/StdOut");
                var stackTrace = XmlHelper.GetInnerTextFromXml(resultsInnerXml, "/UnitTestResult/Output/StackTrace");
                var resultFiles = XmlHelper.GetAttributeValueFromXml(resultsInnerXml, "/UnitTestResult/ResultFiles/ResultFile", "path");
                //var osInfo = "";
                //var ieInfo = "";
                var logColor = Colors.Black;
                // update server test case state
                foreach (TestCase t in this._testCasesModelICollectionView)
                {
                    if (t.ID.EndsWith("." + testID))
                    {
                        switch (outCome)
                        {
                            /// need to add timeout as local run merg into 1 methord in the future....
                            case "Aborted":
                                t.State = TestCaseState.Aborted;
                                break;
                            case "Failed":
                                t.State = TestCaseState.Failed;
                                logColor = Colors.Red;
                                break;
                            case "Inconclusive":
                                t.State = TestCaseState.Inconclusive;
                                logColor = Colors.Orange;
                                break;
                            case "NotExecuted":
                                t.State = TestCaseState.NotExecuted;
                                break;
                            case "Passed":
                                t.State = TestCaseState.Passed;
                                logColor = Colors.Green;
                                break;
                            case "PassedButRunAborted":
                                t.State = TestCaseState.PassedButRunAborted;
                                break;
                            case "Timeout":
                                t.State = TestCaseState.Timeout;
                                logColor = Colors.DeepSkyBlue;
                                break;
                            default:
                                //error
                                t.State = TestCaseState.NotSet;
                                logColor = Colors.Red;
                                break;
                        }

                        t.Duration = Convert.ToDateTime(duration);
                        t.StartTime = Convert.ToDateTime(startTime);
                        t.EndTime = Convert.ToDateTime(endTime);
                        t.BriefErrorMessage = errorMessage.Replace("\r", "").Replace("\n", "");
                        t.ErrorMessage = errorMessage;
                        t.StdOut = stdOut;
                        t.StackTrace = stackTrace;
                        t.ClientMachineName = client.ClientName;
                        t.ClientMachineInfo = string.Format("{0} {1}", client.ClientOS, client.ClientBrowser);

                        break;
                    }
                }

                if (enableCrossThreadLog && !string.IsNullOrEmpty(errorMessage))
                    LoggerViewModel.Log(string.Format("ErrorMessage: {0}", errorMessage), Colors.Red);

                if (enableCrossThreadLog)
                    LoggerViewModel.Log(string.Format("{0} {1} [{2}].", testID, outCome, client.ClientName), logColor);
            }
            // bug of VS
            catch (TargetInvocationException ex)
            {
                if (enableCrossThreadLog)
                    LoggerViewModel.Log(string.Format("Throw a targetInvocationException [TcpServerHelper][RUNTESTRESULT$]: {0}.", ex.Message), Colors.Red);

                MarkRunningTestFailed(client, ex.Message);
            }
            catch (Exception ex)
            {
                if (enableCrossThreadLog)
                {
                    LoggerViewModel.Log(
                        string.Format(
                            "Throw an exception [TcpServerHelper][RUNTESTRESULT$]: {0}.",
                            ex.Message), Colors.Red);
                    LoggerViewModel.Log(
                        string.Format(
                            "xml:{0}.",
                            recvMsg), Colors.SlateGray);
                }

                MarkRunningTestFailed(client, ex.Message);
            }
        }

        private void MarkRunningTestFailed(TCPClientInfo client, string errorMessage)
        {
            foreach (TestCase testCase in this._testCasesModelICollectionView)
            {
                if (testCase.State == TestCaseState.Running && testCase.ClientMachineName == client.ClientName)
                {
                    testCase.State = TestCaseState.Failed;
                    testCase.BriefErrorMessage = errorMessage;
                    testCase.ErrorMessage = errorMessage;
                    testCase.ClientMachineName = client.ClientName;
                    testCase.ClientMachineInfo = string.Format("{0} {1}", client.ClientOS, client.ClientBrowser);
                    break;
                }
            }
        }

        public void BootClientMachine(string machineName)
        {
            for (int i = 0; i < clients.Count;i++ )
            {
                if (clients[i].ClientName.Equals(machineName))
                {
                    byte[] b0 = encoding.GetBytes("You are booted!");
                    SendMessageToClient(i, b0);

                    clients[i].Client.Client.Disconnect(false);
                    break;
                }
            }

            for (int index = 0; index < TestMachinesViewModel.ClientsModel.Count; index++)
            {
                if (TestMachinesViewModel.ClientsModel[index].Name.Equals(machineName))
                {
                    TestMachinesViewModel.ClientsModel[index].State = TestMachineState.Disconnected;
                    break;
                }
            }

            LoggerViewModel.Log(string.Format("Client machine: {0} is booted.", machineName));
        }

        //private void SendMessageToAllClients(int self, byte[] bytes)
        //{
        //    for (int i = 0; i < clients.Count; i++)
        //    {
        //        if (i != self)
        //        {
        //            SendMessageToClient(i, bytes);
        //        }
        //    }
        //}

        private void SendMessageToClient(int target, byte[] bytes)
        {
            SendMessageToClient(clients[target].Client, bytes);
        }

        private void SendMessageToClient(TcpClient tcp, byte[] bytes)
        {
            NetworkStream ns = tcp.GetStream();
            try
            {
                byte[] length = BitConverter.GetBytes(bytes.Length);
                lock (ns)
                {
                    ns.Write(length, 0, length.Length);
                    ns.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Thread.Sleep(100);
        }

        public void SendTestBitsToClientByMachineName(string machineName, string dllPath, string testSettingsPath)
        {
            string sourceDirectory = Path.GetDirectoryName(dllPath);
            string[] sourceFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            using (MemoryStream payload = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(payload, encoding))
            {
                writer.Write(encoding.GetBytes("TESTBITS$"));
                writer.Write(Path.GetFileName(dllPath));

                bool settingsIsInSourceDirectory = !string.IsNullOrEmpty(testSettingsPath)
                    && Path.GetFullPath(testSettingsPath).StartsWith(Path.GetFullPath(sourceDirectory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                string testSettingsRelativePath = string.IsNullOrEmpty(testSettingsPath)
                    ? string.Empty
                    : settingsIsInSourceDirectory
                        ? testSettingsPath.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar)
                        : Path.GetFileName(testSettingsPath);
                writer.Write(testSettingsRelativePath);
                writer.Write(sourceFiles.Length + (string.IsNullOrEmpty(testSettingsPath) || settingsIsInSourceDirectory ? 0 : 1));

                foreach (string sourceFile in sourceFiles)
                {
                    WriteFile(writer, sourceFile, sourceFile.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar));
                }

                if (!string.IsNullOrEmpty(testSettingsPath) && !settingsIsInSourceDirectory)
                {
                    WriteFile(writer, testSettingsPath, Path.GetFileName(testSettingsPath));
                }

                foreach (TCPClientInfo client in clients)
                {
                    if (client.ClientName.Equals(machineName))
                    {
                        SendMessageToClient(client.Client, payload.ToArray());
                        LoggerViewModel.Log(string.Format("Sending test bits to [{0}] via TCP...", machineName));
                        break;
                    }
                }
            }
        }

        private static void WriteFile(BinaryWriter writer, string sourcePath, string relativePath)
        {
            byte[] fileBytes = File.ReadAllBytes(sourcePath);
            writer.Write(relativePath);
            writer.Write((long)fileBytes.Length);
            writer.Write(fileBytes);
        }

        public void SendMessageToClientByMachineName(string machineName, string message)
        {
            foreach (TCPClientInfo tc in clients)
            {
                if (tc.ClientName.Equals(machineName))
                {
                    SendMessageToClient(tc.Client, encoding.GetBytes(message));
                    break;
                }
            }
        }

        public void Dispose()
        {
            threadStopper = true;

            if (thread != null && thread.IsAlive)
            {
                thread.Join(1000);
            }

            foreach (TCPClientInfo client in clients.ToArray())
            {
                if (client.Client != null && client.Client.Connected)
                {
                    SendMessageToClient(client.Client, encoding.GetBytes("SERVERDISCONNECT$"));
                }

                if (client.Client != null)
                {
                    client.Client.Close();
                }
            }

            clients.Clear();
            listener.Stop();
        }
    }

    public struct TCPClientInfo
    {
        public TcpClient Client;
        public string ClientName;
        public string ClientOS;
        public string ClientBrowser;
        public string PlayerTag;
        public string ClientIP;
    }
}

