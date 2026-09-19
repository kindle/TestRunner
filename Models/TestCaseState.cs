//-------------------------------------------------------------------------------------------------
// <copyright file="TestCaseState.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    /// <summary>
    /// Test case state
    /// </summary>
    internal enum TestCaseState
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet,

        /// <summary>
        /// Pending test case
        /// </summary>
        Pending,

        /// <summary>
        /// Running test case
        /// </summary>
        Running,

        /// <summary>
        /// Passed test case
        /// </summary>
        Passed,

        /// <summary>
        /// Failed test case
        /// </summary>
        Failed,

        /// <summary>
        /// Inconclusive test case
        /// </summary>
        Inconclusive,

        /// <summary>
        /// Timeout test case
        /// </summary>
        Timeout,

        /// <summary>
        /// NotExecuted test case
        /// </summary>
        NotExecuted,

        /// <summary>
        /// Aborted test case
        /// </summary>
        Aborted,

        /// <summary>
        /// PassedButRunAborted test case
        /// </summary>
        PassedButRunAborted
    }
}
