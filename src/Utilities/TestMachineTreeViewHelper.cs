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
