//-------------------------------------------------------------------------------------------------
// <copyright file="TestMonitor.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    using TestViewer.Utilities;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for TestMonitor.xaml
    /// </summary>
    public partial class TestMonitor : UserControl
    {
        /// <summary>
        /// Register routed event
        /// </summary>
        public static readonly RoutedEvent ViewTestResultsDetailsForTestMonitorEvent = EventManager.RegisterRoutedEvent(
            "ViewTestResultsDetailsForTestMonitor",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TestMonitor));

        /// <summary>
        /// Initializes a new instance of the TestMonitor class
        /// </summary>
        public TestMonitor()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// View test results details
        /// </summary>
        public event RoutedEventHandler ViewTestResultsDetailsForTestMonitor
        {
            add { this.AddHandler(ViewTestResultsDetailsForTestMonitorEvent, value); }
            remove { this.RemoveHandler(ViewTestResultsDetailsForTestMonitorEvent, value); }
        }

        /// <summary>
        /// Raise view test results details event
        /// </summary>
        public void RaiseViewTestResultsDetailsForTestMonitorEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ViewTestResultsDetailsForTestMonitorEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// View test results details
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Executed routed event args</param>
        private void ViewTestResultsDetailsForTestMonitorExecute(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.RaiseViewTestResultsDetailsForTestMonitorEvent();
        }

        private void ResumeClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Resume");
        }

        private void PauseClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pause");
        }

        private void StopClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Stop");
        }

        private void SendMail(object sender, RoutedEventArgs e)
        {
            OfficeHelper.SendMail(testPassList.ItemsSource as ICollectionView, -1);
        }

        private void Sort(object sender, MouseButtonEventArgs e)
        {
            var column = (sender as TextBlock).Text;
            //this.testCasesViewModel.Sort(column);
            //this.TestCasesModelScheduledICollectionView
        }
    }
}
