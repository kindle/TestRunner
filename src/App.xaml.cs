//-------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer
{
    using System;
    using System.Threading;
    using System.Windows;

    using TestViewer.Views;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Mutex name 
        /// </summary>
        private const string MutexName = "Microsoft.Bailin.TPMS";

        /// <summary>
        /// Main function
        /// </summary>
        [STAThread]
        public static void Main()
        {
            bool isNew;

            using (new Mutex(false, MutexName, out isNew))
            {
                if (isNew)
                {
                    var app = new App();
                    var mainWindow = new MainWindow();
                    //var mainWindow = new ScheduleSettings();

                    try
                    {
                        app.Run(mainWindow);
                    }
                    catch (Exception ex)
                    {
                        // write a log file here. tbd
                        //throw new Exception(ex.ToString());
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }  
    }
}
