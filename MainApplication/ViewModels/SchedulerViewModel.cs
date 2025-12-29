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
    public class SchedulerViewModel
    {
        /* ---------------------------------------------------------
         * 子ViewModel(アプリの各機能)
         * --------------------------------------------------------- */

        public NodeEditorViewModel NodeEditor { get; }
        /* TODO:タブごとの機能追加 */

        /* ---------------------------------------------------------
         * タブ管理
         * --------------------------------------------------------- */

        public ObservableCollection<object> Tabs { get; }

        private object _selectedTab;
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

        public bool IsNodeEditor => SelectedTab == NodeEditor;

        /* ---------------------------------------------------------
         * 保存・読み込み関連定義
         * --------------------------------------------------------- */

        private readonly IJsonSerializerService _jsonSerializer;
        private readonly IFileService _fileService;

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }

        private string _currentFilePath;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

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

        public void Save()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                RequestSaveAs?.Invoke();
                return;
            }

            SaveToFile(_currentFilePath);
        }

        public void SaveAs(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            SaveToFile(path);
            _currentFilePath = path;
        }

        private void SaveToFile(string path)
        {
            var root = ToRootDataModel();
            var json = _jsonSerializer.Serialize(root);
            _fileService.SaveText(path, json);
        }

        /* ---------------------------------------------------------
         * RootSaveDataの構築
         * --------------------------------------------------------- */

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
         * ViewModelからViewへ依頼するためのイベント
         * --------------------------------------------------------- */

        public event Action RequestLoad;
        public event Action RequestSaveAs;

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
