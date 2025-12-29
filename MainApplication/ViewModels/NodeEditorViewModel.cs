using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using MainApplication.Mappers;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class NodeEditorViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * 基本プロパティ(UI の表示状態)
         * --------------------------------------------------------- */

        private string _modelName;
        public string ModelName
        {
            get => _modelName;
            set
            {
                if (_modelName != value)
                {
                    _modelName = value;
                    OnPropertyChanged(nameof(ModelName));
                }
            }
        }

        private double _baseCanvasWidth;
        public double BaseCanvasWidth
        {
            get => _baseCanvasWidth;
            set
            {
                if (_baseCanvasWidth != value)
                {
                    _baseCanvasWidth = value;
                    OnPropertyChanged(nameof(BaseCanvasWidth));
                    UpdateGridState();
                }
            }
        }

        private double _baseCanvasHeight;
        public double BaseCanvasHeight
        {
            get => _baseCanvasHeight;
            set
            {
                if (_baseCanvasHeight != value)
                {
                    _baseCanvasHeight = value;
                    OnPropertyChanged(nameof(BaseCanvasHeight));
                    UpdateGridState();
                }
            }
        }

        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    OnPropertyChanged(nameof(Zoom));
                    UpdateGridState();
                }
            }
        }

        private double _panX;
        public double PanX
        {
            get => _panX;
            set
            {
                if (_panX != value)
                {
                    _panX = value;
                    OnPropertyChanged(nameof(PanX));
                    UpdateGridState();
                }
            }
        }

        private double _panY;
        public double PanY
        {
            get => _panY;
            set
            {
                if (_panY != value)
                {
                    _panY = value;
                    OnPropertyChanged(nameof(PanY));
                    UpdateGridState();
                }
            }
        }

        /* ---------------------------------------------------------
         * GridManager(論理座標系の中枢)
         * --------------------------------------------------------- */

        public GridManager Grid { get; }

        /* ---------------------------------------------------------
         * ノード・接続線管理
         * --------------------------------------------------------- */

        public NodeCollectionViewModel Nodes { get; }
        public ConnectionCollectionViewModel Connections { get; }

        private void RefreshNodeAndConnectionPositions()
        {
            Nodes.UpdateAllNodes();
            Connections.UpdateAllConnections();
        }

        public void CommitCurrentNodeEdits()
        {
            Nodes.SelectedNode?.CommitEdits();
        }

        /* ---------------------------------------------------------
         * 操作履歴管理
         * --------------------------------------------------------- */

        private UndoRedoManager _undoredo = new UndoRedoManager();
        public UndoRedoManager UndoRedo
        {
            get => _undoredo;
            set
            {
                if (_undoredo != value)
                {
                    _undoredo = value;
                    OnPropertyChanged(nameof(UndoRedo));
                }
            }
        }

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand MoveToHistoryCommand { get; }
        public event EventHandler<UndoRedoManager.HistoryItem> CurrentHistoryChanged;

        private void OnCurrentHistoryChanged(object sender, UndoRedoManager.HistoryItem e)
        {
            CurrentHistoryChanged?.Invoke(this, e);
            RefreshNodeAndConnectionPositions();
        }

        /* ---------------------------------------------------------
         * DateTimeEditorService(日時編集用サービス)
         * --------------------------------------------------------- */

        private IDateTimeEditorService _dateTimeEditor = new DateTimeEditorService();
        public IDateTimeEditorService DateTimeEditor
        {
            get => _dateTimeEditor;
            set
            {
                if (_dateTimeEditor != value)
                {
                    _dateTimeEditor = value;
                    OnPropertyChanged(nameof(DateTimeEditor));
                }
            }
        }

        /* ---------------------------------------------------------
         * データ読み込み関連
         * --------------------------------------------------------- */

        /* 読み込みデータ適用処理 */
        public void LoadFromTaskEditorDataModel(TaskEditorDataModel data)
        {
            Nodes.Nodes.Clear();
            Connections.Connections.Clear();
            NodeEditorViewModel loadedData = NodeEditorMapper.ToViewModel(data, this);


            foreach (var loadedNodes in loadedData.Nodes.Nodes)
            {
                Nodes.Nodes.Add(loadedNodes);
            }

            foreach (var loadedConnections in loadedData.Connections.Connections)
            {
                Connections.Connections.Add(loadedConnections);
            }

            RefreshNodeAndConnectionPositions();

            UndoRedo.Clear();
        }

        /* ---------------------------------------------------------
         * データ保存関連
         * --------------------------------------------------------- */

        /* 保存用データ構築処理 */
        public void SaveToTaskEditorDataModel(out TaskEditorDataModel data)
        {
            data = NodeEditorMapper.ToDataModel(this);
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        public NodeEditorViewModel(string modelName)
        {
            ModelName = modelName;

            Nodes = new NodeCollectionViewModel(UndoRedo, DateTimeEditor, this);
            Connections = new ConnectionCollectionViewModel(UndoRedo, this);
            Grid = new GridManager();

            UndoCommand = new RelayCommand(() =>
            {
                UndoRedo.Undo();
                RefreshNodeAndConnectionPositions();
            }, () => UndoRedo.CanUndo);

            RedoCommand = new RelayCommand(() =>
            {
                UndoRedo.Redo();
                RefreshNodeAndConnectionPositions();
            }, () => UndoRedo.CanRedo);
            MoveToHistoryCommand = new RelayCommand<UndoRedoManager.HistoryItem>(item => UndoRedo.MoveToHistory(item));

            UndoRedo.CurrentHistoryChanged += OnCurrentHistoryChanged;
        }

        /* ---------------------------------------------------------
         * GridManagerにUI状態を反映
         * --------------------------------------------------------- */

        public void UpdateGridState()
        {
            /* ズーム・パン */
            Grid.Zoom = Zoom;
            Grid.PanX = PanX;
            Grid.PanY = PanY;

            /* 論理座標系のサイズ */
            Grid.CanvasViewLogicalWidth = BaseCanvasWidth / Zoom;
            Grid.CanvasViewLogicalHeight = BaseCanvasHeight / Zoom;

            /* 論理原点 */
            Grid.CanvasViewOriginX = -PanX / Zoom;
            Grid.CanvasViewOriginY = -PanY / Zoom;

            /* グリッド線更新 */
            Grid.UpdateGrid();
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}