using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート上の稼働不可区間を表すViewModel
    /// </summary>
    /// <param name="left">タスクバー内の左位置</param>
    /// <param name="width">表示幅</param>
    public class GanttUnavailablePeriodItemViewModel(double left, double width) : ViewModelBase
    {
        /// <summary>
        /// タスクバー内の左位置
        /// </summary>
        public double Left { get; } = left;

        /// <summary>
        /// 表示幅
        /// </summary>
        public double Width { get; } = width;
    }
}

/* --- End of file --- */
