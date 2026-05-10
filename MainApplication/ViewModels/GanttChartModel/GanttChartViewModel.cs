using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.StatusBarModel;
using MainApplication.ViewModels.TeamModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート全体のViewModel
    /// </summary>
    public class GanttChartViewModel : ViewModelBase
    {
        private const double DefaultDayWidth = 72.0;
        private const double DefaultRowHeight = 48.0;
        private const int DefaultVisibleDays = 14;
        private readonly DependencyEditorViewModel _dependencyEditor;
        private readonly Func<Guid> _projectIdProvider;
        private readonly ObservableCollection<DateOnly> _specialHolidays;
        private readonly StatusBarViewModel? _statusBar;
        private readonly GanttChartService _service = new();
        private readonly Dispatcher _dispatcher;
        private DateOnly _timelineStartDate = DateOnly.FromDateTime(DateTime.Today);
        private DateOnly _timelineEndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(DefaultVisibleDays - 1);
        private double _dayWidth = DefaultDayWidth;
        private double _chartWidth = DefaultDayWidth * DefaultVisibleDays;
        private double _chartHeight = DefaultRowHeight;
        private double _viewportChartWidth = DefaultDayWidth * DefaultVisibleDays;
        private double _viewportChartHeight = DefaultRowHeight;
        private double _horizontalOffset;
        private double _verticalOffset;
        private bool _isViewportInitialized;
        private int _scrollToTodayRequestCount;
        private bool _isRefreshing;
        private bool _isRefreshQueued;
        private bool _isStatusOperationActive;

        /// <summary>
        /// ガントチャートViewModelを生成する
        /// </summary>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="projectIdProvider">対象プロジェクトID提供元</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        public GanttChartViewModel(
            DependencyEditorViewModel dependencyEditor,
            Func<Guid> projectIdProvider,
            ObservableCollection<DateOnly>? specialHolidays = null,
            StatusBarViewModel? statusBar = null,
            IProjectMemberAvailabilityProvider? memberAvailabilityProvider = null
        )
        {
            _dependencyEditor = dependencyEditor;
            _projectIdProvider = projectIdProvider;
            _specialHolidays = specialHolidays ?? [];
            _statusBar = statusBar;
            MemberAvailabilityProvider = memberAvailabilityProvider;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _dependencyEditor.Nodes.PropertyChanged += NodeCollection_PropertyChanged;
            _dependencyEditor.CurrentHistoryChanged += DependencyEditor_CurrentHistoryChanged;
            _dependencyEditor.Nodes.Nodes.CollectionChanged += Nodes_CollectionChanged;
            _dependencyEditor.Connections.Connections.CollectionChanged += Connections_CollectionChanged;
            _specialHolidays.CollectionChanged += SpecialHolidays_CollectionChanged;
            foreach (var node in _dependencyEditor.Nodes.Nodes)
            {
                SubscribeNode(node);
            }

            RefreshCommand = new RelayCommand(RequestRefresh);
            SelectTaskCommand = new RelayCommand<GanttTaskItemViewModel>(SelectTask, task => task != null);
            Refresh();
        }

        /// <summary>
        /// 表示タスク一覧
        /// </summary>
        public ObservableCollection<GanttTaskItemViewModel> Tasks { get; } = [];

        /// <summary>
        /// 時間軸日付一覧
        /// </summary>
        public ObservableCollection<GanttTimelineDayViewModel> TimelineDays { get; } = [];

        /// <summary>
        /// 依存関係線一覧
        /// </summary>
        public ObservableCollection<GanttDependencyLineViewModel> DependencyLines { get; } = [];

        /// <summary>
        /// プロジェクト内メンバー稼働条件の参照先
        /// </summary>
        public IProjectMemberAvailabilityProvider? MemberAvailabilityProvider { get; set; }

        /// <summary>
        /// 表示更新コマンド
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// ガントチャート上のタスク選択コマンド
        /// </summary>
        public ICommand SelectTaskCommand { get; }

        /// <summary>
        /// 1日分の表示幅
        /// </summary>
        public double DayWidth
        {
            get => _dayWidth;
            private set => SetProperty(
                ref _dayWidth,
                value,
                [
                    nameof(HorizontalScaleText),
                    nameof(TodayHorizontalOffset)
                ]
            );
        }

        /// <summary>
        /// 1行分の表示高さ
        /// </summary>
        public double RowHeight => DefaultRowHeight;

        /// <summary>
        /// 現在日付へ横スクロールする要求回数
        /// </summary>
        public int ScrollToTodayRequestCount
        {
            get => _scrollToTodayRequestCount;
            private set => SetProperty(ref _scrollToTodayRequestCount, value);
        }

        /// <summary>
        /// 現在日付を表示するための横スクロール位置
        /// </summary>
        public double TodayHorizontalOffset
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var todayItem = TimelineDays.FirstOrDefault(day => day.Date == today);
                return todayItem == null ? 0.0 : Math.Max(0.0, todayItem.Left - DayWidth);
            }
        }

        /// <summary>
        /// チャート表示幅
        /// </summary>
        public double ChartWidth
        {
            get => _chartWidth;
            private set
            {
                if (SetProperty(ref _chartWidth, value, [nameof(MaxHorizontalOffset)]))
                {
                    OnScrollRangeChanged();
                    HorizontalOffset = _horizontalOffset;
                }
            }
        }

        /// <summary>
        /// チャート表示高さ
        /// </summary>
        public double ChartHeight
        {
            get => _chartHeight;
            private set
            {
                if (SetProperty(ref _chartHeight, value, [nameof(MaxVerticalOffset)]))
                {
                    OnScrollRangeChanged();
                    VerticalOffset = _verticalOffset;
                }
            }
        }

        /// <summary>
        /// チャート表示領域幅
        /// </summary>
        public double ViewportChartWidth
        {
            get => _viewportChartWidth;
            private set
            {
                if (SetProperty(ref _viewportChartWidth, value, [nameof(MaxHorizontalOffset)]))
                {
                    OnScrollRangeChanged();
                    HorizontalOffset = _horizontalOffset;
                }
            }
        }

        /// <summary>
        /// チャート表示領域高さ
        /// </summary>
        public double ViewportChartHeight
        {
            get => _viewportChartHeight;
            private set
            {
                if (SetProperty(ref _viewportChartHeight, value, [nameof(MaxVerticalOffset)]))
                {
                    OnScrollRangeChanged();
                    VerticalOffset = _verticalOffset;
                }
            }
        }

        /// <summary>
        /// 横スクロール位置
        /// </summary>
        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set => SetProperty(
                ref _horizontalOffset,
                Clamp(value, 0.0, MaxHorizontalOffset),
                [nameof(ChartTranslateX)]
            );
        }

        /// <summary>
        /// 縦スクロール位置
        /// </summary>
        public double VerticalOffset
        {
            get => _verticalOffset;
            set => SetProperty(
                ref _verticalOffset,
                Clamp(value, 0.0, MaxVerticalOffset),
                [nameof(ChartTranslateY)]
            );
        }

        /// <summary>
        /// 横スクロール最大値
        /// </summary>
        public double MaxHorizontalOffset => Math.Max(0.0, ChartWidth - ViewportChartWidth);

        /// <summary>
        /// 縦スクロール最大値
        /// </summary>
        public double MaxVerticalOffset => Math.Max(0.0, ChartHeight - ViewportChartHeight);

        /// <summary>
        /// チャート描画の横移動量
        /// </summary>
        public double ChartTranslateX => -HorizontalOffset;

        /// <summary>
        /// チャート描画の縦移動量
        /// </summary>
        public double ChartTranslateY => -VerticalOffset;

        /// <summary>
        /// 横スクロールバーのつまみサイズ
        /// </summary>
        public double HorizontalViewportSize => Math.Max(0.0, ViewportChartWidth);

        /// <summary>
        /// 縦スクロールバーのつまみサイズ
        /// </summary>
        public double VerticalViewportSize => Math.Max(0.0, ViewportChartHeight);

        /// <summary>
        /// 横スクロールバーの表示可否
        /// </summary>
        public bool CanScrollHorizontally => MaxHorizontalOffset > 0.0;

        /// <summary>
        /// 縦スクロールバーの表示可否
        /// </summary>
        public bool CanScrollVertically => MaxVerticalOffset > 0.0;

        /// <summary>
        /// 横方向拡縮率表示
        /// </summary>
        public string HorizontalScaleText => $"{DayWidth / DefaultDayWidth:P0}";

        /// <summary>
        /// 表示開始日
        /// </summary>
        public DateOnly TimelineStartDate
        {
            get => _timelineStartDate;
            private set => SetProperty(ref _timelineStartDate, value, [nameof(TimelineRangeText)]);
        }

        /// <summary>
        /// 表示範囲文字列
        /// </summary>
        public string TimelineRangeText
        {
            get
            {
                if (TimelineDays.Count == 0)
                {
                    return "";
                }

                return $"{TimelineDays.First().Date:yyyy/MM/dd} - {TimelineDays.Last().Date:yyyy/MM/dd}";
            }
        }

        /// <summary>
        /// 表示タスクが存在しないかどうか
        /// </summary>
        public bool HasNoTasks => Tasks.Count == 0;

        /// <summary>
        /// ガントチャート表示を更新する
        /// </summary>
        public void Refresh()
        {
            Refresh(true);
        }

        /// <summary>
        /// ガントチャート表示を更新する
        /// </summary>
        /// <param name="requestScrollToToday">更新後に現在日付へ移動するか</param>
        private void Refresh(bool requestScrollToToday)
        {
            if (_isRefreshing)
            {
                return;
            }

            _isRefreshing = true;
            try
            {
                _dependencyEditor.RefreshTaskStatuses();

                var startDate = GetTimelineStartDate();
                TimelineStartDate = startDate;

                var tasks = _service.CreateProjectTasks(
                    _dependencyEditor,
                    _projectIdProvider(),
                    MemberAvailabilityProvider,
                    startDate,
                    DayWidth,
                    RowHeight,
                    _specialHolidays
                );
                var endDate = GetTimelineEndDate(tasks, startDate);
                _timelineEndDate = endDate;
                RebuildTimeline(startDate, endDate);

                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }

                RebuildDependencyLines();
                ChartHeight = Math.Max(RowHeight, Tasks.Count * RowHeight);
                OnPropertyChangedA(nameof(TimelineRangeText));
                OnPropertyChangedA(nameof(HasNoTasks));
                if (requestScrollToToday)
                {
                    RequestScrollToToday();
                }
                else
                {
                    ClampScrollOffsets();
                }

                if (_isStatusOperationActive)
                {
                    _statusBar?.CompleteOperation("プロジェクトスケジュールを更新しました");
                }
            }
            catch
            {
                if (_isStatusOperationActive)
                {
                    _statusBar?.FailOperation("プロジェクトスケジュール更新に失敗しました");
                }

                throw;
            }
            finally
            {
                _isRefreshing = false;
                _isStatusOperationActive = false;
            }
        }

        /// <summary>
        /// チャート表示領域幅を反映する
        /// </summary>
        /// <param name="viewportChartWidth">チャート表示領域幅</param>
        public void SetViewportChartWidth(double viewportChartWidth)
        {
            if (viewportChartWidth <= 0 || Math.Abs(_viewportChartWidth - viewportChartWidth) < 1.0)
            {
                return;
            }

            ViewportChartWidth = viewportChartWidth;
            RebuildTimeline(TimelineStartDate, _timelineEndDate);
            OnPropertyChangedA(nameof(TimelineRangeText));
            if (!_isViewportInitialized)
            {
                _isViewportInitialized = true;
                RequestScrollToToday();
            }
        }

        /// <summary>
        /// チャート表示領域高さを反映する
        /// </summary>
        /// <param name="viewportChartHeight">チャート表示領域高さ</param>
        public void SetViewportChartHeight(double viewportChartHeight)
        {
            if (viewportChartHeight <= 0 || Math.Abs(_viewportChartHeight - viewportChartHeight) < 1.0)
            {
                return;
            }

            ViewportChartHeight = viewportChartHeight;
        }

        /// <summary>
        /// チャート表示位置を移動する
        /// </summary>
        /// <param name="horizontalDelta">横方向移動量</param>
        /// <param name="verticalDelta">縦方向移動量</param>
        public void ScrollBy(double horizontalDelta, double verticalDelta)
        {
            HorizontalOffset += horizontalDelta;
            VerticalOffset += verticalDelta;
        }

        /// <summary>
        /// 横方向表示倍率を変更する
        /// </summary>
        /// <param name="factor">倍率変更係数</param>
        /// <param name="anchorX">拡縮基準の表示領域内X座標</param>
        public void ZoomHorizontal(double factor, double anchorX)
        {
            var oldDayWidth = DayWidth;
            var newDayWidth = Clamp(oldDayWidth * factor, 24.0, 240.0);
            if (Math.Abs(oldDayWidth - newDayWidth) < 0.1)
            {
                return;
            }

            var anchorDayPosition = (HorizontalOffset + Math.Max(0.0, anchorX)) / oldDayWidth;
            DayWidth = newDayWidth;
            Refresh(false);
            HorizontalOffset = (anchorDayPosition * newDayWidth) - Math.Max(0.0, anchorX);
        }

        /// <summary>
        /// ガントチャート上のタスクを選択する
        /// </summary>
        /// <param name="task">選択対象タスク</param>
        public void SelectTask(GanttTaskItemViewModel? task)
        {
            if (task == null)
            {
                return;
            }

            _dependencyEditor.Nodes.SelectNode(task.Node);
        }

        /// <summary>
        /// ノード選択状態の変更をガントチャート表示へ反映する
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">プロパティ変更情報</param>
        private void NodeCollection_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TaskNodeCollectionViewModel.SelectedNode))
            {
                return;
            }

            foreach (var task in Tasks)
            {
                task.RefreshSelectionState();
            }
        }

        /// <summary>
        /// 更新要求を短い遅延で集約する
        /// </summary>
        public void RequestRefresh()
        {
            if (_isRefreshing || _isRefreshQueued)
            {
                return;
            }

            _isRefreshQueued = true;
            _isStatusOperationActive = true;
            _statusBar?.BeginOperation("プロジェクトスケジュールを更新中...");
            _dispatcher.BeginInvoke(
                RefreshQueued,
                DispatcherPriority.Background
            );
        }

        /// <summary>
        /// キュー登録されたガントチャート更新を実行する
        /// </summary>
        private void RefreshQueued()
        {
            _isRefreshQueued = false;
            Refresh();
        }

        /// <summary>
        /// ノード一覧変更時に監視対象を更新する
        /// </summary>
        private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TaskNodeViewModel node in e.OldItems)
                {
                    UnsubscribeNode(node);
                }
            }

            if (e.NewItems != null)
            {
                foreach (TaskNodeViewModel node in e.NewItems)
                {
                    SubscribeNode(node);
                }
            }

            RequestRefresh();
        }

        /// <summary>
        /// 接続線一覧変更時にガントチャートを更新する
        /// </summary>
        private void Connections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RequestRefresh();
        }

        /// <summary>
        /// 特別休日一覧変更時にガントチャートを更新する
        /// </summary>
        private void SpecialHolidays_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RequestRefresh();
        }

        /// <summary>
        /// ノード変更監視を開始する
        /// </summary>
        private void SubscribeNode(TaskNodeViewModel node)
        {
            node.PropertyChanged += Node_PropertyChanged;
            node.Detail.PropertyChanged += NodeDetail_PropertyChanged;
        }

        /// <summary>
        /// ノード変更監視を解除する
        /// </summary>
        private void UnsubscribeNode(TaskNodeViewModel node)
        {
            node.PropertyChanged -= Node_PropertyChanged;
            node.Detail.PropertyChanged -= NodeDetail_PropertyChanged;
        }

        /// <summary>
        /// ノード状態変更時にガントチャートを更新する
        /// </summary>
        private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TaskNodeViewModel node && e.PropertyName == nameof(TaskNodeViewModel.Status))
            {
                RefreshTaskDisplayForNode(node);
            }
        }

        /// <summary>
        /// タスク詳細変更時にガントチャートを更新する
        /// </summary>
        private void NodeDetail_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TaskDetailViewModel detail && e.PropertyName == nameof(TaskDetailViewModel.TaskName))
            {
                RefreshTaskDisplayForDetail(detail);
            }
        }

        /// <summary>
        /// 編集履歴確定時にガントチャートを更新する
        /// </summary>
        private void DependencyEditor_CurrentHistoryChanged(object? sender, UndoRedoManager.HistoryItem? e)
        {
            RequestRefresh();
        }

        /// <summary>
        /// 指定ノードに対応するタスク行の表示状態を更新する
        /// </summary>
        /// <param name="node">更新対象ノード</param>
        private void RefreshTaskDisplayForNode(TaskNodeViewModel node)
        {
            foreach (var task in Tasks.Where(task => ReferenceEquals(task.Node, node)))
            {
                task.RefreshNodeDisplayState();
            }
        }

        /// <summary>
        /// 指定タスク詳細に対応するタスク行の表示状態を更新する
        /// </summary>
        /// <param name="detail">更新対象タスク詳細</param>
        private void RefreshTaskDisplayForDetail(TaskDetailViewModel detail)
        {
            foreach (var task in Tasks.Where(task => ReferenceEquals(task.Node.Detail, detail)))
            {
                task.RefreshNodeDisplayState();
            }
        }

        /// <summary>
        /// 現在日付が見える位置へ横スクロールする
        /// </summary>
        public void ScrollToToday()
        {
            HorizontalOffset = TodayHorizontalOffset;
        }

        /// <summary>
        /// スクロール範囲変更に連動するプロパティ変更通知を発行する
        /// </summary>
        private void OnScrollRangeChanged()
        {
            OnPropertyChangedA(nameof(HorizontalViewportSize));
            OnPropertyChangedA(nameof(VerticalViewportSize));
            OnPropertyChangedA(nameof(CanScrollHorizontally));
            OnPropertyChangedA(nameof(CanScrollVertically));
        }

        /// <summary>
        /// 現在のスクロール位置を表示可能範囲に収める
        /// </summary>
        private void ClampScrollOffsets()
        {
            HorizontalOffset = _horizontalOffset;
            VerticalOffset = _verticalOffset;
        }

        /// <summary>
        /// 値を指定範囲内に収める
        /// </summary>
        /// <param name="value">対象値</param>
        /// <param name="minimum">最小値</param>
        /// <param name="maximum">最大値</param>
        /// <returns>範囲内に収めた値</returns>
        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }

        /// <summary>
        /// 表示開始日を取得する
        /// </summary>
        /// <returns>表示開始日</returns>
        private DateOnly GetTimelineStartDate()
        {
            var dates = _dependencyEditor.Nodes.Nodes
                .SelectMany(node => new[]
                {
                    node.Detail.StartDateTime,
                    node.Detail.EndDateTime
                })
                .Where(value => value != null)
                .Select(value => DateOnly.FromDateTime(value!.Value.Date))
                .ToList();

            dates.Add(DateOnly.FromDateTime(DateTime.Today));
            return dates.Min().AddDays(-1);
        }

        /// <summary>
        /// 表示終了日を取得する
        /// </summary>
        /// <param name="tasks">表示タスク一覧</param>
        /// <param name="startDate">表示開始日</param>
        /// <returns>表示終了日</returns>
        private static DateOnly GetTimelineEndDate(IEnumerable<GanttTaskItemViewModel> tasks, DateOnly startDate)
        {
            var endDate = tasks
                .Select(task => DateOnly.FromDateTime(task.EndDateTime.Date))
                .DefaultIfEmpty(startDate.AddDays(DefaultVisibleDays - 1))
                .Max();

            var minimumEnd = startDate.AddDays(DefaultVisibleDays - 1);
            return endDate > minimumEnd ? endDate.AddDays(1) : minimumEnd;
        }

        /// <summary>
        /// 時間軸を再構築する
        /// </summary>
        /// <param name="startDate">表示開始日</param>
        /// <param name="endDate">表示終了日</param>
        private void RebuildTimeline(DateOnly startDate, DateOnly endDate)
        {
            TimelineDays.Clear();
            var minimumViewportDays = Math.Max(DefaultVisibleDays, (int)Math.Ceiling(_viewportChartWidth / DayWidth));
            var dayCount = Math.Max(minimumViewportDays, endDate.DayNumber - startDate.DayNumber + 1);
            ChartWidth = dayCount * DayWidth;

            for (var index = 0; index < dayCount; index++)
            {
                var date = startDate.AddDays(index);
                TimelineDays.Add(new GanttTimelineDayViewModel(
                    date,
                    index * DayWidth,
                    DayWidth,
                    _specialHolidays.Contains(date)
                ));
            }

            OnPropertyChangedA(nameof(TodayHorizontalOffset));
        }

        /// <summary>
        /// 現在日付へのスクロール要求を発行する
        /// </summary>
        private void RequestScrollToToday()
        {
            ScrollToTodayRequestCount++;
        }

        /// <summary>
        /// 依存関係線を再構築する
        /// </summary>
        private void RebuildDependencyLines()
        {
            DependencyLines.Clear();
            var taskByNode = Tasks
                .Where(task => task.HasSchedule)
                .ToDictionary(task => task.Node);

            foreach (var connection in _dependencyEditor.Connections.Connections)
            {
                var fromTask = taskByNode.Keys.FirstOrDefault(node => node.OutputPorts.Contains(connection.FromPort));
                var toTask = taskByNode.Keys.FirstOrDefault(node => node.InputPorts.Contains(connection.ToPort));
                if (fromTask == null || toTask == null)
                {
                    continue;
                }

                var source = taskByNode[fromTask];
                var target = taskByNode[toTask];
                var startX = source.BarLeft + source.ScheduleBarWidth;
                var startY = source.RowTop + (RowHeight / 2.0);
                var endX = Math.Max(0.0, target.BarLeft);
                var endY = target.RowTop + (RowHeight / 2.0);

                DependencyLines.Add(
                    new GanttDependencyLineViewModel(
                        source.TaskName,
                        target.TaskName,
                        startX,
                        startY,
                        endX,
                        endY
                    )
                );
            }
        }
    }
}

/* --- End of file --- */
