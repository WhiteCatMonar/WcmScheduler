using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class NodeEditorViewModel : INotifyPropertyChanged
    {
        public UndoRedoManager UndoRedo { get; } = new UndoRedoManager();

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand MoveToHistoryCommand { get; }
        public ConnectionCollectionViewModel Connections { get; }

        public NodeEditorViewModel()
        {
            Connections = new ConnectionCollectionViewModel(UndoRedo, this);

            AddNodeCommand = new RelayCommand(AddNode);
            DeleteSelectedNodeCommand = new RelayCommand(DeleteSelectedNode, () => SelectedNode != null);

            UndoCommand = new RelayCommand(() =>
            {
                UndoRedo.Undo();

                /* Undo後に整合性を取る */
                RefreshNodeAndConnectionPositions();

            }, () => UndoRedo.CanUndo);

            RedoCommand = new RelayCommand(() =>
            {
                UndoRedo.Redo();

                /* Redo後に整合性を取る */
                RefreshNodeAndConnectionPositions();

            }, () => UndoRedo.CanRedo);
            MoveToHistoryCommand = new RelayCommand<UndoRedoManager.HistoryItem>(item => UndoRedo.MoveToHistory(item));

            UndoRedo.CurrentHistoryChanged += OnCurrentHistoryChanged;
        }

        private void RefreshNodeAndConnectionPositions()
        {
            /* 全ノードのポート座標を更新 */
            foreach (var node in Nodes)
            {
                node.UpdateAllPortPositions();
            }

            /* 全接続線のジオメトリを更新 */
            Connections.UpdateAllConnections();
        }

        private void OnCurrentHistoryChanged(object sender, UndoRedoManager.HistoryItem e)
        {
            CurrentHistoryChanged?.Invoke(this, e);
            RefreshNodeAndConnectionPositions();
        }
        public event EventHandler<UndoRedoManager.HistoryItem> CurrentHistoryChanged;

        /* ノード一覧 */
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        private NodeViewModel _selectedNode;
        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode != value)
                {
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = false;
                    }
                    _selectedNode = value;
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = true;
                    }
                    OnPropertyChanged(nameof(SelectedNode));
                }
            }
        }

        public void SelectNode(NodeViewModel node) => SelectedNode = node;

        public void UnselectNode()
        {
            SelectedNode = null;
            foreach (var node in Nodes)
            {
                node.IsSelected = false;
            }
        }

        public ICommand AddNodeCommand { get; }
        private void AddNode()
        {
            var node = new NodeViewModel(UndoRedo)
            {
                NodeGuid = Guid.NewGuid()
            };

            var initial = ClampPosition(20, 20, node);
            node.X = initial.X;
            node.Y = initial.Y;

            node.InputPorts.Add(new PortViewModel {
                Name = "Input",
                Type = PortViewModel.PortType.Input,
                ParentNode = node
            });
            node.OutputPorts.Add(new PortViewModel {
                Name = "Output",
                Type = PortViewModel.PortType.Output,
                ParentNode = node
            });

            var action = new AddNodeAction(Nodes, node);
            UndoRedo.Execute(action);

            SelectedNode = node;
        }

        public ICommand DeleteSelectedNodeCommand { get; }
        private void DeleteSelectedNode()
        {
            if (SelectedNode != null)
            {
                var action = new DeleteNodeAction(Nodes, SelectedNode);
                UndoRedo.Execute(action);
                SelectedNode = null;
            }
        }

        public void UpdateNodePosition(NodeViewModel node, double newX, double newY)
        {
            if (node == null)
            {
                return;
            }

            var clamped = ClampPosition(newX, newY, node);
            node.X = clamped.X;
            node.Y = clamped.Y;
        }

        public void MoveNode(NodeViewModel node, double oldX, double oldY, double newX, double newY)
        {
            if (node == null)
            {
                return;
            }

            var clamped = ClampPosition(newX, newY, node);
            if ((oldX != clamped.X) || (oldY != clamped.Y))
            {
                var action = new MoveNodeAction(node, oldX, oldY, clamped.X, clamped.Y);
                UndoRedo.Execute(action);
            }
        }

        /* 座標関係処理 */

        /* ズーム・パン状態（View から更新され、接続線などが利用する） */
        private double _zoom = 1.0;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (Math.Abs(_zoom - value) > double.Epsilon)
                {
                    _zoom = value;
                    OnPropertyChanged(nameof(Zoom));
                }
            }
        }

        private double _panX = 0.0;
        public double PanX
        {
            get => _panX;
            set
            {
                if (Math.Abs(_panX - value) > double.Epsilon)
                {
                    _panX = value;
                    OnPropertyChanged(nameof(PanX));
                }
            }
        }

        private double _panY = 0.0;
        public double PanY
        {
            get => _panY;
            set
            {
                if (Math.Abs(_panY - value) > double.Epsilon)
                {
                    _panY = value;
                    OnPropertyChanged(nameof(PanY));
                }
            }
        }

        /* NodeEditorArea(表示領域) */
        private double _baseCanvasWidth = 200;
        public double BaseCanvasWidth
        {
            get => _baseCanvasWidth;
            set { _baseCanvasWidth = value; OnPropertyChanged(nameof(BaseCanvasWidth)); }
        }

        private double _baseCanvasHeight = 200;
        public double BaseCanvasHeight
        {
            get => _baseCanvasHeight;
            set { _baseCanvasHeight = value; OnPropertyChanged(nameof(BaseCanvasHeight)); }
        }

        /* NodeEditerCanvas(ズーム後の論理Canvas) */
        private double _zoomedCanvasWidth = 200;
        public double ZoomedCanvasWidth
        {
            get => _zoomedCanvasWidth;
            set {
                _zoomedCanvasWidth = value;
                Connections.CanvasViewLogicalWidth = value;
                OnPropertyChanged(nameof(ZoomedCanvasWidth));
                OnPropertyChanged(nameof(ZoomedCanvasAreaEndX));
                OnPropertyChanged(nameof(Connections.CanvasViewLogicalWidth));
            }
        }

        private double _zoomedCanvasHeight = 200;
        public double ZoomedCanvasHeight
        {
            get => _zoomedCanvasHeight;
            set {
                _zoomedCanvasHeight = value;
                Connections.CanvasViewLogicalHeight = value;
                OnPropertyChanged(nameof(ZoomedCanvasHeight));
                OnPropertyChanged(nameof(ZoomedCanvasAreaEndY));
                OnPropertyChanged(nameof(Connections.CanvasViewLogicalHeight));
            }
        }

        private const double GridSize = 1;

        private double RoundToGrid(double value)
            => Math.Round(value / GridSize) * GridSize;

        public Point ClampPosition(double x, double y, NodeViewModel node)
        {
            double clampedX = Math.Max(ZoomedCanvasAreaStartX, Math.Min(x, ZoomedCanvasAreaEndX - node.Width));
            double clampedY = Math.Max(ZoomedCanvasAreaStartY, Math.Min(y, ZoomedCanvasAreaEndY - node.Height));

            clampedX = RoundToGrid(clampedX);
            clampedY = RoundToGrid(clampedY);

            return new Point(clampedX, clampedY);
        }

        /* グリッド関連 */

        private double _gridSpacing = 20;
        public double GridSpacing
        {
            get => _gridSpacing;
            set
            {
                if (_gridSpacing != value)
                {
                    _gridSpacing = value;
                    OnPropertyChanged(nameof(GridSpacing));

                    UpdateGrid(_gridOriginX, _gridOriginY, ZoomedCanvasWidth, ZoomedCanvasHeight, _gridSpacing);
                }
            }
        }

        public ObservableCollection<LineViewModel> GridLines { get; } = new ObservableCollection<LineViewModel>();

        private double _gridOriginX = 0;
        private double _gridOriginY = 0;

        private double _canvasViewOriginX = 0;
        private double _canvasViewOriginY = 0;

        /* 表示中の四隅の座標 */
        public double ZoomedCanvasAreaStartX => _canvasViewOriginX;
        public double ZoomedCanvasAreaStartY => _canvasViewOriginY;
        public double ZoomedCanvasAreaEndX => _canvasViewOriginX + ZoomedCanvasWidth;
        public double ZoomedCanvasAreaEndY => _canvasViewOriginY + ZoomedCanvasHeight;

        public void UpdateGrid(double originX, double originY, double width, double height, double spacing)
        {
            GridLines.Clear();

            /* キャンバスの原点位置に対応する論理座標を記録 */
            _canvasViewOriginX = -originX;
            _canvasViewOriginY = -originY;
            Connections.CanvasViewOriginX = _canvasViewOriginX;
            Connections.CanvasViewOriginY = _canvasViewOriginY;

            /* パンによる原点移動を考慮(Floor で揺れ防止) */
            _gridOriginX = Math.Floor(_canvasViewOriginX / spacing) * spacing;
            _gridOriginY = Math.Floor(_canvasViewOriginY / spacing) * spacing;

            /* 描画範囲(origin から width+origin まで) */
            double endX = _gridOriginX + width + spacing;
            double endY = _gridOriginY + height + spacing;
            
            double actualSpacing = spacing * Zoom;
            bool isShowSubGrid = actualSpacing >= 8; /* ズーム後のピクセル間隔が8px以上なら補助線表示 */

            /* 垂直線 */
            for (double x = _gridOriginX; x < endX; x += spacing)
            {
                int gridIndex = (int)Math.Round(x / spacing);
                bool isMajor = (gridIndex % 10 == 0);


                if (!isMajor && !isShowSubGrid)
                {
                    continue;
                }

                GridLines.Add(new LineViewModel
                {
                    X1 = x,
                    Y1 = _gridOriginY,
                    X2 = x,
                    Y2 = endY,
                    IsMajor = isMajor
                });
            }

            /* 水平線 */
            for (double y = _gridOriginY; y < endY; y += spacing)
            {
                int gridIndex = (int)Math.Round(y / spacing);
                bool isMajor = (gridIndex % 10 == 0);

                if (!isMajor && !isShowSubGrid)
                {
                    continue;
                }

                GridLines.Add(new LineViewModel
                {
                    X1 = _gridOriginX,
                    Y1 = y,
                    X2 = endX,
                    Y2 = y,
                    IsMajor = isMajor
                });
            }

            /* 表示中座標更新 */
            OnPropertyChanged(nameof(ZoomedCanvasAreaStartX));
            OnPropertyChanged(nameof(ZoomedCanvasAreaStartY));
            OnPropertyChanged(nameof(ZoomedCanvasAreaEndX));
            OnPropertyChanged(nameof(ZoomedCanvasAreaEndY));
            OnPropertyChanged(nameof(Connections.CanvasViewOriginX));
            OnPropertyChanged(nameof(Connections.CanvasViewOriginY));
        }

        /* イベント関係処理 */
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}