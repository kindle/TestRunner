//-------------------------------------------------------------------------------------------------
// <copyright file="FuzzyQueryHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Utilities
{
    using System.Collections.Generic;

    using TestViewer.Models;

    /// <summary>
    /// Fuzzy query helper
    /// </summary>
    public static class FuzzyQueryHelper
    {
        /// <summary>
        /// Fuzzy query
        /// </summary>
        /// <param name="t">Test case</param>
        /// <param name="filter">Filter string</param>
        /// <param name="isInconclusive">Exclude filter string</param>
        /// <returns>True as show</returns>
        internal static bool FuzzyQuery(TestCase t, string filterText, bool isInconclusive)
        {
            string testCaseInfoString = string.Join(" ", t.ID, t.Description, t.Owner, t.Custom1, t.Custom2, t.Custom3, t.Ignore ? "[Ignore]" : "");

            filterText = filterText.Replace("\r\n", ",").Replace(" ", "").Replace(";", ",");

            if (filterText.Contains(","))
            {
                List<string> testCaseList = new List<string>(filterText.ToUpper().Split(','));

                // check if filter text is all /r/n or spaces, there should be no text filtering
                bool isInValidFilterText = true;
                foreach (string str in testCaseList)
                {
                    if (str.Trim() != string.Empty)
                    {
                        isInValidFilterText = false;
                    }
                }

                if (isInValidFilterText)
                {
                    return true;
                }

                // if filter text is all /r/n or spaces
                if (isInconclusive)
                {
                    return !testCaseList.Contains(t.Name.ToUpper());
                }

                return testCaseList.Contains(t.Name.ToUpper());
            }

            if (filterText.Contains("&"))
            {
                List<string> testCaseList = new List<string>(filterText.ToUpper().Split('&'));

                if (isInconclusive)
                {
                    bool isInconclusiveReturnValue = true;
                    foreach (var str in testCaseList)
                    {
                        if (testCaseInfoString.ToUpper().Contains(str))
                        {
                            isInconclusiveReturnValue = false;
                            break;
                        }
                    }
                    return isInconclusiveReturnValue;
                }

                bool returnValue = true;
                foreach (var str in testCaseList)
                {
                    if (!testCaseInfoString.ToUpper().Contains(str))
                    {
                        returnValue = false;
                        break;
                    }
                }

                return returnValue;
            }

            if (filterText.Contains("|"))
            {
                List<string> testCaseList = new List<string>(filterText.ToUpper().Split('|'));

                //if (isInconclusive)
                //{
                //    this is meaningless
                //}

                bool returnValue = false;
                foreach (var str in testCaseList)
                {
                    if (testCaseInfoString.ToUpper().Contains(str))
                    {
                        returnValue = true;
                        break;
                    }
                }

                return returnValue;
            }

            // no , no & no |
            if (isInconclusive)
            {
                return !testCaseInfoString.ToUpper().Contains(filterText.ToUpper());
            }

            return testCaseInfoString.ToUpper().Contains(filterText.ToUpper());
        }
    }
}
