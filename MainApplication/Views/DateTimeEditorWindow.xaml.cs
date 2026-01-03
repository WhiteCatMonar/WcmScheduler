using MainApplication.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace MainApplication.Views
{
    /// <summary>
    /// 日時入力用のモーダルダイアログ。
    /// DateTimeEditorViewModelをDataContextとし、
    /// 入力完了(Result変更)をトリガーにバリデーションとDialogResultの設定を行う。
    /// </summary>
    public partial class DateTimeEditorWindow : Window
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        /// <summary>
        /// 入力された日時が有効かどうかを判定するためのコールバック。
        /// nullの場合はバリデーションなし。
        /// </summary>
        private readonly Func<DateTime?, bool>? _validate;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 初期値とバリデーション関数を受け取り、ViewModelを初期化する。
        /// </summary>
        public DateTimeEditorWindow(DateTime? initial, Func<DateTime?, bool>? validate = null)
        {
            InitializeComponent();
            DataContext = new DateTimeEditorViewModel(initial);
            _validate = validate;
        }

        /* ---------------------------------------------------------
         * 表示完了時(ContentRendered)
         * --------------------------------------------------------- */

        /// <summary>
        /// ウィンドウ表示後、ViewModelのResultプロパティを監視し、
        /// 値が確定したらバリデーション → DialogResult = trueを行う。
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is DateTimeEditorViewModel vm)
            {
                vm.PropertyChanged += (s, ev) =>
                {
                    if (ev.PropertyName == nameof(DateTimeEditorViewModel.Result))
                    {
                        /* バリデーションが設定されている場合はチェック */
                        if (_validate != null && !_validate(vm.Result))
                        {
                            MessageBox.Show("開始日時と終了日時の関係が不正です");
                            return;
                        }

                        /* OKとしてダイアログを閉じる */
                        DialogResult = true;
                    }
                };
            }
        }

        /* ---------------------------------------------------------
         * キャンセルボタン
         * --------------------------------------------------------- */

        /// <summary>
        /// キャンセル時はResultを変更せず、DialogResult = falseとして閉じる。
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /* ---------------------------------------------------------
         * 入力制限(数字のみ)
         * --------------------------------------------------------- */

        /// <summary>
        /// 時刻入力欄などで、数字以外の入力を拒否する。
        /// </summary>
        private void DigitsOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}

/* --- End of file --- */
