using System;
using System.Collections.Generic;
using Bannerlord.Commander.UI.Enums;
using Bannerlord.Commander.UI.ViewModels;

namespace Bannerlord.Commander.UI.Services
{
    /// <summary>
    /// Service class for sorting hero lists.
    /// Uses IComparer pattern matching native inventory implementation for in-place sorting.
    /// This allows MBBindingList.Sort() to work efficiently without rebuilding the list.
    /// </summary>
    public static class HeroSorter
    {
        /// <summary>
        /// Gets a comparer for the specified column.
        /// </summary>
        /// <param name="column">The column to sort by</param>
        /// <returns>A comparer configured for the specified column</returns>
        public static HeroComparer GetComparer(HeroSortColumn column)
        {
            return column switch
            {
                HeroSortColumn.Name => new HeroNameComparer(),
                HeroSortColumn.Gender => new HeroGenderComparer(),
                HeroSortColumn.Age => new HeroAgeComparer(),
                HeroSortColumn.Clan => new HeroClanComparer(),
                HeroSortColumn.Kingdom => new HeroKingdomComparer(),
                HeroSortColumn.Culture => new HeroCultureComparer(),
                HeroSortColumn.Type => new HeroTypeComparer(),
                HeroSortColumn.Level => new HeroLevelComparer(),
                _ => new HeroNameComparer()
            };
        }

        /// <summary>
        /// Sorts a list of HeroItemVM based on the specified column and direction.
        /// Used for initial sorting during loading before adding to MBBindingList.
        /// </summary>
        /// <param name="list">The list to sort (modified in place)</param>
        /// <param name="column">The column to sort by</param>
        /// <param name="ascending">True for ascending order, false for descending</param>
        public static void Sort(List<HeroItemVM> list, HeroSortColumn column, bool ascending)
        {
            if (list == null || list.Count == 0)
                return;

            var comparer = GetComparer(column);
            comparer.SetSortMode(ascending);
            list.Sort(comparer);
        }
    }

    /// <summary>
    /// Abstract base class for hero comparers.
    /// Follows the native inventory pattern exactly with SetSortMode for ascending/descending.
    /// Native pattern: y.CompareTo(x) then multiply by (_isAscending ? -1 : 1)
    /// </summary>
    public abstract class HeroComparer : IComparer<HeroItemVM>
    {
        protected bool _isAscending = true;

        /// <summary>
        /// Sets the sort direction.
        /// </summary>
        /// <param name="isAscending">True for ascending, false for descending</param>
        public void SetSortMode(bool isAscending)
        {
            _isAscending = isAscending;
        }

        /// <summary>
        /// Compares two heroes for sorting.
        /// </summary>
        public abstract int Compare(HeroItemVM x, HeroItemVM y);

        /// <summary>
        /// Resolves equality by falling back to name comparison.
        /// Matches native pattern: x.ItemDescription.CompareTo(y.ItemDescription)
        /// </summary>
        protected int ResolveEquality(HeroItemVM x, HeroItemVM y)
        {
            return (x.Name ?? "").CompareTo(y.Name ?? "");
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by name.
    /// Matches native ItemNameComparer pattern exactly.
    /// </summary>
    public class HeroNameComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x), then adjust for direction
            if (_isAscending)
            {
                return (y.Name ?? "").CompareTo(x.Name ?? "") * -1;
            }
            return (y.Name ?? "").CompareTo(x.Name ?? "");
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by gender.
    /// </summary>
    public class HeroGenderComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = (y.Gender ?? "").CompareTo(x.Gender ?? "");
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by age.
    /// </summary>
    public class HeroAgeComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = y.Age.CompareTo(x.Age);
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by clan.
    /// </summary>
    public class HeroClanComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = (y.Clan ?? "").CompareTo(x.Clan ?? "");
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by kingdom.
    /// </summary>
    public class HeroKingdomComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = (y.Kingdom ?? "").CompareTo(x.Kingdom ?? "");
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by culture.
    /// </summary>
    public class HeroCultureComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = (y.Culture ?? "").CompareTo(x.Culture ?? "");
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by type.
    /// </summary>
    public class HeroTypeComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = (y.HeroType ?? "").CompareTo(x.HeroType ?? "");
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }

    /// <summary>
    /// Comparer for sorting heroes by level.
    /// </summary>
    public class HeroLevelComparer : HeroComparer
    {
        public override int Compare(HeroItemVM x, HeroItemVM y)
        {
            // Native pattern: y.CompareTo(x)
            int num = y.Level.CompareTo(x.Level);
            if (num != 0)
            {
                return num * (_isAscending ? -1 : 1);
            }
            return ResolveEquality(x, y);
        }
    }
}
