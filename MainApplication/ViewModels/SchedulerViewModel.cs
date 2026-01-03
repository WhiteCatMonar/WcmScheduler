using MainApplication.Infrastructure;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// アプリケーション全体の状態を管理するViewModel。
    /// タブ管理、保存・読み込み、子ViewModelの生成などを担当する。
    /// </summary>
    public class SchedulerViewModel
    {
        /* ---------------------------------------------------------
         * チーム内プロジェクト管理
         * --------------------------------------------------------- */

        public TeamProjectsViewModel TeamProjects { get; }

        /* ---------------------------------------------------------
         * 保存・読み込み関連定義
         * --------------------------------------------------------- */

        private readonly IJsonSerializerService _jsonSerializer;
        private readonly IFileService _fileService;

        /// <summary>ファイル読み込みコマンド</summary>
        public ICommand LoadCommand { get; }

        /// <summary>上書き保存コマンド</summary>
        public ICommand SaveCommand { get; }

        /// <summary>別名保存コマンド</summary>
        public ICommand SaveAsCommand { get; }

        private string? _currentFilePath;

        /* ---------------------------------------------------------
         * タブ管理
         * --------------------------------------------------------- */

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<object> Tabs { get; }

        private object? _selectedTab;

        /// <summary>
        /// 現在選択されているタブ
        /// </summary>
        public object? SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// SchedulerViewModelを生成し、子ViewModelやサービスを初期化する。
        /// </summary>
        public SchedulerViewModel()
        {
            /* サービス */
            _jsonSerializer = new JsonSerializerService();
            _fileService = new FileService();

            /* 子となるViewModelの生成 */
            TeamProjects = new TeamProjectsViewModel();
            /* TODO:タブごとの機能追加 */

            /* タブ管理 */
            var teamTab = new TabInfo("チーム内プロジェクト", TeamProjects);
            Tabs = new ObservableCollection<object>
            {
                teamTab
                /* TODO:タブごとの機能追加 */
            };

            SelectedTab = teamTab;

            /* コマンド */
            LoadCommand = new RelayCommand(() => RequestLoad?.Invoke());
            SaveCommand = new RelayCommand(() => Save());
            SaveAsCommand = new RelayCommand(() => RequestSaveAs?.Invoke());
        }

        /* ---------------------------------------------------------
         * 読み込み処理
         * --------------------------------------------------------- */
        
        /// <summary>
        /// 指定ファイルからデータを読み込み、ViewModelに適用する。
        /// </summary>
        public void LoadFromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var json = _fileService.LoadText(path);
            var root = _jsonSerializer.Deserialize<RootSaveDataModel>(json);

            if (root == null)
                return;

            ApplyRootDataModel(root);

            _currentFilePath = path;
        }

        /// <summary>
        /// RootSaveDataModelを各ViewModelに適用する。
        /// </summary>
        private void ApplyRootDataModel(RootSaveDataModel root)
        {
            if (root.TaskEditor != null)
            {
                TeamProjects.SelectedProject?.
                    NodeEditor.LoadFromTaskEditorDataModel(root.TaskEditor);

                /* TODO:タブごとの機能追加  */
            }
        }

        /* ---------------------------------------------------------
         * 保存処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在のファイルに上書き保存する。
        /// パスが未設定の場合はSaveAsを要求する。
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                RequestSaveAs?.Invoke();
                return;
            }

            SaveToFile(_currentFilePath);
        }

        /// <summary>
        /// 指定パスに保存する。
        /// </summary>
        public void SaveAs(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            SaveToFile(path);
            _currentFilePath = path;
        }

        /// <summary>
        /// 実際の保存処理(ファイル書き込み)
        /// </summary>
        private void SaveToFile(string path)
        {
            var root = ToRootDataModel();
            var json = _jsonSerializer.Serialize(root);
            _fileService.SaveText(path, json);
        }

        /* ---------------------------------------------------------
         * RootSaveDataの構築
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在の状態をRootSaveDataModelに変換する。
        /// </summary>
        private RootSaveDataModel ToRootDataModel()
        {
            TaskEditorDataModel taskEditor = new();

            /* ユーザー：タスク、開発者：ノードという呼称にするため、名称をここで変更 */
            TeamProjects.SelectedProject?.NodeEditor.SaveToTaskEditorDataModel(out taskEditor);

            RootSaveDataModel save_data = new()
            {
                TaskEditor = taskEditor
            };

            /* TODO:タブごとの機能追加 */

            return save_data;
        }

        /* ---------------------------------------------------------
         * ViewModel → Viewへの依頼イベント
         * --------------------------------------------------------- */
        
        /// <summary>
        /// Viewに「ファイルを開くダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action? RequestLoad;

        /// <summary>
        /// Viewに「名前を付けて保存ダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action? RequestSaveAs;

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
