using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainApplication.Views.Behaviors
{
    /// <summary>
    /// ListBoxItemのダブルクリックをICommandにバインドするための添付ビヘイビア。
    /// 
    /// ListBoxItemは通常Clickイベントしか持たないため、
    /// MVVMパターンでダブルクリック操作を扱うために使用する。
    /// </summary>
    public static class ListBoxItemDoubleClickBehavior
    {
        /* ---------------------------------------------------------
         * Command添付プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// ダブルクリック時に実行されるICommandを設定する添付プロパティ。
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(ListBoxItemDoubleClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static void SetCommand(DependencyObject d, ICommand value) => d.SetValue(CommandProperty, value);

        public static ICommand GetCommand(DependencyObject d) => (ICommand)d.GetValue(CommandProperty);

        /* ---------------------------------------------------------
         * CommandParameter添付プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// ICommandに渡すパラメータを指定する添付プロパティ。
        /// 未設定の場合はListBoxItemのDataContextが使用される。
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(ListBoxItemDoubleClickBehavior),
                new PropertyMetadata(null));

        public static void SetCommandParameter(DependencyObject d, object value) => d.SetValue(CommandParameterProperty, value);

        public static object GetCommandParameter(DependencyObject d) => d.GetValue(CommandParameterProperty);

        /* ---------------------------------------------------------
         * CommandProperty変更時の処理
         * --------------------------------------------------------- */

        /// <summary>
        /// Commandが設定されたらダブルクリックイベントを購読し、
        /// Commandが解除されたら購読を外す。
        /// </summary>
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBoxItem item)
            {
                if (e.NewValue != null)
                {
                    item.MouseDoubleClick += OnMouseDoubleClick;
                }
                else
                {
                    item.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        /* ---------------------------------------------------------
         * ダブルクリック時の処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ダブルクリックされたらICommandを実行する。
        /// CommandParameterが設定されていればそれを使用し、
        /// 未設定ならListBoxItemのDataContextをパラメータとして渡す。
        /// </summary>
        private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                var cmd = GetCommand(item);
                var param = GetCommandParameter(item) ?? item.DataContext;
                if (cmd?.CanExecute(param) == true)
                {
                    cmd.Execute(param);
                }
            }
        }
    }
}

/* --- End of file --- */
