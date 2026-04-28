using System.Windows.Controls;
using MainApplication.ViewModels.GanttChartModel;

namespace MainApplication.Views
{
    /// <summary>
    /// ガントチャートView
    /// </summary>
    public partial class GanttChartView : UserControl
    {
        /// <summary>
        /// GanttChartViewを初期化する
        /// </summary>
        public GanttChartView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// チャート表示領域のサイズ変更をViewModelへ反映する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void ChartScrollViewer_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (DataContext is GanttChartViewModel viewModel)
            {
                viewModel.SetViewportChartWidth(e.NewSize.Width - 320.0);
            }
        }
    }
}

/* --- End of file --- */
