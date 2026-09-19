//-------------------------------------------------------------------------------------------------
// <copyright file="LogsModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    using TestViewer.Utilities;

    /// <summary>
    /// Observable logs data
    /// </summary>
    internal class LogsModel : ObservableCollectionWrapper<Log>
    {
    }
}
