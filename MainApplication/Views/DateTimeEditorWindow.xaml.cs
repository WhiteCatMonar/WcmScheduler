using MainApplication.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace MainApplication.Views
{
    /// <summary>
    /// DateTimeEditorWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DateTimeEditorWindow : Window
    {
        public DateTimeEditorWindow(DateTime? initial)
        {
            InitializeComponent();
            DataContext = new DateTimeEditorViewModel(initial);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DateTimeEditorViewModel vm)
            {
                // ConfirmCommandでResultをセット済み
                if (vm.Composed.HasValue)
                {
                    vm.ConfirmCommand.Execute(null);
                    DialogResult = true;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DateTimeEditorViewModel vm)
            {
                vm.ClearCommand.Execute(null);
                DialogResult = true; // 呼び出し元に「OK扱い」で返す
            }
        }

        // 数字のみ許可
        private void DigitsOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
