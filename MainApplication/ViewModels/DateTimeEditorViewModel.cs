using MainApplication.ViewModels.Core;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// 日付・時刻を編集するための ViewModel。
    /// モーダルダイアログから使用され、編集結果を Result に格納する。
    /// </summary>
    public class DateTimeEditorViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private DateTime? _selectedDate;
        private int _hour;
        private int _minute;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 日時編集ViewModelを生成する。
        /// 初期値がnullの場合は現在日時を基準にする。
        /// </summary>
        public DateTimeEditorViewModel(DateTime? initial)
        {
            /* 初期値がなければ今日の0:00を基準にする */
            DateTime baseDateTime = initial ?? DateTime.Now;
            SelectedDate = baseDateTime.Date;
            Hour = baseDateTime.Hour;
            Minute = baseDateTime.Minute;

            /* OK(確定) */
            ConfirmCommand = new RelayCommand(
                () => Result = Composed,    /* 結果を確定する */
                () => SelectedDate.HasValue /* 確定可能かどうか */
            );

            /* クリア(null に戻す) */
            ClearCommand = new RelayCommand(
                () => {
                    SelectedDate = null;
                    Hour = 0;
                    Minute = 0;
                    Result = null;
                }
            );

            /* 時刻増減 */
            IncreaseHourCommand = new RelayCommand(() => Hour = (Hour + 1) % 24);
            DecreaseHourCommand = new RelayCommand(() => Hour = (Hour + 23) % 24);
            IncreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 1) % 60);
            DecreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 59) % 60);
        }

        /* ---------------------------------------------------------
         * 日付・時刻プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// 選択された日付(null の場合は未選択)
        /// </summary>
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value, [nameof(Composed)]);
        }

        /// <summary>
        /// 時(0〜23)
        /// </summary>
        public int Hour
        {
            get => _hour;
            set => SetProperty(ref _hour, Math.Max(0, Math.Min(23, value)), [nameof(Composed)]);
        }

        /// <summary>
        /// 分(0〜59)
        /// </summary>
        public int Minute
        {
            get => _minute;
            set => SetProperty(ref _minute, Math.Max(0, Math.Min(59, value)), [nameof(Composed)]);
        }

        /* ---------------------------------------------------------
         * 合成日時(プレビュー用)
         * --------------------------------------------------------- */

        /// <summary>
        /// 選択された日付・時刻から合成された日時。
        /// 日付が未選択の場合は null。
        /// </summary>
        public DateTime? Composed => SelectedDate.HasValue
            ? SelectedDate.Value.Date.AddHours(Hour).AddMinutes(Minute)
            : (DateTime?)null;

        /* ---------------------------------------------------------
         * コマンド
         * --------------------------------------------------------- */

        /// <summary>日時を確定するコマンド</summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>時をインクリメント(+1)する</summary>
        public ICommand IncreaseHourCommand { get; }

        /// <summary>時をデクリメント(-1)する</summary>
        public ICommand DecreaseHourCommand { get; }

        /// <summary>分をインクリメント(+1)する</summary>
        public ICommand IncreaseMinuteCommand { get; }

        /// <summary>分をデクリメント(-1)する</summary>
        public ICommand DecreaseMinuteCommand { get; }
        
        /// <summary>入力内容をクリアする</summary>
        public ICommand ClearCommand { get; }

        /* ---------------------------------------------------------
         * 結果値(Window 側で取得)
         * --------------------------------------------------------- */
        private DateTime? _result;

        /// <summary>
        /// ダイアログの結果として返される日時。
        /// ConfirmCommand 実行時に設定される。
        /// </summary>
        public DateTime? Result {
            get => _result;
            private set => SetProperty(ref _result, value);
        }
    }
}

/* --- End of file --- */
