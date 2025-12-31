using System;
using System.Collections.Generic;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.ViewModels;

namespace Bannerlord.Commander.UI.Services
{
    /// <summary>
    /// Service class for sorting hero lists.
    /// Encapsulates all sorting logic to keep ViewModels clean.
    /// </summary>
    public static class HeroSorter
    {
        /// <summary>
        /// Sorts a list of HeroItemVM based on the specified column and direction
        /// </summary>
        /// <param name="list">The list to sort (modified in place)</param>
        /// <param name="column">The column to sort by</param>
        /// <param name="ascending">True for ascending order, false for descending</param>
        public static void Sort(List<HeroItemVM> list, HeroSortColumn column, bool ascending)
        {
            if (list == null || list.Count == 0)
                return;

            Comparison<HeroItemVM> comparison = GetComparison(column, ascending);
            list.Sort(comparison);
        }

        /// <summary>
        /// Gets the comparison delegate for the specified column and direction
        /// </summary>
        private static Comparison<HeroItemVM> GetComparison(HeroSortColumn column, bool ascending)
        {
            return column switch
            {
                HeroSortColumn.Name => CreateStringComparison(h => h.Name, ascending),
                HeroSortColumn.Gender => CreateStringComparison(h => h.Gender, ascending),
                HeroSortColumn.Age => CreateIntComparison(h => h.Age, ascending),
                HeroSortColumn.Clan => CreateStringComparison(h => h.Clan, ascending),
                HeroSortColumn.Kingdom => CreateStringComparison(h => h.Kingdom, ascending),
                HeroSortColumn.Culture => CreateStringComparison(h => h.Culture, ascending),
                HeroSortColumn.Type => CreateStringComparison(h => h.HeroType, ascending),
                HeroSortColumn.Level => CreateIntComparison(h => h.Level, ascending),
                _ => CreateStringComparison(h => h.Name, ascending)
            };
        }

        /// <summary>
        /// Creates a string comparison delegate
        /// </summary>
        private static Comparison<HeroItemVM> CreateStringComparison(Func<HeroItemVM, string> selector, bool ascending)
        {
            return (a, b) =>
            {
                int result = string.Compare(selector(a), selector(b), StringComparison.Ordinal);
                return ascending ? result : -result;
            };
        }

        /// <summary>
        /// Creates an integer comparison delegate
        /// </summary>
        private static Comparison<HeroItemVM> CreateIntComparison(Func<HeroItemVM, int> selector, bool ascending)
        {
            return (a, b) =>
            {
                int result = selector(a).CompareTo(selector(b));
                return ascending ? result : -result;
            };
        }
    }
}
