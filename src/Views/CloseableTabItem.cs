//-------------------------------------------------------------------------------------------------
// <copyright file="CloseableTabItem.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Closeable tab item
    /// </summary>
    public class CloseableTabItem : TabItem
    {
        /// <summary>
        /// Register routed event
        /// </summary>
        public static readonly RoutedEvent CloseTabEvent =
            EventManager.RegisterRoutedEvent(
            "CloseTab",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CloseableTabItem));

        /// <summary>
        /// Initializes static members of the CloseableTabItem class
        /// </summary>
        static CloseableTabItem()
        {
            // This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            // This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CloseableTabItem),
                new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
        }

        /// <summary>
        /// Initializes a new instance of the CloseableTabItem class
        /// </summary>
        public CloseableTabItem()
        {
            this.Background = new SolidColorBrush(Color.FromRgb(43, 60, 89));
            this.Foreground = new SolidColorBrush(Color.FromRgb(139, 137, 137));
        }

        /// <summary>
        /// Routed event handler
        /// </summary>
        public event RoutedEventHandler CloseTab
        {
            add { this.AddHandler(CloseTabEvent, value); }
            remove { this.RemoveHandler(CloseTabEvent, value); }
        }

        /// <summary>
        /// On apply template event
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button closeButton = GetTemplateChild("PART_Close") as Button;

            if (closeButton != null)
            {
                closeButton.Click += this.CloseButtonClick;
            }
        }

        /// <summary>
        /// Onclick event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }
    }
}
