using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainApplication.Views.Behaviors
{
    public static class ListBoxItemDoubleClickBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand),
            typeof(ListBoxItemDoubleClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

        public static void SetCommand(DependencyObject d, ICommand value) => d.SetValue(CommandProperty, value);
        public static ICommand GetCommand(DependencyObject d) => (ICommand)d.GetValue(CommandProperty);

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object),
            typeof(ListBoxItemDoubleClickBehavior), new PropertyMetadata(null));

        public static void SetCommandParameter(DependencyObject d, object value) => d.SetValue(CommandParameterProperty, value);
        public static object GetCommandParameter(DependencyObject d) => d.GetValue(CommandParameterProperty);

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
