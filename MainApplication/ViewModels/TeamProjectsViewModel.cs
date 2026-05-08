using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.StatusBarModel;
using MainApplication.ViewModels.TeamModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// チーム内プロジェクトを管理するViewModel
    /// </summary>
    public class TeamProjectsViewModel : ViewModelBase
    {
        private readonly ObservableCollection<TeamMemberViewModel> _members;
        private readonly ObservableCollection<DateOnly> _specialHolidays;
        private readonly StatusBarViewModel? _statusBar;
        private ProjectViewModel? _selectedProject;
        private TabInfo? _selectedTab;

        /// <summary>
        /// プロジェクト一覧
        /// </summary>
        public ObservableCollection<ProjectViewModel> Projects { get; }

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<TabInfo> Tabs { get; }

        /// <summary>
        /// プロジェクト追加コマンド
        /// </summary>
        public ICommand AddProjectCommand { get; }

        /// <summary>
        /// 選択中プロジェクト削除コマンド
        /// </summary>
        public ICommand DeleteSelectedProjectCommand { get; }

        /// <summary>
        /// 現在選択されているプロジェクト
        /// </summary>
        public ProjectViewModel? SelectedProject
        {
            get => _selectedProject;
            set => SetProperty(ref _selectedProject, value, [nameof(CanDeleteSelectedProject)]);
        }

        /// <summary>
        /// 選択中プロジェクトを削除できるかどうか
        /// </summary>
        public bool CanDeleteSelectedProject => SelectedProject != null && Projects.Count > 1;

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
                    post: (oldValue, newValue) =>
                    {
                        if (newValue != null)
                        {
                            SelectedProject = (ProjectViewModel)newValue.Content;
                        }
                    }
                )
            );
        }

        /// <summary>
        /// TeamProjectsViewModelを初期化し、プロジェクト一覧を生成する
        /// </summary>
        /// <param name="members">チームメンバー一覧</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        public TeamProjectsViewModel(
            ObservableCollection<TeamMemberViewModel> members,
            ObservableCollection<DateOnly> specialHolidays,
            StatusBarViewModel? statusBar = null
        )
        {
            _members = members;
            _specialHolidays = specialHolidays;
            _statusBar = statusBar;
            Projects = [];

            var project = new ProjectViewModel("New Project", members, specialHolidays, statusBar);
            Projects.Add(project);

            var newProjectTab = CreateProjectTab(project);
            Tabs =
            [
                newProjectTab
            ];

            SelectedTab = Tabs[0];

            AddProjectCommand = new RelayCommand(AddProject);
            DeleteSelectedProjectCommand = new RelayCommand(DeleteSelectedProject, () => CanDeleteSelectedProject);
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
                var project = new ProjectViewModel(projectData.ProjectName ?? string.Empty, _members, _specialHolidays, _statusBar)
                {
                    ProjectId = projectData.ProjectId == Guid.Empty ? Guid.NewGuid() : projectData.ProjectId
                };
                project.DependencyEditor.LoadFromTaskEditorDataModel(projectData.TaskEditor ?? new TaskEditorDataModel());
                Projects.Add(project);
                Tabs.Add(CreateProjectTab(project));
            }

            if (Projects.Count == 0)
            {
                var project = new ProjectViewModel("New Project", _members, _specialHolidays, _statusBar);
                Projects.Add(project);
                Tabs.Add(CreateProjectTab(project));
            }

            SelectedTab = Tabs[0];
            OnPropertyChangedA(nameof(CanDeleteSelectedProject));
        }

        /// <summary>
        /// プロジェクトタブを作成する
        /// </summary>
        /// <param name="project">対象プロジェクト</param>
        /// <returns>プロジェクトタブ情報</returns>
        private static TabInfo CreateProjectTab(ProjectViewModel project)
        {
            return new TabInfo(project.ProjectName ?? string.Empty, project);
        }

        /// <summary>
        /// プロジェクトを追加する
        /// </summary>
        private void AddProject()
        {
            var project = new ProjectViewModel(CreateDefaultProjectName(), _members, _specialHolidays, _statusBar);
            var tab = CreateProjectTab(project);
            Projects.Add(project);
            Tabs.Add(tab);
            SelectedTab = tab;
            OnPropertyChangedA(nameof(CanDeleteSelectedProject));
        }

        /// <summary>
        /// 選択中プロジェクトを削除する
        /// </summary>
        private void DeleteSelectedProject()
        {
            if (SelectedProject == null || Projects.Count <= 1)
            {
                return;
            }

            var project = SelectedProject;
            var tab = Tabs.FirstOrDefault(item => ReferenceEquals(item.Content, project));
            var index = tab == null ? Projects.IndexOf(project) : Tabs.IndexOf(tab);

            Projects.Remove(project);
            if (tab != null)
            {
                Tabs.Remove(tab);
            }

            var nextIndex = Math.Clamp(index, 0, Tabs.Count - 1);
            SelectedTab = Tabs[nextIndex];
            OnPropertyChangedA(nameof(CanDeleteSelectedProject));
        }

        /// <summary>
        /// 追加プロジェクトの初期名を作成する
        /// </summary>
        /// <returns>重複しない初期プロジェクト名</returns>
        private string CreateDefaultProjectName()
        {
            const string baseName = "New Project";
            if (!Projects.Any(project => project.ProjectName == baseName))
            {
                return baseName;
            }

            var index = 2;
            while (Projects.Any(project => project.ProjectName == $"{baseName} {index}"))
            {
                index++;
            }

            return $"{baseName} {index}";
        }
    }
}

/* --- End of file --- */
