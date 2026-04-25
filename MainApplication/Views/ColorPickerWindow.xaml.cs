using MainApplication.ViewModels;
using System.Windows;

namespace MainApplication.Views
{
    /// <summary>
    /// ARGB形式の色を編集するためのモーダルダイアログ。
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        private readonly Func<string, bool>? _validate;

        /// <summary>
        /// 初期値とバリデーション関数を受け取り、ViewModelを初期化する。
        /// </summary>
        /// <param name="initial">初期色文字列。</param>
        /// <param name="validate">入力検証用デリゲート。</param>
        public ColorPickerWindow(string initial, Func<string, bool>? validate = null)
        {
            InitializeComponent();
            DataContext = new ColorPickerViewModel(initial);
            _validate = validate;
        }

        /// <summary>
        /// 表示後にViewModelの確定結果を監視する。
        /// </summary>
        /// <param name="e">イベント引数。</param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is ColorPickerViewModel vm)
            {
                vm.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName != nameof(ColorPickerViewModel.Result) || vm.Result is null)
                    {
                        return;
                    }

                    if (_validate != null && !_validate(vm.Result))
                    {
                        MessageBox.Show("色の形式が不正です");
                        return;
                    }

                    DialogResult = true;
                };
            }
        }

        /// <summary>
        /// キャンセル時は結果を変更せずに閉じる。
        /// </summary>
        /// <param name="sender">イベント送信元。</param>
        /// <param name="e">イベント引数。</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// 数字以外の入力を拒否する。
        /// </summary>
        /// <param name="sender">イベント送信元。</param>
        /// <param name="e">テキスト入力イベント引数。</param>
        private void DigitsOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}

/* --- End of file --- */
