//-------------------------------------------------------------------------------------------------
// <copyright file="TestCase.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    using System;

    /// <summary>
    /// Test case class
    /// </summary>
    internal class TestCase : TestBase
    {
        /// <summary>
        /// A value indicating whether the test case is selected
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// A value indicating the state of the test case
        /// </summary>
        private TestCaseState state;

        /// <summary>
        /// A value indicating the client machine name that runs the test case
        /// </summary>
        private string clientMachineName { get; set; }

        /// <summary>
        /// A value indicating the client machine info that runs the test case
        /// </summary>
        private string clientMachineInfo { get; set; }

        /// <summary>
        /// A value indicating the start time of the test case
        /// </summary>
        private DateTime? startTime;

        /// <summary>
        /// A value indicating the end time of the test case
        /// </summary>
        private DateTime? endTime;

        /// <summary>
        /// A value indicating the duration of the test case
        /// </summary>
        private DateTime? duration;

        /// <summary>
        /// A value indicating the rerun times of the test case
        /// </summary>
        private int rerunTimes;

        /// <summary>
        /// A value indicating the error message of the test case
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// A value indicating the brief error message of the test case
        /// </summary>
        private string briefErrorMessage;

        /// <summary>
        /// Gets or sets a value indicating whether the test case is selected
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
        /// Gets or sets the state of the test case
        /// </summary>
        public TestCaseState State
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
        /// Gets or sets the client machine name that runs the test case
        /// </summary>
        public string ClientMachineName
        {
            get
            {
                return this.clientMachineName;
            }

            set
            {
                this.clientMachineName = value;
                this.OnPropertyChanged("ClientMachineName");
            }
        }

        /// <summary>
        /// Gets or sets the client machine info that runs the test case
        /// </summary>
        public string ClientMachineInfo
        {
            get
            {
                return this.clientMachineInfo;
            }

            set
            {
                this.clientMachineInfo = value;
                this.OnPropertyChanged("ClientMachineInfo");
            }
        }

        /// <summary>
        /// Gets or sets the test case name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the test case priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the custom test property1
        /// </summary>
        public string Custom1 { get; set; }

        /// <summary>
        /// Gets or sets the custom test property2
        /// </summary>
        public string Custom2 { get; set; }

        /// <summary>
        /// Gets or sets the custom test property3
        /// </summary>
        public string Custom3 { get; set; }

        /// <summary>
        /// Gets or sets the owner
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ID used to identify unique test case
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test case is ignored 
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rerun is completed
        /// </summary>
        public bool RerunCompleted { get; set; }

        /// <summary>
        /// Gets or sets the start time when running the case
        /// </summary>
        public DateTime? StartTime
        {
            get
            {
                return this.startTime;
            }

            set
            {
                this.startTime = value;
                this.OnPropertyChanged("StartTime");
            }
        }

        /// <summary>
        /// Gets or sets the end time when running the case
        /// </summary>
        public DateTime? EndTime
        {
            get
            {
                return this.endTime;
            }

            set
            {
                this.endTime = value;
                this.OnPropertyChanged("EndTime");
            }
        }

        /// <summary>
        /// Gets or sets the executed duration of the test
        /// </summary>
        public DateTime? Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                this.duration = value;
                this.OnPropertyChanged("Duration");
            }
        }

        /// <summary>
        /// Gets or sets the rerun times of the test
        /// </summary>
        public int RerunTimes
        {
            get
            {
                return this.rerunTimes;
            }

            set
            {
                this.rerunTimes = value;
                this.OnPropertyChanged("RerunTimes");
            }
        }

        /// <summary>
        /// Gets or sets the ResultFile
        /// </summary>
        public string ResultFile { get; set; }

        /// <summary>
        /// Gets or sets the brief error message
        /// </summary>
        public string BriefErrorMessage
        {
            get
            {
                return this.briefErrorMessage;
            }

            set
            {
                this.briefErrorMessage = value;
                this.OnPropertyChanged("BriefErrorMessage");
            }
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }

            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged("ErrorMessage");
            }
        }

        /// <summary>
        /// Gets or sets the Standard output
        /// </summary>
        public string StdOut { get; set; }

        /// <summary>
        /// Gets or sets the stack trace output
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the result files output
        /// </summary>
        public string ResultFiles { get; set; }
    }
}
