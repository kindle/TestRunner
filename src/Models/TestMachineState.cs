//-------------------------------------------------------------------------------------------------
// <copyright file="TestMachineState.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    /// <summary>
    /// Test machine state
    /// </summary>
    public enum TestMachineState
    {
        /// <summary>
        /// Not set(for the root in treeview)
        /// </summary>
        NotSet,

        /// <summary>
        /// Disconnected, yellow pc icon in treeview
        /// </summary>
        Disconnected,

        /// <summary>
        /// Free state, green pc icon in treeview
        /// </summary>
        Free,

        /// <summary>
        /// Busy state, red pc icon in treeview
        /// </summary>
        Busy
    }
}
