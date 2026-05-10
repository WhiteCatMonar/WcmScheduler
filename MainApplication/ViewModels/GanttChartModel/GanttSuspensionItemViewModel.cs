using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート上の中断期間表示ViewModel
    /// </summary>
    public class GanttSuspensionItemViewModel(double left, double width) : ViewModelBase
    {
        /// <summary>
        /// 表示左位置
        /// </summary>
        public double Left { get; } = left;

        /// <summary>
        /// 表示幅
        /// </summary>
        public double Width { get; } = width;
    }
}

/* --- End of file --- */
