//-------------------------------------------------------------------------------------------------
// <copyright file="TestPassTemplatesViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.ViewModels
{
    using System.Collections.ObjectModel;

    using TestViewer.Models;

    /// <summary>
    /// Test pass template view model
    /// </summary>
    internal class TestPassTemplatesViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TestPassTemplatesViewModel class
        /// </summary>
        public TestPassTemplatesViewModel()
        {
            this.TestPassTemplatesModel = new ObservableCollection<TestPassTemplate>();
            TestPassTemplate tpt = new TestPassTemplate();
            tpt.Type = "Zune";
            tpt.Name = "Zune.net Test Pass";
            tpt.Icon = "zune.png";
            tpt.Location = "l1";
            tpt.Workspace = "ws1";
            tpt.FilterCondition = "fc1";
            this.TestPassTemplatesModel.Add(tpt);

            tpt = new TestPassTemplate();
            tpt.Type = "Xbox";
            tpt.Name = "Xbox.com Test Pass";
            tpt.Icon = "xbox360.png";
            tpt.Location = "l2";
            tpt.Workspace = "ws2";
            tpt.FilterCondition = "fc2";
            this.TestPassTemplatesModel.Add(tpt);
        }

        /// <summary>
        /// Gets or sets test pass templates data
        /// </summary>
        public ObservableCollection<TestPassTemplate> TestPassTemplatesModel { get; set; }
    }
}
