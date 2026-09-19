//-------------------------------------------------------------------------------------------------
// <copyright file="FileInputBox.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using Microsoft.Win32;

    /// <summary>
    /// 1. Dependency property FileName supports data binding
    /// 2. Routed event FileNameChanged supports triggers
    /// 3. Content property equals FileName property
    /// </summary>
    [ContentProperty("FileName")]
    public partial class FileInputBox : UserControl
    {
        /// <summary>
        /// Register dependency property
        /// </summary>
        public static DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(FileInputBox));

        /// <summary>
        /// Register dependency property
        /// </summary>
        public static RoutedEvent FileNameChangedEvent =
            EventManager.RegisterRoutedEvent("FileNameChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileInputBox));

        /// <summary>
        /// Initializes a new instance of the FileInputBox class
        /// </summary>
        public FileInputBox()
        {
            this.InitializeComponent();
            this.theTextBox.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(this.TextBoxTextChanged), true);
        }

        /// <summary>
        /// Routed event
        /// </summary>
        public event RoutedEventHandler FileNameChanged
        {
            add { this.AddHandler(FileNameChangedEvent, value); }
            remove { this.RemoveHandler(FileNameChangedEvent, value); }
        }
        
        /// <summary>
        /// Gets or sets file name
        /// </summary>
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { this.SetValue(FileNameProperty, value); }
        }

        private string filter;

        /// <summary>
        /// Gets or sets filter
        /// </summary>
        public string Filter
        {
            get { return filter; }
            set { this.filter = value; }
        }

        /// <summary>
        /// Text changed event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void TextBoxTextChanged(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FileNameChangedEvent));
        }

        /// <summary>
        /// Button click event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            //d.Filter = "CodedUI Dll|*.dll" + "|All Files|*.*";
            d.Filter = this.Filter + "|All Files|*.*";

            if (d.ShowDialog() == true)
            {
                this.FileName = d.FileName;
                this.theTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                this.theTextBox.GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
            }
        }
    }
}
