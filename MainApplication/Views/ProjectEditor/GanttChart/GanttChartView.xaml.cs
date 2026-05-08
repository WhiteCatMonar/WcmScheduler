using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using MainApplication.ViewModels.GanttChartModel;

namespace MainApplication.Views.ProjectEditor.GanttChart
{
    /// <summary>
    /// ガントチャートView
    /// </summary>
    public partial class GanttChartView : UserControl
    {
        private GanttChartViewModel? _subscribedViewModel;

        /// <summary>
        /// GanttChartViewを初期化する
        /// </summary>
        public GanttChartView()
        {
            InitializeComponent();
            Loaded += GanttChartView_Loaded;
            DataContextChanged += GanttChartView_DataContextChanged;
        }

        /// <summary>
        /// 表示完了時に現在日付へスクロールする
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void GanttChartView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollToToday();
        }

        /// <summary>
        /// DataContext変更時にViewModelのスクロール要求を監視する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void GanttChartView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.PropertyChanged -= GanttChartViewModel_PropertyChanged;
            }

            _subscribedViewModel = e.NewValue as GanttChartViewModel;
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.PropertyChanged += GanttChartViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// ViewModelからの現在日付スクロール要求を処理する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void GanttChartViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GanttChartViewModel.ScrollToTodayRequestCount))
            {
                ScrollToToday();
            }
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
                viewModel.SetViewportChartWidth(e.NewSize.Width);
            }
        }

        /// <summary>
        /// ホイール操作時にチャート部分のスクロールを制御する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void ChartScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
                return;
            }

            VerticalScrollViewer.ScrollToVerticalOffset(VerticalScrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// ガントチャート上のタスク行またはバー選択をViewModelへ通知する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">マウスイベント引数</param>
        private void GanttTaskItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GanttChartViewModel viewModel &&
                sender is FrameworkElement element &&
                element.DataContext is GanttTaskItemViewModel task)
            {
                viewModel.SelectTask(task);
                e.Handled = true;
            }
        }

        /// <summary>
        /// レイアウト完了後に現在日付へ横スクロールする
        /// </summary>
        private void ScrollToToday()
        {
            if (DataContext is not GanttChartViewModel viewModel)
            {
                return;
            }

            Dispatcher.BeginInvoke(
                () => ChartScrollViewer.ScrollToHorizontalOffset(viewModel.TodayHorizontalOffset),
                DispatcherPriority.Loaded
            );
        }
    }
}

/* --- End of file --- */
