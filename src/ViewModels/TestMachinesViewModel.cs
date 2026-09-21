//-------------------------------------------------------------------------------------------------
// <copyright file="TestMachinesViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ViewModels
{
    using TestRunner.Models;
    using TestRunner.Utilities;

    /// <summary>
    /// Test machines view model
    /// </summary>
    public class TestMachinesViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TestMachinesViewModel class
        /// </summary>
        public TestMachinesViewModel()
        {
            ClientsModel = new ObservableCollectionWrapper<TestMachine>();
            foreach (var testMachine in TestMachineTreeViewHelper.LoadSettings())
            {
                ClientsModel.Add(new TestMachine() { Name = testMachine, Tag = "Machine", State = TestMachineState.Disconnected });
            }

            this.TestMachinesModel = new ObservableCollectionWrapper<TestMachine>
            {
                new TestMachine 
                { 
                    Name = "Client Machines", 
                    Tag = "Root",
                    State = TestMachineState.NotSet,
                    IsNodeExpanded = true,
                    Children = ClientsModel
                }
            };
        }


        /// <summary>
        /// Gets or sets observable machine list
        /// </summary>
        public ObservableCollectionWrapper<TestMachine> TestMachinesModel { get; set; }

        /// <summary>
        /// Gets or sets the data source
        /// </summary>
        public static ObservableCollectionWrapper<TestMachine> ClientsModel { get; set; }
    }
}
