using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// 特別休日設定ウィンドウのViewModel
    /// </summary>
    public class SpecialHolidaySettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// 特別休日設定ViewModelを生成する
        /// </summary>
        /// <param name="specialHolidays">特別休日一覧。</param>
        public SpecialHolidaySettingsViewModel(ObservableCollection<DateOnly> specialHolidays)
        {
            SpecialHolidays = specialHolidays;
            RebuildItems();
        }

        /// <summary>
        /// 特別休日一覧
        /// </summary>
        public ObservableCollection<DateOnly> SpecialHolidays { get; }

        /// <summary>
        /// 表示用特別休日一覧
        /// </summary>
        public ObservableCollection<SpecialHolidayItemViewModel> HolidayItems { get; } = [];

        /// <summary>
        /// 対象日が特別休日かどうかを取得する
        /// </summary>
        /// <param name="date">対象日。</param>
        /// <returns>特別休日の場合はtrue。</returns>
        public bool IsSpecialHoliday(DateOnly date)
        {
            return SpecialHolidays.Contains(date);
        }

        /// <summary>
        /// 特別休日を設定する
        /// </summary>
        /// <param name="date">対象日。</param>
        public void AddHoliday(DateOnly date)
        {
            if (SpecialHolidays.Contains(date))
            {
                return;
            }

            SpecialHolidays.Add(date);
            RebuildItems();
        }

        /// <summary>
        /// 特別休日を解除する
        /// </summary>
        /// <param name="date">対象日。</param>
        public void RemoveHoliday(DateOnly date)
        {
            if (!SpecialHolidays.Remove(date))
            {
                return;
            }

            RebuildItems();
        }

        /// <summary>
        /// 表示用特別休日一覧を再構築する
        /// </summary>
        private void RebuildItems()
        {
            HolidayItems.Clear();
            foreach (var date in SpecialHolidays.OrderBy(date => date))
            {
                HolidayItems.Add(new SpecialHolidayItemViewModel(date));
            }
        }
    }
}

/* --- End of file --- */
