//-------------------------------------------------------------------------------------------------
// <copyright file="TestResultsDetails.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Views
{
    using System.Windows.Controls;

    using TestRunner.Models;

    /// <summary>
    /// Interaction logic for TestResultsDetails.xaml
    /// </summary>
    public partial class TestResultsDetails : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the TestResultsDetails class
        /// </summary>
        /// <param name="t">Test case</param>
        internal TestResultsDetails(TestCase t)
        {
            this.InitializeComponent();

            this.DataContext = t;
        }
    }
}
