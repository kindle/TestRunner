//-------------------------------------------------------------------------------------------------
// <copyright file="TestCasesModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Observable test cases data
    /// </summary>
    internal class TestCasesModel : ObservableCollection<TestCase>
    {
    }
}
