using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.TeamModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// 1つのプロジェクトを表すViewModel。
    /// /// プロジェクト内のタブ管理もここで行う。
    /// </summary>
    public class ProjectViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * プロジェクト全体の基本情報
         * --------------------------------------------------------- */

        private string? _projectName;

        /// <summary>
        /// プロジェクト名(画面タイトルやタブ名として使用)。
        /// </summary>
        public string? ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value);
        }

        /// <summary>
        /// プロジェクトID。
        /// </summary>
        public Guid ProjectId { get; set; } = Guid.NewGuid();

        /* ---------------------------------------------------------
         * 子 ViewModel(タスク編集機能)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードエディタ(タスク編集機能)のViewModel。
        /// </summary>

        public NodeEditorViewModel NodeEditor { get; }

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
            set => SetProperty(ref _selectedTab, value, [nameof(IsNodeEditor)]);
        }

        /// <summary>
        /// 現在のタブがNodeEditorかどうか
        /// </summary>
        public bool IsNodeEditor => SelectedTab?.Content == NodeEditor;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ProjectViewModelを生成し、子ViewModelやサービスを初期化する。
        /// </summary>
        public ProjectViewModel(string name, ObservableCollection<TeamMemberViewModel>? members = null)
        {
            ProjectName = name;

            /* 子となるViewModelの生成 */
            NodeEditor = new NodeEditorViewModel();
            if (members != null)
            {
                NodeEditor.SetTeamMembers(members);
            }

            /* タブ管理 */
            var nodeEditorTab = new TabInfo("タスク編集", NodeEditor);
            Tabs = new ObservableCollection<TabInfo>
            {
                nodeEditorTab
                /* TODO:タブごとの機能追加 */
            };

            SelectedTab = nodeEditorTab;
        }
    }
}

/* --- End of file --- */
