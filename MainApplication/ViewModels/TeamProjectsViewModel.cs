using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.TeamModel;
using MainApplication.Models.SaveData;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// チーム内の複数プロジェクトを管理するViewModel。
    /// </summary>
    public class TeamProjectsViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * プロジェクト一覧
         * --------------------------------------------------------- */

        public ObservableCollection<ProjectViewModel> Projects { get; }
        private readonly ObservableCollection<TeamMemberViewModel> _members;

        /* ---------------------------------------------------------
         * 選択中のプロジェクト(UIのタブ切り替えなどで使用)
         * --------------------------------------------------------- */

        private ProjectViewModel? _selectedProject;

        /// <summary>
        /// 現在選択されているプロジェクト。
        /// </summary>
        public ProjectViewModel? SelectedProject
        {
            get => _selectedProject;
            set => SetProperty(ref _selectedProject, value);
        }

        /* ---------------------------------------------------------
         * タブ管理
         * --------------------------------------------------------- */

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<TabInfo> Tabs { get; }


        private TabInfo? _selectedTab;

        /// <summary>
        /// 現在選択されているタブ
        /// </summary>
        public TabInfo? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(
                ref _selectedTab,
                value,
                CreateHooksFromValue(
                    value,
                    post: (oldValue, newValue) => {
                        /* タブ切り替え時にプロジェクトも同期 */
                        if (newValue != null)
                        {
                            SelectedProject = (ProjectViewModel)newValue.Content;
                        }
                    }
                )
            );
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// TeamProjectsViewModel を初期化し、プロジェクト一覧を生成する。
        /// </summary>
        public TeamProjectsViewModel(ObservableCollection<TeamMemberViewModel> members)
        {
            _members = members;
            Projects = [];
            var project = new ProjectViewModel("New Project", members);
            Projects.Add(project);

            /* タブ管理 */
            var newProjectTab = new TabInfo(project.ProjectName ?? string.Empty, project);
            Tabs =
            [
                newProjectTab
                /* TODO: タブごとの機能追加 */
            ];

            /* 初期選択タブの設定 */
            SelectedTab = Tabs[0];
        }

        /// <summary>
        /// 保存データからプロジェクト一覧を復元する
        /// </summary>
        /// <param name="projects">プロジェクト保存データ一覧</param>
        public void LoadFromDataModels(IEnumerable<ProjectDataModel> projects)
        {
            Projects.Clear();
            Tabs.Clear();

            foreach (var projectData in projects)
            {
                var project = new ProjectViewModel(projectData.ProjectName ?? string.Empty, _members)
                {
                    ProjectId = projectData.ProjectId == Guid.Empty ? Guid.NewGuid() : projectData.ProjectId
                };
                project.NodeEditor.LoadFromTaskEditorDataModel(projectData.TaskEditor ?? new TaskEditorDataModel());
                Projects.Add(project);
                Tabs.Add(new TabInfo(project.ProjectName ?? string.Empty, project));
            }

            if (Projects.Count == 0)
            {
                var project = new ProjectViewModel("New Project", _members);
                Projects.Add(project);
                Tabs.Add(new TabInfo(project.ProjectName ?? string.Empty, project));
            }

            SelectedTab = Tabs[0];
        }
    }
}

/* --- End of file --- */
