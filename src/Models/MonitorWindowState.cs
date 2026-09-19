//-------------------------------------------------------------------------------------------------
// <copyright file="MonitorWindowState.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    /// <summary>
    /// Monitor window state in working area
    /// </summary>
    public class MonitorWindowState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the start page is shown, default is shown
        /// </summary>
        public bool StartPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test pass monitor is shown, default is hidden
        /// </summary>
        public bool TestPassMonitor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client monitor is shown, default is hidden
        /// </summary>
        public bool ClientMonitor { get; set; }
    }
}
