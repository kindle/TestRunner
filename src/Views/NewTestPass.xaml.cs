//-------------------------------------------------------------------------------------------------
// <copyright file="NewTestPass.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Views
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using TestRunner.Utilities;

    /// <summary>
    /// Logic for NewTestPass
    /// </summary>
    public partial class NewTestPass
    {
        /// <summary>
        /// Initializes a new instance of the NewTestPass class
        /// </summary>
        public NewTestPass()
        {
            InitializeComponent();
        }

        /// <summary>
        /// On source initialized
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        /// <summary>
        /// Can exit execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Can execute routed event args</param>
        private void CanExitExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Exit execute
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Execute routed event args</param>
        private void ExitExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// OK button click event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Routed event args</param>
        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            // this.testPassTemplate;

            MessageBox.Show(this.testPassName.Text + this.testPassLocation.FolderName + this.testPassSource.FileName);
        }
    }
}
