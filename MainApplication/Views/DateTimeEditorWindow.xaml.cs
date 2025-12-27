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
        private readonly Func<DateTime?, bool> _validate;

        public DateTimeEditorWindow(DateTime? initial, Func<DateTime?, bool> validate = null)
        {
            InitializeComponent();
            DataContext = new DateTimeEditorViewModel(initial);
            _validate = validate;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is DateTimeEditorViewModel vm)
            {
                vm.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == nameof(DateTimeEditorViewModel.Result))
                    {
                        if (_validate != null && !_validate(vm.Result))
                        {
                            MessageBox.Show("開始日時と終了日時の関係が不正です");
                            return;
                        }

                        DialogResult = true;
                    }
                };
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            /* Result を変更しない */
            DialogResult = false;
        }

        /* 数字のみ許可 */
        private void DigitsOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
