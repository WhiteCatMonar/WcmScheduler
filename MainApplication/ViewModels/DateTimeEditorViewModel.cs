using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class DateTimeEditorViewModel : INotifyPropertyChanged
    {
        private DateTime? _selectedDate;
        private int _hour;
        private int _minute;

        public DateTimeEditorViewModel(DateTime? initial)
        {
            /* 初期値がなければ今日の0:00 */
            DateTime baseDateTime = initial ?? DateTime.Now;
            SelectedDate = baseDateTime.Date;
            Hour = baseDateTime.Hour;
            Minute = baseDateTime.Minute;

            ConfirmCommand = new RelayCommand(
                () => Result = Composed,    /* 結果を確定する */
                () => SelectedDate.HasValue /* 確定可能かどうか */
            );
            ClearCommand = new RelayCommand(
                () => {
                    SelectedDate = null;
                    Hour = 0;
                    Minute = 0;
                    Result = null;
                }
            );
            IncreaseHourCommand = new RelayCommand(() => Hour = (Hour + 1) % 24);
            DecreaseHourCommand = new RelayCommand(() => Hour = (Hour + 23) % 24);
            IncreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 1) % 60);
            DecreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 59) % 60);
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(Composed));
            }
        }

        public int Hour
        {
            get => _hour;
            set
            {
                _hour = Math.Max(0, Math.Min(23, value));
                OnPropertyChanged(nameof(Hour));
                OnPropertyChanged(nameof(Composed));
            }
        }

        public int Minute
        {
            get => _minute;
            set
            {
                _minute = Math.Max(0, Math.Min(59, value));
                OnPropertyChanged(nameof(Minute));
                OnPropertyChanged(nameof(Composed));
            }
        }

        /* 現在の入力から合成された日時(プレビュー等に使える) */
        public DateTime? Composed => SelectedDate.HasValue
            ? SelectedDate.Value.Date.AddHours(Hour).AddMinutes(Minute)
            : (DateTime?)null;

        public ICommand ConfirmCommand { get; }
        public ICommand IncreaseHourCommand { get; }
        public ICommand DecreaseHourCommand { get; }
        public ICommand IncreaseMinuteCommand { get; }
        public ICommand DecreaseMinuteCommand { get; }
        public ICommand ClearCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /* Window側で拾うための結果 */
        private DateTime? _result;
        public DateTime? Result {
            get => _result;
            private set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }
    }
}
