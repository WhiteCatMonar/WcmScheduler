using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// 1つのプロジェクトを表すViewModel。
    /// /// プロジェクト内のタブ管理もここで行う。
    /// </summary>
    public class ProjectViewModel : INotifyPropertyChanged
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
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

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
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
                OnPropertyChanged(nameof(IsNodeEditor));
            }
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
        public ProjectViewModel(string name)
        {
            ProjectName = name;

            /* 子となるViewModelの生成 */
            NodeEditor = new NodeEditorViewModel();

            /* タブ管理 */
            var nodeEditorTab = new TabInfo("タスク編集", NodeEditor);
            Tabs = new ObservableCollection<TabInfo>
            {
                nodeEditorTab
                /* TODO:タブごとの機能追加 */
            };

            SelectedTab = nodeEditorTab;
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
