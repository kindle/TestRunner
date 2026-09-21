//-------------------------------------------------------------------------------------------------
// <copyright file="IntToStringValueConverter.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Int to string value converter
    /// </summary>
    internal class IntToStringValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value">Int value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Default value</param>
        /// <param name="culture">Culture information</param>
        /// <returns>String value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)value;
            return string.Format(" Item(s): {0}", intValue);
        }

        /// <summary>
        /// Convert back
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Default value</param>
        /// <param name="culture">Culture information</param>
        /// <returns>Output value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
