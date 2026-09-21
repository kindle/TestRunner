//-------------------------------------------------------------------------------------------------
// <copyright file="ColorToSolidColorBrushValueConverter.cs" company="Microsoft" author="Bailin Wei">
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
    /// Color to SolidColorBrush value converter
    /// </summary>
    internal class ColorToSolidColorBrushValueConverter : IValueConverter
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
            Color temp = (Color)value;

            if (temp == null)
            {
                return new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            return new SolidColorBrush(temp);
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
