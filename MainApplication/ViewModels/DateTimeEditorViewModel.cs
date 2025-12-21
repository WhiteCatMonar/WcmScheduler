using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class DateTimeEditorViewModel : INotifyPropertyChanged
    {
        private DateTime _baseDateTime; // 初期値(既存のStart/Endを渡す)
        private DateTime? _selectedDate;
        private int _hour;
        private int _minute;

        public DateTimeEditorViewModel(DateTime? initial)
        {
            // 初期値がなければ今日の0:00
            _baseDateTime = initial ?? DateTime.Now;
            SelectedDate = _baseDateTime.Date;
            Hour = _baseDateTime.Hour;
            Minute = _baseDateTime.Minute;

            ConfirmCommand = new RelayCommand(() => Confirm(), () => CanConfirm());
            IncreaseHourCommand = new RelayCommand(() => Hour = (Hour + 1) % 24);
            DecreaseHourCommand = new RelayCommand(() => Hour = (Hour + 23) % 24);
            IncreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 1) % 60);
            DecreaseMinuteCommand = new RelayCommand(() => Minute = (Minute + 59) % 60);
            ClearCommand = new RelayCommand(() => { SelectedDate = null; Hour = 0; Minute = 0; Result = null; });
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(nameof(SelectedDate)); OnPropertyChanged(nameof(Composed)); }
        }

        public int Hour
        {
            get => _hour;
            set { _hour = Math.Max(0, Math.Min(23, value)); OnPropertyChanged(nameof(Hour)); OnPropertyChanged(nameof(Composed)); }
        }

        public int Minute
        {
            get => _minute;
            set { _minute = Math.Max(0, Math.Min(59, value)); OnPropertyChanged(nameof(Minute)); OnPropertyChanged(nameof(Composed)); }
        }

        // 現在の入力から合成された日時(プレビュー等に使える)
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

        private bool CanConfirm() => SelectedDate.HasValue;

        // Window側で拾うための結果
        public DateTime? Result { get; private set; }
        private void Confirm() { Result = Composed; }
    }
}
