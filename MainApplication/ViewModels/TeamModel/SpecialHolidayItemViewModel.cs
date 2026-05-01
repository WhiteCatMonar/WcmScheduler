using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// 特別休日一覧に表示する日付
    /// </summary>
    public class SpecialHolidayItemViewModel(DateOnly date) : ViewModelBase
    {
        /// <summary>
        /// 特別休日の日付
        /// </summary>
        public DateOnly Date { get; } = date;

        /// <summary>
        /// 表示用文字列
        /// </summary>
        public string DisplayText => $"{Date:yyyy/MM/dd} {Date:ddd}";
    }
}

/* --- End of file --- */
