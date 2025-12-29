using MainApplication.Infrastructure;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
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

        public UndoRedoManager UndoRedo { get; } = new UndoRedoManager();
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

        public IDateTimeEditorService DateTimeEditor { get; } = new DateTimeEditorService();

        /* ---------------------------------------------------------
         * データ読み込み関連
         * --------------------------------------------------------- */
        public void LoadFromTaskEditorDataModel(TaskEditorDataModel data)
        {
            /* 既存データをクリア */
            Nodes.Nodes.Clear();
            Connections.Connections.Clear();

            /* ノードを復元 */
            foreach (var nodeData in data.Nodes)
            {
                var nodeVM = NodeViewModel.FromDataModel(nodeData, this);
                Nodes.Nodes.Add(nodeVM);
            }

            /* 接続線を復元 */
            foreach (var connData in data.Connections)
            {
                var connVM = ConnectionViewModel.FromDataModel(connData, this);
                Connections.Connections.Add(connVM);
            }

            RefreshNodeAndConnectionPositions();
        }


        /* ---------------------------------------------------------
         * データ保存関連
         * --------------------------------------------------------- */

        /* ノード情報変換 */
        private NodeDataModel ToDataModel(NodeViewModel vm)
        {
            return new NodeDataModel
            {
                Id = vm.NodeGuid.ToString(),
                Type = vm.NodeType,

                Position = new PositionDataModel
                {
                    X = vm.X,
                    Y = vm.Y
                },

                Details = new NodeDetailsDataModel
                {
                    TaskName = vm.TaskName,
                    Person = vm.Person,
                    StartDateTime = vm.StartDateTime,
                    EndDateTime = vm.EndDateTime,
                    Comment = vm.Comment
                },

                Ports = vm.AllPorts.ToList().Select(p => new PortDataModel
                {
                    Id = p.PortGuid.ToString(),
                    Name = p.Name,
                    Type = p.Type.ToString()
                }).ToList()
            };
        }

        /* 接続線情報変換 */
        private ConnectionDataModel ToDataModel(ConnectionViewModel vm)
        {
            return new ConnectionDataModel
            {
                Id = vm.ConnectionGuid.ToString(),
                FromPortId = vm.FromPort.PortGuid.ToString(),
                ToPortId = vm.ToPort.PortGuid.ToString()
            };
        }

        /* タスク依存関係情報構築 */
        public TaskEditorDataModel ToTaskEditorDataModel()
        {
            return new TaskEditorDataModel
            {
                Nodes = Nodes.Nodes
                    .Select(n => ToDataModel(n))
                    .ToList(),

                Connections = Connections.Connections
                    .Select(c => ToDataModel(c))
                    .ToList()
            };
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