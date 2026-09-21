//-------------------------------------------------------------------------------------------------
// <copyright file="TestWindowsViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ViewModels
{
    using TestRunner.Models;

    /// <summary>
    /// Test window view model
    /// </summary>
    public static class TestWindowsViewModel
    {
        /// <summary>
        /// Initializes static members of the <see cref="TestWindowsViewModel"/> class
        /// </summary>
        static TestWindowsViewModel()
        {
            MonitorWindowState = new MonitorWindowState();
        }

        /// <summary>
        /// Gets or sets the monitor window state
        /// </summary>
        public static MonitorWindowState MonitorWindowState { get; set; }
    }
}
