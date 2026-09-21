//-------------------------------------------------------------------------------------------------
// <copyright file="StartPage.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

using System;
using System.Windows.Media.Imaging;

namespace TestRunner.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using Microsoft.Win32;

    using TestRunner.ViewModels;

    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        /// <summary>
        /// Register routed event for load tests
        /// </summary>
        public static readonly RoutedEvent LoadTestsEvent = EventManager.RegisterRoutedEvent(
            "LoadTests",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StartPage));

        /// <summary>
        /// Register routed event for new project
        /// </summary>
        public static readonly RoutedEvent NewProjectEvent = EventManager.RegisterRoutedEvent(
            "NewProject",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StartPage));

        /// <summary>
        /// Register routed event for open project
        /// </summary>
        public static readonly RoutedEvent OpenProjectEvent = EventManager.RegisterRoutedEvent(
            "OpenProject",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StartPage));

        /// <summary>
        /// Register routed event for create server
        /// </summary>
        public static readonly RoutedEvent CreateServerEvent = EventManager.RegisterRoutedEvent(
            "CreateServer",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StartPage));

        /// <summary>
        /// Register routed event for open .trx file
        /// </summary>
        public static readonly RoutedEvent OpenTrxEvent = EventManager.RegisterRoutedEvent(
            "OpenTrx",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StartPage));

        /// <summary>
        /// Initializes a new instance of the StartPage class
        /// </summary>
        public StartPage()
        {
            this.InitializeComponent();
            var r = new Random();
            this.TopRightImage.Source = new BitmapImage(new Uri(string.Format("/Resources/StartPageTopRight{0}.png", r.Next(0, 4)), UriKind.Relative));
        }

        /// <summary>
        /// Load tests
        /// </summary>
        public event RoutedEventHandler LoadTests
        {
            add { this.AddHandler(LoadTestsEvent, value); }
            remove { this.RemoveHandler(LoadTestsEvent, value); }
        }

        /// <summary>
        /// New project
        /// </summary>
        public event RoutedEventHandler NewProject
        {
            add { this.AddHandler(NewProjectEvent, value); }
            remove { this.RemoveHandler(NewProjectEvent, value); }
        }

        /// <summary>
        /// Open project
        /// </summary>
        public event RoutedEventHandler OpenProject
        {
            add { this.AddHandler(OpenProjectEvent, value); }
            remove { this.RemoveHandler(OpenProjectEvent, value); }
        }

        /// <summary>
        /// Create server
        /// </summary>
        public event RoutedEventHandler CreateServer
        {
            add { this.AddHandler(CreateServerEvent, value); }
            remove { this.RemoveHandler(CreateServerEvent, value); }
        }

        /// <summary>
        /// Open .trx
        /// </summary>
        public event RoutedEventHandler OpenTrx
        {
            add { this.AddHandler(OpenTrxEvent, value); }
            remove { this.RemoveHandler(OpenTrxEvent, value); }
        }

        /// <summary>
        /// Raise load tests event
        /// </summary>
        public void RaiseLoadTestsEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(LoadTestsEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raise new project event
        /// </summary>
        public void RaiseNewProjectEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(NewProjectEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raise open project event
        /// </summary>
        public void RaiseOpenProjectEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OpenProjectEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raise create server event
        /// </summary>
        public void RaiseCreateServerEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CreateServerEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Raise open .trx event
        /// </summary>
        public void RaiseOpenTrxEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OpenTrxEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Load tests
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Request navigate event args</param>
        private void LoadTestsClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "CodedUI Dll|*.dll" + "|All Files|*.*";

            if (d.ShowDialog() == true)
            {
                TestCasesViewModel.SourceTestDllUrl = d.FileName;
                this.RaiseLoadTestsEvent();
            }
        }

        /// <summary>
        /// New project 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Request navigate event args</param>
        private void NewProjectClick(object sender, RoutedEventArgs e)
        {
            this.RaiseNewProjectEvent();
        }

        /// <summary>
        /// Open project 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Request navigate event args</param>
        private void OpenProjectClick(object sender, RoutedEventArgs e)
        {
            this.RaiseOpenProjectEvent();
        }

        /// <summary>
        /// Create server
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Request navigate event args</param>
        private void CreateServerClick(object sender, RoutedEventArgs e)
        {
            this.RaiseCreateServerEvent();
        }

        /// <summary>
        /// Open .trx file 
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Request navigate event args</param>
        private void OpenTrxClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "TRX File|*.trx" + "|All Files|*.*";

            if (d.ShowDialog() == true)
            {
                TestCasesViewModel.SourceTrxFileUrl = d.FileName;
                this.RaiseOpenTrxEvent();
            }
        }
    }
}
