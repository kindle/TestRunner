//-------------------------------------------------------------------------------------------------
// <copyright file="SettingsHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System;

namespace TestViewer.Utilities
{
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    
    using TestViewer.ViewModels;

    /// <summary>
    /// Settings helper
    /// </summary>
    public static class SettingsHelper
    {
        /// <summary>
        /// Isolated file name for settings
        /// </summary>
        private const string IsolatedFileNameForSettings = "FooStudioSettings";

        /// <summary>
        /// Save settings
        /// </summary>
        /// <param name="testBitPath">Test Bit path</param>
        /// <param name="testResultOutputPath">Test result output path</param>
        /// <param name="serverIP">Server IP</param>
        /// <param name="serverPort">Server port</param>
        /// /// <param name="testSettings">TestSettings file path</param>
        public static void SaveSettings(string testBitPath, string testResultOutputPath, string serverIP, string serverPort, string testSettings)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                using (IsolatedStorageFileStream rawStream = isf.CreateFile(IsolatedFileNameForSettings))
                {
                    var writer = new StreamWriter(rawStream);
                    writer.WriteLine(testBitPath);
                    writer.WriteLine(testResultOutputPath);
                    writer.WriteLine(serverIP);
                    writer.WriteLine(serverPort);
                    writer.WriteLine(testSettings);
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Load settings
        /// </summary>
        public static void LoadSettings()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                if (isf.FileExists(IsolatedFileNameForSettings))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(IsolatedFileNameForSettings, FileMode.Open))
                    {
                        var reader = new StreamReader(rawStream);

                        // Test Bits
                        var savedTestBitsFolder = reader.ReadLine();
                        TestCasesViewModel.TestBitFolder = GetDefaultTestBitsFolder(savedTestBitsFolder);
                        Environment.CurrentDirectory = TestCasesViewModel.TestBitFolder;
                        
                        // Test Resuts
                        var savedTestResultOutputFolder = reader.ReadLine();
                        TestCasesViewModel.TestResultOutputFolder = GetDefaultTestResultFolder(savedTestResultOutputFolder);
                        
                        // IP, Port and TestSettings
                        TestCasesViewModel.ServerIPAddress = reader.ReadLine();
                        TestCasesViewModel.ServerPort = reader.ReadLine();
                        TestCasesViewModel.SourceTestSettingsUrl = reader.ReadLine();
                        reader.Close();
                    }
                }
                else
                {
                    // default values
                    SaveSettings(GetDefaultTestBitsFolder(), GetDefaultTestResultFolder(), "", "", "");
                    LoadSettings();
                }
            }
        }

        /// <summary>
        /// Delete settings
        /// </summary>
        public static void DeleteSettings()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                if (isf.FileExists(IsolatedFileNameForSettings))
                {
                    isf.DeleteFile(IsolatedFileNameForSettings);
                }
            }
        }

        public static IPAddress GetLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName()); 
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily.ToString() == "InterNetwork");
        }

        public static string GetDefaultTestResultFolder(string outputFolder = null)
        {
            return GetDefaultFolder(outputFolder, "TestResult");
        }

        public static string GetDefaultTestBitsFolder(string testBitsFolder = null)
        {
            return GetDefaultFolder(testBitsFolder, "TestBit");
        }

        private static string GetDefaultFolder(string folderName, string subFolderName)
        {
            // get rid of A:/ for VMs
            var driveList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady).ToList();
            var folder = string.IsNullOrEmpty(folderName) ? string.Format("{0}BailinStudio\\{1}", driveList[0].Name, subFolderName) : folderName;
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}
