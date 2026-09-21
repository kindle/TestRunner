//-------------------------------------------------------------------------------------------------
// <copyright file="LoggerViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ViewModels
{
    using System.IO;
    using System.Text;
    using System.Windows.Media;

    using TestRunner.Models;
    using System.ComponentModel;
    using System.Windows.Data;
    using System;
    using System.Reflection;

    /// <summary>
    /// Logger view model
    /// </summary>
    public class LoggerViewModel
    {
        private static readonly object LogFileLock = new object();
        private static string logFile;

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
                DateTime timestamp = DateTime.Now;
                string message = timestamp.ToString("hh:mm:ss tt: ") + text.Trim();

                LogsModel.Add(new Log
                {
                    Message = message,
                    MessageColor = color
                });

                WriteToLogFile(timestamp, message);
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
        /// Starts a new log file for a test run
        /// </summary>
        public static void BeginTestRun()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            lock (LogFileLock)
            {
                Directory.CreateDirectory(logDirectory);
                logFile = Path.Combine(logDirectory, "TestRunner_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".log");
                File.WriteAllText(logFile, string.Empty, Encoding.UTF8);
            }
        }

        private static void WriteToLogFile(DateTime timestamp, string message)
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                lock (LogFileLock)
                {
                    Directory.CreateDirectory(logDirectory);
                    if (logFile == null)
                    {
                        logFile = Path.Combine(logDirectory, "TestRunner_" + timestamp.ToString("yyyyMMdd_HHmm") + ".log");
                    }

                    File.AppendAllText(logFile, message + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to write TestRunner log file: " + ex.Message);
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
