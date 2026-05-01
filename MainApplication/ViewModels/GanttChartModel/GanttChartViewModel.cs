using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

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
        private readonly NodeEditorViewModel _nodeEditor;
        private readonly ObservableCollection<DateOnly> _specialHolidays;
        private readonly GanttChartService _service = new();
        private DateOnly _timelineStartDate = DateOnly.FromDateTime(DateTime.Today);
        private DateOnly _timelineEndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(DefaultVisibleDays - 1);
        private double _chartWidth = DefaultDayWidth * DefaultVisibleDays;
        private double _chartHeight = DefaultRowHeight;
        private double _viewportChartWidth = DefaultDayWidth * DefaultVisibleDays;

        /// <summary>
        /// ガントチャートViewModelを生成する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        public GanttChartViewModel(
            NodeEditorViewModel nodeEditor,
            ObservableCollection<DateOnly>? specialHolidays = null
        )
        {
            _nodeEditor = nodeEditor;
            _specialHolidays = specialHolidays ?? [];
            RefreshCommand = new RelayCommand(Refresh);
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
        /// 表示更新コマンド
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// 1日分の表示幅
        /// </summary>
        public double DayWidth => DefaultDayWidth;

        /// <summary>
        /// 1行分の表示高さ
        /// </summary>
        public double RowHeight => DefaultRowHeight;

        /// <summary>
        /// チャート表示幅
        /// </summary>
        public double ChartWidth
        {
            get => _chartWidth;
            private set => SetProperty(ref _chartWidth, value);
        }

        /// <summary>
        /// チャート表示高さ
        /// </summary>
        public double ChartHeight
        {
            get => _chartHeight;
            private set => SetProperty(ref _chartHeight, value);
        }

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
            _nodeEditor.RefreshTaskStatuses();

            var startDate = GetTimelineStartDate();
            TimelineStartDate = startDate;

            var tasks = _service.CreateProjectTasks(_nodeEditor, startDate, DayWidth, RowHeight, _specialHolidays);
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

            _viewportChartWidth = viewportChartWidth;
            RebuildTimeline(TimelineStartDate, _timelineEndDate);
            OnPropertyChangedA(nameof(TimelineRangeText));
        }

        /// <summary>
        /// 表示開始日を取得する
        /// </summary>
        /// <returns>表示開始日</returns>
        private DateOnly GetTimelineStartDate()
        {
            var dates = _nodeEditor.Nodes.Nodes
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
                TimelineDays.Add(new GanttTimelineDayViewModel(startDate.AddDays(index), index * DayWidth, DayWidth));
            }
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

            foreach (var connection in _nodeEditor.Connections.Connections)
            {
                var fromTask = taskByNode.Keys.FirstOrDefault(node => node.OutputPorts.Contains(connection.FromPort));
                var toTask = taskByNode.Keys.FirstOrDefault(node => node.InputPorts.Contains(connection.ToPort));
                if (fromTask == null || toTask == null)
                {
                    continue;
                }

                var source = taskByNode[fromTask];
                var target = taskByNode[toTask];
                var startX = source.BarLeft + source.BarWidth;
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
