//-------------------------------------------------------------------------------------------------
// <copyright file="OldWindow.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Views
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class OldWindow : IWin32Window
    {
        IntPtr _handle;
        public OldWindow(IntPtr handle)
        {
            _handle = handle;
        }

        #region IWin32Window Members
        IntPtr IWin32Window.Handle
        {
            get { return _handle; }
        }
        #endregion
    } 
}
