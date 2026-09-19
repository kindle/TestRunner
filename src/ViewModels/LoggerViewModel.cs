//-------------------------------------------------------------------------------------------------
// <copyright file="LoggerViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.ViewModels
{
    using System.Windows.Media;

    using TestViewer.Models;
    using System.ComponentModel;
    using System.Windows.Data;
    using System;
    using System.Reflection;

    /// <summary>
    /// Logger view model
    /// </summary>
    public class LoggerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the LoggerViewModel class
        /// </summary>
        public LoggerViewModel()
        {
            LogsModel = new LogsModel();
            this.LogsModelICollectionView = CollectionViewSource.GetDefaultView(LogsModel);
        }

        /// <summary>
        /// Gets or sets observable log list
        /// </summary>
        private static LogsModel LogsModel { get; set; }
        
        /// <summary>
        /// Gets or sets the data source for logs
        /// </summary>
        public ICollectionView LogsModelICollectionView { get; set; }

        /// <summary>
        /// Log with black foreground
        /// </summary>
        /// <param name="text"></param>
        public static void Log(string text)
        {
            Log(text, Colors.Black);
        }

        /// <summary>
        /// Log with specific foreground
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Log(string text, Color color)
        {
            try
            {
                LogsModel.Add(new Log
                {
                    Message = DateTime.Now.ToString("hh:mm:ss tt: ") + text.Trim(),
                    MessageColor = color
                });
            }
            // bug of VS
            catch (TargetInvocationException ex)
            {
                Log(string.Format("Throw a targetInvocationException [LoggerViewModel]: {0}.", ex.Message), Colors.Red);
            }
            catch (Exception ex)
            {
                Log(string.Format("Throw an exception [LoggerViewModel]: {0}.", ex.Message), Colors.Red);
            }
        }

        /// <summary>
        /// Remove all logs
        /// </summary>
        public static void Clear()
        {
            LogsModel.Clear();
        }
    }
}
