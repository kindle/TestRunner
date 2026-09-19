//-------------------------------------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Views
{
    using System.Windows;
    using TestViewer.Utilities;
    using TestViewer.ViewModels;

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            SettingsHelper.LoadSettings();
            this.testBitsFolder.theTextBox.Text = TestCasesViewModel.TestBitFolder;
            this.testResultOutputFolder.theTextBox.Text = TestCasesViewModel.TestResultOutputFolder;
            this.localIPAddress.Content = SettingsHelper.GetLocalIPAddress().ToString();
            this.serverIPAddress.Text = TestCasesViewModel.ServerIPAddress;
            this.serverPort.Text = TestCasesViewModel.ServerPort;
            this.testSettings.FileName = TestCasesViewModel.SourceTestSettingsUrl;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // path validation first

            // Save settings
            SettingsHelper.SaveSettings(
                this.testBitsFolder.theTextBox.Text,
                this.testResultOutputFolder.theTextBox.Text,
                this.serverIPAddress.Text,
                this.serverPort.Text,
                this.testSettings.FileName
                );

            // Load settings to global vars
            SettingsHelper.LoadSettings();

            this.Close();
        }

        private void ExitExecute(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void SetAsServer(object sender, RoutedEventArgs e)
        {
            this.serverIPAddress.Text = this.localIPAddress.Content.ToString();
        }
    }
}
