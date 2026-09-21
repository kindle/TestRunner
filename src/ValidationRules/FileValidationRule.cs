//-------------------------------------------------------------------------------------------------
// <copyright file="FileValidationRule.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ValidationRules
{
    using System;
    using System.IO;
    using System.Windows.Controls;

    /// <summary>
    /// File validation rule
    /// </summary>
    public class FileValidationRule : ValidationRule
    {
        /// <summary>
        /// Validation result
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="cultureInfo">Culture infomation</param>
        /// <returns>Output value</returns>
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                throw new ApplicationException("File name is empty.");
            }

            string fileName = value.ToString();

            if (fileName.Trim().Equals(string.Empty))
            {
                return new ValidationResult(true, null);
            }

            if (!File.Exists(fileName))
            {
                return new ValidationResult(false, "The file does not exist!");
            }

            // Since it also applies to *.testsettings file, comment out this block
            /*if (!fileName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ValidationResult(false, "This is not a coded UI file!");
            }
             
            if (fileName.Trim().Equals(string.Empty))
            {
                return new ValidationResult(false, "Please choose a valid file!");
            }*/

            return new ValidationResult(true, null);
        }
    }
}
