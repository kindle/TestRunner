//-------------------------------------------------------------------------------------------------
// <copyright file="ScheduleSettings.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Views
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using TestRunner.Utilities;
    using TestRunner.ViewModels;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ScheduleSettings.xaml
    /// </summary>
    public partial class ScheduleSettings : Window
    {
        public ScheduleSettings()
        {
            InitializeComponent();

            this.EmailTo.Text = OfficeHelper.GetLocalAccount();
            this.EmailCc.Text = OfficeHelper.GetLocalAccount();
            this.EmailSubject.Text = "[{0}]::TestRunner: Automation - Test Report";
            this.EmailSignature.Text = "Test Team";
        }

        /// <summary>
        /// On source initialized
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            TestCasesViewModel.AutoSendEmail = this.AutoSendEmail.IsChecked;
            TestCasesViewModel.EmailSubject = this.EmailSubject.Text;
            TestCasesViewModel.EmailSignature = this.EmailSignature.Text;

            var selectedValue = (this.AutoRerun.SelectedValue as ComboBoxItem).Content;
            if (selectedValue.ToString() == "No")
            {
                TestCasesViewModel.AutoRerunTimes = 0;
            }
            else
            {
                TestCasesViewModel.AutoRerunTimes = int.Parse(selectedValue.ToString());
            }

            DialogResult = true;
            this.Close();
        }

        private void ExitExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void ResolveAlias(object sender, RoutedEventArgs e)
        {
            this.EmailTo.Text = OfficeHelper.ResolveAlias(this.EmailTo.Text);
            this.EmailCc.Text = OfficeHelper.ResolveAlias(this.EmailCc.Text);
        }
    }
}
