using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.Helpers
{
    public static class VisualTreeUtils
    {
        /// <summary>
        /// VisualTreeとLogicalTreeの両方を辿って
        /// 指定した型の親要素を探すユーティリティ
        /// </summary>
        public static T FindAncestor<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject current = child;

            while (current != null)
            {
                if (current is T found)
                {
                    return found;
                }

                /* Visual Tree を優先 */
                DependencyObject visualParent = null;
                try
                {
                    visualParent = VisualTreeHelper.GetParent(current);
                }
                catch
                {
                    /* Template 内部などで例外が出ることがあるため安全に無視 */
                }

                if (visualParent != null)
                {
                    current = visualParent;
                    continue;
                }

                /* Visual Tree で見つからなければ Logical Tree を辿る */
                if (current is FrameworkElement fe && fe.Parent != null)
                {
                    current = fe.Parent;
                    continue;
                }

                break;
            }

            return null;
        }

        /// <summary>
        /// VisualTreeとLogicalTreeの両方を辿って
        /// 指定したViewModelを持つ親要素を探す
        /// </summary>
        public static T FindParentViewModel<T>(DependencyObject child) where T : class
        {
            DependencyObject current = child;

            while (current != null)
            {
                if (current is FrameworkElement fe && fe.DataContext is T vm)
                {
                    return vm;
                }

                /* Visual Tree を優先 */
                DependencyObject visualParent = null;
                try
                {
                    visualParent = VisualTreeHelper.GetParent(current);
                }
                catch
                {
                    /* Template 内部などで例外が出ることがあるため安全に無視 */
                }

                if (visualParent != null)
                {
                    current = visualParent;
                    continue;
                }

                /* Visual Tree で見つからなければ Logical Tree を辿る */
                if (current is FrameworkElement fe2 && fe2.Parent != null)
                {
                    current = fe2.Parent;
                    continue;
                }

                break;
            }

            return null;
        }

        /// <summary>
        /// VisualTreeを辿って
        /// 指定したViewModelを持つ子要素を探す
        /// </summary>
        public static IEnumerable<T> FindChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                    yield return t;

                foreach (var descendant in FindChildren<T>(child))
                    yield return descendant;
            }
        }
    }
}
