using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.GanttChartModel;
using MainApplication.ViewModels.TeamModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// 1つのプロジェクトを表すViewModel
    /// </summary>
    public class ProjectViewModel : ViewModelBase
    {
        private string? _projectName;
        private TabInfo? _selectedTab;

        /// <summary>
        /// プロジェクト名
        /// </summary>
        public string? ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        /// <summary>
        /// プロジェクトID
        /// </summary>
        public Guid ProjectId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ノードエディタViewModel
        /// </summary>
        public NodeEditorViewModel NodeEditor { get; }

        /// <summary>
        /// プロジェクトスケジュール用ガントチャートViewModel
        /// </summary>
        public GanttChartViewModel GanttChart { get; }

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<TabInfo> Tabs { get; }

        /// <summary>
        /// 現在選択されているタブ
        /// </summary>
        public TabInfo? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(
                ref _selectedTab,
                value,
                [nameof(IsNodeEditor)],
                CreateHooksFromValue(
                    value,
                    post: (oldValue, newValue) =>
                    {
                        if (newValue?.Content == GanttChart)
                        {
                            GanttChart.Refresh();
                        }
                    }
                )
            );
        }

        /// <summary>
        /// 現在のタブがNodeEditorかどうか
        /// </summary>
        public bool IsNodeEditor => SelectedTab?.Content == NodeEditor;

        /// <summary>
        /// ProjectViewModelを生成し、子ViewModelやサービスを初期化する
        /// </summary>
        /// <param name="name">プロジェクト名</param>
        /// <param name="members">チームメンバー一覧</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        public ProjectViewModel(
            string name,
            ObservableCollection<TeamMemberViewModel>? members = null,
            ObservableCollection<DateOnly>? specialHolidays = null
        )
        {
            ProjectName = name;
            NodeEditor = new NodeEditorViewModel();
            if (members != null)
            {
                NodeEditor.SetTeamMembers(members);
            }

            GanttChart = new GanttChartViewModel(NodeEditor, specialHolidays);

            var nodeEditorTab = new TabInfo("タスク編集", NodeEditor);
            var ganttChartTab = new TabInfo("プロジェクトスケジュール", GanttChart);
            Tabs =
            [
                nodeEditorTab,
                ganttChartTab
            ];

            SelectedTab = nodeEditorTab;
        }
    }
}

/* --- End of file --- */
