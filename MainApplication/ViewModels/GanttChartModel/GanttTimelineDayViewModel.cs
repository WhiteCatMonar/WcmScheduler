using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート時間軸の日付1日分のViewModel
    /// </summary>
    public class GanttTimelineDayViewModel(DateOnly date, double left, double width, bool isSpecialHoliday) : ViewModelBase
    {
        /// <summary>
        /// 対象日
        /// </summary>
        public DateOnly Date { get; } = date;

        /// <summary>
        /// 表示左位置
        /// </summary>
        public double Left { get; } = left;

        /// <summary>
        /// 表示幅
        /// </summary>
        public double Width { get; } = width;

        /// <summary>
        /// 日付表示文字列
        /// </summary>
        public string DateText => Date.ToString("MM/dd");

        /// <summary>
        /// 曜日表示文字列
        /// </summary>
        public string DayOfWeekText => Date.ToString("ddd");

        /// <summary>
        /// 土曜日かどうか
        /// </summary>
        public bool IsSaturday => Date.DayOfWeek == DayOfWeek.Saturday;

        /// <summary>
        /// 日曜日かどうか
        /// </summary>
        public bool IsSunday => Date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// 特別休日かどうか
        /// </summary>
        public bool IsSpecialHoliday { get; } = isSpecialHoliday;
    }
}

/* --- End of file --- */
