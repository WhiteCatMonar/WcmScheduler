using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.GanttChartModel;
using MainApplication.ViewModels.StatusBarModel;
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
        /// 依存関係編集ViewModel
        /// </summary>
        public DependencyEditorViewModel DependencyEditor { get; }

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
                [nameof(IsDependencyEditor)],
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
        /// 現在のタブが依存関係編集かどうか
        /// </summary>
        public bool IsDependencyEditor => SelectedTab?.Content == DependencyEditor;

        /// <summary>
        /// ProjectViewModelを生成し、子ViewModelやサービスを初期化する
        /// </summary>
        /// <param name="name">プロジェクト名</param>
        /// <param name="members">チームメンバー一覧</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        public ProjectViewModel(
            string name,
            ObservableCollection<TeamMemberViewModel>? members = null,
            ObservableCollection<DateOnly>? specialHolidays = null,
            StatusBarViewModel? statusBar = null
        )
        {
            ProjectName = name;
            DependencyEditor = new DependencyEditorViewModel();
            if (members != null)
            {
                DependencyEditor.SetTeamMembers(members);
            }

            GanttChart = new GanttChartViewModel(DependencyEditor, specialHolidays, statusBar);

            var dependencyEditorTab = new TabInfo("タスク編集", DependencyEditor);
            var ganttChartTab = new TabInfo("プロジェクトスケジュール", GanttChart);
            Tabs =
            [
                dependencyEditorTab,
                ganttChartTab
            ];

            SelectedTab = dependencyEditorTab;
        }
    }
}

/* --- End of file --- */
