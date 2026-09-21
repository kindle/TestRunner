//-------------------------------------------------------------------------------------------------
// <copyright file="Log.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Models
{
    using System.Windows.Media;

    /// <summary>
    /// Log
    /// </summary>
    public class Log : TestBase
    {
        /// <summary>
        /// Log message
        /// </summary>
        private string message;

        /// <summary>
        /// Log message color
        /// </summary>
        private Color messageColor;

        /// <summary>
        /// Gets or sets log message
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value; 
                this.OnPropertyChanged("Message");
            }
        }

        /// <summary>
        /// Gets or sets log message color
        /// </summary>
        public Color MessageColor
        {
            get
            {
                return this.messageColor;
            }

            set
            {
                this.messageColor = value;
                this.OnPropertyChanged("MessageColor");
            }
        }
    }
}
