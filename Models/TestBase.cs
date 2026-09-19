//-------------------------------------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Base class of test
    /// </summary>
    public class TestBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// On property changed event
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
