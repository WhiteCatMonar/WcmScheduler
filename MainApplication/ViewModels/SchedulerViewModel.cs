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
            SaveCommand = new RelayCommand(() => Save());
            SaveAsCommand = new RelayCommand(() => RequestSaveAs?.Invoke());
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
         * RootSaveData の構築
         * --------------------------------------------------------- */

        private RootSaveData ToRootDataModel()
        {
            return new RootSaveData
            {
                TaskEditor = NodeEditor.ToTaskEditorDataModel() /* ユーザー：タスク、開発者：ノードという呼称にするため、名称をここで変更 */
                /* TODO:タブごとの機能追加 */
            };
        }

        /* ---------------------------------------------------------
         * View へ SaveAs を依頼するイベント
         * --------------------------------------------------------- */

        public event Action RequestSaveAs;

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
