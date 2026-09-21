//-------------------------------------------------------------------------------------------------
// <copyright file="BooleanToColorValueConverter.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Boolean to color value converter
    /// </summary>
    internal class BooleanToColorValueConverter : IValueConverter
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
                return new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            return new SolidColorBrush(Color.FromRgb(169, 169, 169));
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
