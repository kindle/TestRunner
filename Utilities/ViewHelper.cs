//-------------------------------------------------------------------------------------------------
// <copyright file="ViewHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Utilities
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// View helper
    /// </summary>
    internal class ViewHelper
    {
        /// <summary>
        /// Find visual child by name
        /// </summary>
        /// <typeparam name="T">Type object</typeparam>
        /// <param name="parent">Parent node</param>
        /// <param name="name">Node name</param>
        /// <returns>Specified type object</returns>
        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }

                T result = FindVisualChildByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
