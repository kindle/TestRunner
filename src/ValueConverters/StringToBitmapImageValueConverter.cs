//-------------------------------------------------------------------------------------------------
// <copyright file="StringToBitmapImageValueConverter.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    using TestViewer.Models;

    /// <summary>
    /// String to bitmap image value converter
    /// </summary>
    internal class StringToBitmapImageValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Default value</param>
        /// <param name="culture">Culture information</param>
        /// <returns>Output value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string uriString;

            if (parameter == null)
            {
                TestCaseState state = (TestCaseState)value;
                uriString = string.Format("pack://application:,,,/Resources/Case_State_{0}.png", state.ToString());
            }
            else
            {
                string parameterString = (string)parameter;

                if (parameterString == "ClientTreeView")
                {
                    TestMachineState iconName = (TestMachineState)value;
                    uriString = string.Format("pack://application:,,,/Resources/Client{0}.ico", iconName.ToString());
                }
                else
                {
                    uriString = string.Empty;
                }
            }

            return new BitmapImage(new Uri(uriString));
        }

        /// <summary>
        /// Convert back method
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Default value</param>
        /// <param name="culture">Culture information</param>
        /// <returns>Output value</returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
