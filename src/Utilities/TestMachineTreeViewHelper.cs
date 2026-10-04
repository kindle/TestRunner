//-------------------------------------------------------------------------------------------------
// <copyright file="TestMachineTreeViewHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Utilities
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using TestRunner.ViewModels;
    
    /// <summary>
    /// Test Machine Tree View helper
    /// </summary>
    public static class TestMachineTreeViewHelper
    {
        /// <summary>
        /// Isolated file name for test machine list
        /// </summary>
        private const string IsolatedFileNameForTestMachineTreeView = "TestMachineTreeView";

        /// <summary>
        /// Isolated file name for machine display names, one "name\tdisplayName" per line
        /// </summary>
        private const string IsolatedFileNameForDisplayNames = "TestMachineDisplayNames";

        /// <summary>
        /// Load display names keyed by machine name
        /// </summary>
        public static Dictionary<string, string> LoadDisplayNames()
        {
            var result = new Dictionary<string, string>();

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                if (isf.FileExists(IsolatedFileNameForDisplayNames))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(IsolatedFileNameForDisplayNames, FileMode.Open))
                    using (var reader = new StreamReader(rawStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            int separator = line.IndexOf('\t');
                            if (separator > 0)
                            {
                                result[line.Substring(0, separator)] = line.Substring(separator + 1);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Save display name for a machine
        /// </summary>
        /// <param name="machineName">Machine name</param>
        /// <param name="displayName">Display name, empty to clear</param>
        public static void SaveDisplayName(string machineName, string displayName)
        {
            var displayNames = LoadDisplayNames();
            displayName = (displayName ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (displayName.Length == 0)
            {
                displayNames.Remove(machineName);
            }
            else
            {
                displayNames[machineName] = displayName;
            }

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                using (IsolatedStorageFileStream rawStream = isf.CreateFile(IsolatedFileNameForDisplayNames))
                using (var writer = new StreamWriter(rawStream))
                {
                    foreach (var pair in displayNames)
                    {
                        writer.WriteLine(pair.Key + "\t" + pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Save 
        /// </summary>
        public static void SaveSettings()
        {
            List<string> machineNameList = LoadSettings();

            foreach (var machine in TestMachinesViewModel.ClientsModel)
            {
                if(!machineNameList.Contains(machine.Name))
                {
                    machineNameList.Add(machine.Name);
                }
            }

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                using (IsolatedStorageFileStream rawStream = isf.CreateFile(IsolatedFileNameForTestMachineTreeView))
                {
                    var writer = new StreamWriter(rawStream);
                    foreach (var machineName in machineNameList)
                    {
                        writer.WriteLine(machineName);
                    }
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Load 
        /// </summary>
        public static List<string> LoadSettings()
        {
            List<string> temp = new List<string>();

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                if (isf.FileExists(IsolatedFileNameForTestMachineTreeView))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(IsolatedFileNameForTestMachineTreeView, FileMode.Open))
                    {
                        var reader = new StreamReader(rawStream);
                        string machineName;
                        while((machineName = reader.ReadLine()) != null)
                        {
                            temp.Add(machineName);
                        }
                        reader.Close();
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// Remove a machine and its display name from local storage
        /// </summary>
        /// <param name="machineName">Machine name</param>
        public static void RemoveMachine(string machineName)
        {
            List<string> machineNameList = LoadSettings();
            machineNameList.Remove(machineName);

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                using (IsolatedStorageFileStream rawStream = isf.CreateFile(IsolatedFileNameForTestMachineTreeView))
                using (var writer = new StreamWriter(rawStream))
                {
                    foreach (var name in machineNameList)
                    {
                        writer.WriteLine(name);
                    }
                }
            }

            SaveDisplayName(machineName, string.Empty);
        }

        /// <summary>
        /// Delete 
        /// </summary>
        public static void DeleteSettings()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
            {
                if (isf.FileExists(IsolatedFileNameForTestMachineTreeView))
                {
                    isf.DeleteFile(IsolatedFileNameForTestMachineTreeView);
                }
            }
        }
    }
}
