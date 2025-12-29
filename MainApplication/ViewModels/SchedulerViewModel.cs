using MainApplication.Infrastructure;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication
{
    /// <summary>
    /// アプリ全体を統括するルートViewModel。
    /// タブ管理、保存・読み込み、子ViewModelの生成などを担当する。
    /// </summary>
    public class SchedulerViewModel
    {
        /* ---------------------------------------------------------
         * 子ViewModel(アプリの各機能)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードエディタ(タスク編集機能)のViewModel
        /// </summary>
        public NodeEditorViewModel NodeEditor { get; }

        /* TODO:タブごとの機能追加 */

        /* ---------------------------------------------------------
         * タブ管理
         * --------------------------------------------------------- */

        /// <summary>
        /// 表示中のタブ一覧
        /// </summary>
        public ObservableCollection<object> Tabs { get; }

        private object _selectedTab;

        /// <summary>
        /// 現在選択されているタブ
        /// </summary>
        public object SelectedTab
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
        public bool IsNodeEditor => SelectedTab == NodeEditor;

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

        private string _currentFilePath;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// SchedulerViewModelを生成し、子ViewModelやサービスを初期化する。
        /// </summary>
        public SchedulerViewModel(Dictionary<string,string> modelNames)
        {
            /* サービス */
            _jsonSerializer = new JsonSerializerService();
            _fileService = new FileService();

            /* 子となるViewModelの生成 */
            NodeEditor = new NodeEditorViewModel(modelNames["NodeEditor"]);
            /* TODO:タブごとの機能追加 */

            /* タブ管理 */
            Tabs = new ObservableCollection<object>
            {
                NodeEditor
                /* TODO:タブごとの機能追加 */
            };
            SelectedTab = NodeEditor;

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
            RootSaveDataModel save_data = new RootSaveDataModel();
            /* ユーザー：タスク、開発者：ノードという呼称にするため、名称をここで変更 */
            NodeEditor.SaveToTaskEditorDataModel(out TaskEditorDataModel TaskEditor);
            save_data.TaskEditor = TaskEditor;

            /* TODO:タブごとの機能追加 */

            return save_data;
        }

        /* ---------------------------------------------------------
         * ViewModel → Viewへの依頼イベント
         * --------------------------------------------------------- */
        
        /// <summary>
        /// Viewに「ファイルを開くダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action RequestLoad;

        /// <summary>
        /// Viewに「名前を付けて保存ダイアログを表示してほしい」と依頼するイベント
        /// </summary>
        public event Action RequestSaveAs;

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
