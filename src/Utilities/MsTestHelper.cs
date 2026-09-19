//-------------------------------------------------------------------------------------------------
// <copyright file="MsTestHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System.Linq;

namespace TestViewer.Utilities
{
    using System.IO;

    /// <summary>
    /// MsTest helper
    /// </summary>
    internal class MsTestHelper
    {
        /// <summary>
        /// Specify the bat in order to run mstest.exe in cmd
        /// </summary>
        /// <returns></returns>
        public static string GetCommandPromptPath()
        {
            string[] predefineFilePaths =
                {
                    //@"C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC\vcvarsall.bat",
                    //@"C:\Program Files\Microsoft Visual Studio 11.0\VC\vcvarsall.bat",
                    @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat",
                    @"C:\Program Files\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
                };

            foreach (var possibleFile in predefineFilePaths)
            {
                if (File.Exists(possibleFile))
                {
                    return possibleFile;
                }
            }

            throw new DirectoryNotFoundException("vcvarsall.bat was not found, can't run MSTest.exe");
        }

        /// <summary>
        /// Specify the mstest.exe file path
        /// </summary>
        /// <returns></returns>
        public static string GetMsTestPath()
        {
            var driveList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady).ToList();

            string[] predefineFilePaths =
                {
                    @"{0}Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\mstest.exe",
                    @"{0}Program Files\Microsoft Visual Studio 11.0\Common7\IDE\mstest.exe"
                    //@"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\mstest.exe",
                    //@"C:\Program Files\Microsoft Visual Studio 10.0\Common7\IDE\mstest.exe"
                };

            foreach (var drive in driveList)
            {
                foreach (var possibleFile in predefineFilePaths)
                {
                    if (File.Exists(string.Format(possibleFile, drive.Name)))
                    {
                        return string.Format(possibleFile, drive.Name);
                    }
                }
            }

            throw new DirectoryNotFoundException("MSTest.exe was not found");
        }
    }
}
