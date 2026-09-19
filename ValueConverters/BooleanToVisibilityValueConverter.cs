//-------------------------------------------------------------------------------------------------
// <copyright file="BooleanToVisibilityValueConverter.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Boolean to bitmap image value converter
    /// </summary>
    internal class BooleanToVisibilityValueConverter : IValueConverter
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
            bool temp = (bool)value;

            if (temp == null || temp == false)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
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
