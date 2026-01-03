using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// チーム内の複数プロジェクトを管理するViewModel。
    /// </summary>
    public class TeamProjectsViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * プロジェクト一覧
         * --------------------------------------------------------- */

        public ObservableCollection<ProjectViewModel> Projects { get; }

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
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    OnPropertyChanged(nameof(SelectedProject));
                }
            }
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
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    OnPropertyChanged(nameof(SelectedTab));

                    /* タブ切り替え時にプロジェクトも同期 */
                    if (_selectedTab != null)
                    {
                        SelectedProject = (ProjectViewModel)_selectedTab.Content;
                    }
                }
            }
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// TeamProjectsViewModel を初期化し、プロジェクト一覧を生成する。
        /// </summary>
        public TeamProjectsViewModel()
        {
            Projects = [];
            var project = new ProjectViewModel("New Project");
            Projects.Add(project);

            /* タブ管理 */
            var newProjectTab = new TabInfo(project.ProjectName ?? string.Empty, project);
            Tabs =
            [
                newProjectTab
                /* TODO:タブごとの機能追加 */
            ];

            /* 初期選択タブの設定 */
            SelectedTab = Tabs[0];
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する。
        /// </summary>
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
