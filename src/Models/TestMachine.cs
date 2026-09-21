//-------------------------------------------------------------------------------------------------
// <copyright file="TestMachine.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Models
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Test machine
    /// </summary>
    public class TestMachine : TestBase
    {
        /// <summary>
        /// Machine name
        /// </summary>
        private string name;

        /// <summary>
        /// Machine tag
        /// </summary>
        private string tag;

        /// <summary>
        /// Test machine state
        /// </summary>
        private TestMachineState state;

        /// <summary>
        /// Is node expanded, default is expanded for the root, only root use this for now.
        /// </summary>
        private bool isNodeExpanded;

        /// <summary>
        /// Is node checked, default is not checked, only checked clients can run tests
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// Is test bits updated, set it True, default is False
        /// </summary>
        public bool IsTestBitsUpdated;

        /// <summary>
        /// Gets or sets machine name
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value; 
                this.OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets machine tag
        /// [Root, Machine] tbd: change to enum later
        /// </summary>
        public string Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                this.tag = value;
                this.OnPropertyChanged("Tag");
            }
        }

        /// <summary>
        /// Gets or sets test machine state
        /// </summary>
        public TestMachineState State
        {
            get
            {
                return this.state;
            }

            set
            {
                this.state = value; 
                this.OnPropertyChanged("State");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node is expanded
        /// </summary>
        public bool IsNodeExpanded
        {
            get
            {
                return this.isNodeExpanded;
            }

            set
            {
                this.isNodeExpanded = value; 
                this.OnPropertyChanged("IsNodeExpanded");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node is checked
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }

            set
            {
                this.isChecked = value;
                this.OnPropertyChanged("IsChecked");
            }
        }

        /// <summary>
        /// Gets or sets clients collection
        /// </summary>
        public ObservableCollection<TestMachine> Children { get; set; }
    }
}
