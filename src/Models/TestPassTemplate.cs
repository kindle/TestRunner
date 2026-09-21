//-------------------------------------------------------------------------------------------------
// <copyright file="TestPassTemplate.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Models
{
    /// <summary>
    /// Test pass template
    /// </summary>
    internal class TestPassTemplate : TestBase
    {
        /// <summary>
        /// Gets or sets test pass type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets test pass name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets test pass icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets test pass workspace
        /// </summary>
        public string Workspace { get; set; }

        /// <summary>
        /// Gets or sets test pass location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets test pass filter condition
        /// </summary>
        public string FilterCondition { get; set; }
    }
}
