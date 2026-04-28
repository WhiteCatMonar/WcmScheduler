using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート時間軸の日付1日分のViewModel
    /// </summary>
    public class GanttTimelineDayViewModel(DateOnly date, double left, double width) : ViewModelBase
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
    }
}

/* --- End of file --- */
