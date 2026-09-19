//-------------------------------------------------------------------------------------------------
// <copyright file="TrxViewer.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    using TestViewer.Utilities;

    /// <summary>
    /// Interaction logic for TrxViewer.xaml
    /// </summary>
    public partial class TrxViewer : UserControl
    {
        /// <summary>
        /// Register routed event
        /// </summary>
        public static readonly RoutedEvent ViewTestResultsDetailsForTrxViewerEvent = EventManager.RegisterRoutedEvent(
            "ViewTestResultsDetailsForTrxViewer",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TestMonitor));

        /// <summary>
        /// Register routed event
        /// </summary>
        public static readonly RoutedEvent SendNameToFilterEvent = EventManager.RegisterRoutedEvent(
            "SendNameToFilter",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TestMonitor));

        public TrxViewer()
        {
            InitializeComponent();
            // item add or remove change
            this.trxList.ItemContainerGenerator.ItemsChanged += new ItemsChangedEventHandler(ItemContainerGenerator_ItemsChanged);
        }

        /// <summary>
        /// View test results details
        /// </summary>
        public event RoutedEventHandler ViewTestResultsDetailsForTrxViewer
        {
            add { this.AddHandler(ViewTestResultsDetailsForTrxViewerEvent, value); }
            remove { this.RemoveHandler(ViewTestResultsDetailsForTrxViewerEvent, value); }
        }

        public event RoutedEventHandler SendNameToFilter
        {
            add { this.AddHandler(SendNameToFilterEvent, value); }
            remove { this.RemoveHandler(SendNameToFilterEvent, value); }
        }

        /// <summary>
        /// Raise view test results details event
        /// </summary>
        public void RaiseViewTestResultsDetailsForTrxViewerEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ViewTestResultsDetailsForTrxViewerEvent);
            this.RaiseEvent(newEventArgs);
        }

        public void RaiseSendNameToFilterEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SendNameToFilterEvent);
            this.RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// View test results details
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Executed routed event args</param>
        private void ViewTestResultsDetailsForTrxViewerExecute(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.RaiseViewTestResultsDetailsForTrxViewerEvent();
        }

        private void SendNameToFilterExecute(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.RaiseSendNameToFilterEvent();
        }

        private void Sort(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var column = (sender as TextBlock).Text;
            this.TrxViewModel.Sort(column);
        }

        private void AllCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var isAllChecked = (bool)checkBox.IsChecked;
            this.TrxViewModel.ToggleCheckAll(isAllChecked);
            UpdateCheckedCount();
        }

        void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            //LoggerViewModel.Log("ItemContainerGenerator_ItemsChanged.");
            UpdateCheckedCount();
        }

        private void UpdateCheckedCount()
        {
            this.checkCountText.Text = string.Format("Checked item(s): {0}", this.TrxViewModel.GetCheckedItemCount());
        }

        private void SingleCheckBoxClick(object sender, RoutedEventArgs e)
        {
            UpdateCheckedCount();
        }

        //private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //LoggerViewModel.Log("selection changed.");
        //    // tbd not accurate
        //    UpdateCheckedCount();
        //}

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            //tbd 
            UpdateCheckedCount();
            base.OnMouseUp(e);
        }

        private void ToggleFailedItems(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var isAllChecked = (bool)checkBox.IsChecked;
            this.TrxViewModel.ToggleFailedItems(isAllChecked);
            UpdateCheckedCount();
        }

        private void ToggleInconclusiveItems(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var isAllChecked = (bool)checkBox.IsChecked;
            this.TrxViewModel.ToggleInconclusiveItems(isAllChecked);
            UpdateCheckedCount();
        }

        private void SendMail(object sender, RoutedEventArgs e)
        {
            OfficeHelper.SendMail(this.TrxViewModel.TrxModelICollectionView, this.TrxViewModel.TotalDuration.Ticks);
        }
    }
}
