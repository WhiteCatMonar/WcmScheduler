using MainApplication.ViewModels.TeamModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MainApplication.Views
{
    /// <summary>
    /// 特別休日設定ウィンドウ
    /// </summary>
    public partial class SpecialHolidaySettingsWindow : Window
    {
        private bool _isSynchronizing;

        /// <summary>
        /// 特別休日設定ウィンドウを初期化する
        /// </summary>
        public SpecialHolidaySettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// カレンダー読み込み時に特別休日選択状態を同期する
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
        private void HolidayCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            SynchronizeSelectedDates();
        }

        /// <summary>
        /// カレンダー日付クリック時に特別休日を設定または解除する
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
        private void HolidayCalendar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSynchronizing || DataContext is not SpecialHolidaySettingsViewModel viewModel)
            {
                return;
            }

            var button = FindAncestor<CalendarDayButton>(e.OriginalSource as DependencyObject);
            if (button?.DataContext is not DateTime dateTime)
            {
                return;
            }

            var date = DateOnly.FromDateTime(dateTime);
            if (viewModel.IsSpecialHoliday(date))
            {
                viewModel.RemoveHoliday(date);
            }
            else
            {
                viewModel.AddHoliday(date);
            }

            SynchronizeSelectedDates();
            e.Handled = true;
        }

        /// <summary>
        /// 閉じるボタン押下時にウィンドウを閉じる
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// ViewModelの特別休日一覧をカレンダー選択状態へ反映する
        /// </summary>
        private void SynchronizeSelectedDates()
        {
            if (DataContext is not SpecialHolidaySettingsViewModel viewModel)
            {
                return;
            }

            _isSynchronizing = true;
            HolidayCalendar.SelectedDates.Clear();
            foreach (var date in viewModel.SpecialHolidays)
            {
                HolidayCalendar.SelectedDates.Add(date.ToDateTime(TimeOnly.MinValue));
            }

            _isSynchronizing = false;
        }

        /// <summary>
        /// 指定型の親要素を取得する
        /// </summary>
        /// <typeparam name="T">親要素の型。</typeparam>
        /// <param name="source">探索開始要素。</param>
        /// <returns>見つかった親要素。</returns>
        private static T? FindAncestor<T>(DependencyObject? source)
            where T : DependencyObject
        {
            var current = source;
            while (current != null)
            {
                if (current is T target)
                {
                    return target;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}

/* --- End of file --- */
