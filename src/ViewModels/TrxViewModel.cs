//-------------------------------------------------------------------------------------------------
// <copyright file="TrxViewModel.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.ViewModels
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Data;

    using TestRunner.Models;
    using System.Collections.Generic;
    using System.Text;
using System;

    /// <summary>
    /// Trx view model
    /// </summary>
    public class TrxViewModel
    {
        /// <summary>
        /// Initializes a new instance of the TrxViewModel class
        /// </summary>
        public TrxViewModel()
        {
            TrxModel = new TestCasesModel();
            this.TrxModelICollectionView = CollectionViewSource.GetDefaultView(TrxModel);
            sortOrderBy = new SortOrderBy();
        }

        /// <summary>
        /// Gets or sets observable .trx list
        /// </summary>
        private TestCasesModel TrxModel { get; set; }

        /// <summary>
        /// Gets or sets the duration
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets the data source for .trx
        /// </summary>
        public ICollectionView TrxModelICollectionView { get; set; }

        internal void AddTestResult(TestCase tc)
        {
            this.TrxModel.Add(tc);
        }

        /// <summary>
        /// Get test case by name
        /// </summary>
        /// <param name="testCaseName">Test case name</param>
        /// <returns>Test case object</returns>
        internal TestCase GetTestCaseByName(string testCaseName)
        {
            return this.TrxModelICollectionView.Cast<TestCase>().FirstOrDefault(t => t.Name.Equals(testCaseName));
        }

        internal void Sort(string column)
        {
            List<TestCase> sortList = null;
            switch (column)
            { 
                case "Result":
                    sortList = this.sortOrderBy.isResultAscending ? 
                        this.TrxModel.OrderBy(x => x.State).ToList() : this.TrxModel.OrderByDescending(x => x.State).ToList();
                    this.sortOrderBy.isResultAscending = !this.sortOrderBy.isResultAscending;
                    break;
                case "Test Name":
                    sortList = this.sortOrderBy.isTestNameAscending ?
                        this.TrxModel.OrderBy(x => x.Name).ToList() : this.TrxModel.OrderByDescending(x => x.Name).ToList();
                    this.sortOrderBy.isTestNameAscending = !this.sortOrderBy.isTestNameAscending;
                    break;
                case "Owner":
                    sortList = this.sortOrderBy.isDurationAscending ?
                        this.TrxModel.OrderBy(x => x.Owner).ToList() : this.TrxModel.OrderByDescending(x => x.Owner).ToList();
                    this.sortOrderBy.isDurationAscending = !this.sortOrderBy.isDurationAscending;
                    break;
                case "Priority":
                    sortList = this.sortOrderBy.isDurationAscending ?
                        this.TrxModel.OrderBy(x => x.Priority).ToList() : this.TrxModel.OrderByDescending(x => x.Priority).ToList();
                    this.sortOrderBy.isDurationAscending = !this.sortOrderBy.isDurationAscending;
                    break;
                case "Duration":
                    sortList = this.sortOrderBy.isDurationAscending ?
                        this.TrxModel.OrderBy(x => x.Duration).ToList() : this.TrxModel.OrderByDescending(x => x.Duration).ToList();
                    this.sortOrderBy.isDurationAscending = !this.sortOrderBy.isDurationAscending;
                    break;
                case "Start Time":
                    sortList = this.sortOrderBy.isStartTimeAscending ?
                        this.TrxModel.OrderBy(x => x.StartTime).ToList() : this.TrxModel.OrderByDescending(x => x.StartTime).ToList();
                    this.sortOrderBy.isStartTimeAscending = !this.sortOrderBy.isStartTimeAscending;
                    break;
                case "End Time":
                    sortList = this.sortOrderBy.isEndTimeAscending ?
                        this.TrxModel.OrderBy(x => x.EndTime).ToList() : this.TrxModel.OrderByDescending(x => x.EndTime).ToList();
                    this.sortOrderBy.isEndTimeAscending = !this.sortOrderBy.isEndTimeAscending;
                    break;
                case "Error Message":
                    sortList = this.sortOrderBy.isErrorMessageAscending ?
                        this.TrxModel.OrderBy(x => x.BriefErrorMessage).ToList() : this.TrxModel.OrderByDescending(x => x.BriefErrorMessage).ToList();
                    this.sortOrderBy.isErrorMessageAscending = !this.sortOrderBy.isErrorMessageAscending;
                    break;
                default:
                    // error
                    break;
            }

            this.TrxModel.Clear();
            sortList.ForEach(this.TrxModel.Add);
        }

        private SortOrderBy sortOrderBy { get; set; }

        internal class SortOrderBy
        {
            internal bool isResultAscending = false;
            internal bool isTestNameAscending = false;
            internal bool isDurationAscending = false;
            internal bool isStartTimeAscending = false;
            internal bool isEndTimeAscending = false;
            internal bool isErrorMessageAscending = false;
        }

        internal void ToggleCheckAll(bool isCheckAll)
        {
            foreach (var item in this.TrxModel)
            {
                item.IsChecked = isCheckAll;
            }
        }

        internal void ToggleFailedItems(bool isFailedChecked)
        {
            foreach (var item in this.TrxModel)
            {
                if (item.State == TestCaseState.Failed)
                {
                    item.IsChecked = isFailedChecked;
                }
            }
        }

        internal void ToggleInconclusiveItems(bool isFailedChecked)
        {
            foreach (var item in this.TrxModel)
            {
                if (item.State == TestCaseState.Inconclusive)
                {
                    item.IsChecked = isFailedChecked;
                }
            }
        }
        

        internal string GetCheckedItemNames()
        {
            StringBuilder returnString = new StringBuilder();
            foreach (var item in this.TrxModel)
            {
                if (item.IsChecked)
                {
                    returnString.Append(item.Name + "\r\n");
                }
            }
            return returnString.ToString();
        }

        internal int GetCheckedItemCount()
        {
            int checkedCount = 0;
            foreach (var item in this.TrxModel)
            {
                if (item.IsChecked)
                {
                    checkedCount++;
                }
            }
            return checkedCount;
        }
    }
}
