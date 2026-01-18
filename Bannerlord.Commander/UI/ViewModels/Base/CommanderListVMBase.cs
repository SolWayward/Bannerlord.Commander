using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.Base
{
    /// <summary>
    /// Base class providing keyboard navigation for list-based ViewModels.
    /// All Commander modes with lists should inherit from this.
    /// </summary>
    public abstract class CommanderListVMBase<T> : ViewModel where T : ViewModel
    {
        #region Abstract Methods - Subclasses Must Implement

        /// <summary>
        /// Gets the current list being displayed
        /// </summary>
        protected abstract MBBindingList<T> GetCurrentList();

        /// <summary>
        /// Gets the currently selected item
        /// </summary>
        protected abstract T GetSelectedItem();

        /// <summary>
        /// Selects the specified item
        /// </summary>
        protected abstract void SelectItem(T item);

        /// <summary>
        /// Determines if an item is visible (not filtered)
        /// </summary>
        protected abstract bool IsItemVisible(T item);

        #endregion

        #region Public Navigation Methods

        /// <summary>
        /// Navigate to the next visible item in the list
        /// </summary>
        public virtual void NavigateNext()
        {
            T nextItem = FindNextVisibleItem(forward: true);
            if (nextItem != null)
            {
                SelectItem(nextItem);
            }
        }

        /// <summary>
        /// Navigate to the previous visible item in the list
        /// </summary>
        public virtual void NavigatePrevious()
        {
            T previousItem = FindNextVisibleItem(forward: false);
            if (previousItem != null)
            {
                SelectItem(previousItem);
            }
        }

        #endregion

        #region Protected Helper Methods

        /// <summary>
        /// Finds the next or previous visible item relative to current selection.
        /// Respects filtering and current sort order.
        /// </summary>
        protected virtual T FindNextVisibleItem(bool forward)
        {
            MBBindingList<T> list = GetCurrentList();
            if (list == null || list.Count == 0)
                return null;

            T currentItem = GetSelectedItem();

            // If no item selected, return first/last visible item
            if (currentItem == null)
            {
                return FindFirstVisibleItem(forward);
            }

            // Find current item index
            int currentIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == currentItem)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                return FindFirstVisibleItem(forward);
            }

            // Search for next visible item in the specified direction
            int step = forward ? 1 : -1;
            int index = currentIndex + step;

            while (index >= 0 && index < list.Count)
            {
                T item = list[index];
                if (IsItemVisible(item))
                {
                    return item;
                }
                index += step;
            }

            // No visible item found - stay on current
            return null;
        }

        /// <summary>
        /// Finds the first or last visible item in the list
        /// </summary>
        private T FindFirstVisibleItem(bool forward)
        {
            MBBindingList<T> list = GetCurrentList();
            if (list == null || list.Count == 0)
                return null;

            if (forward)
            {
                // Find first visible from start
                for (int i = 0; i < list.Count; i++)
                {
                    if (IsItemVisible(list[i]))
                        return list[i];
                }
            }
            else
            {
                // Find first visible from end
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (IsItemVisible(list[i]))
                        return list[i];
                }
            }

            return null;
        }

        #endregion
    }
}
