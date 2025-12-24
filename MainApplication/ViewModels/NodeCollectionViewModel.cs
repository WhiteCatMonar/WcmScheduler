using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MainApplication.ViewModels.Infrastructure;
using MainApplication.ViewModels.Actions;

namespace MainApplication.ViewModels
{
    public class NodeCollectionViewModel : INotifyPropertyChanged
    {
        private readonly UndoRedoManager _undoRedo;
        private readonly NodeEditorViewModel _editor;

        public NodeCollectionViewModel(UndoRedoManager undoRedo, NodeEditorViewModel editor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            AddNodeCommand = new RelayCommand(AddNode);
            DeleteSelectedNodeCommand = new RelayCommand(DeleteSelectedNode, () => SelectedNode != null);
        }

        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();
        
        public void UpdateAllNodes()
        {
            foreach (var n in Nodes)
            {
                n.UpdateAllPortPositions();
            }
        }

        /* ---------------------------------------------------------
         * 選択管理
         * --------------------------------------------------------- */
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

        public void SelectNode(NodeViewModel node)
        {
            SelectedNode = node;
        }

        public void UnselectNode()
        {
            SelectedNode = null;
            foreach (var node in Nodes)
            {
                node.IsSelected = false;
            }
        }

        /* ---------------------------------------------------------
         * ノード追加・削除
         * --------------------------------------------------------- */
        public ICommand AddNodeCommand { get; }
        public void AddNode()
        {
            var node = CreateDefaultNode();

            var initial = ClampPosition(20, 20, node);
            node.X = initial.X;
            node.Y = initial.Y;

            var action = new AddNodeAction(Nodes, node);
            _undoRedo.Execute(action);

            SelectedNode = node;
        }

        private NodeViewModel CreateDefaultNode()
        {
            var node = new NodeViewModel(_undoRedo)
            {
                NodeGuid = Guid.NewGuid()
            };

            node.InputPorts.Add(new PortViewModel
            {
                Name = "Input",
                Type = PortViewModel.PortType.Input,
                ParentNode = node
            });

            node.OutputPorts.Add(new PortViewModel
            {
                Name = "Output",
                Type = PortViewModel.PortType.Output,
                ParentNode = node
            });

            return node;
        }

        public ICommand DeleteSelectedNodeCommand { get; }
        private void DeleteSelectedNode()
        {
            if (SelectedNode != null)
            {
                var action = new DeleteNodeAction(Nodes, SelectedNode);
                _undoRedo.Execute(action);
                SelectedNode = null;
            }
        }

        /* ---------------------------------------------------------
         * ノード移動
         * --------------------------------------------------------- */
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
                _undoRedo.Execute(action);
            }
        }

        /* ---------------------------------------------------------
         * グリッドスナップ
         * --------------------------------------------------------- */
        private double _gridSize = 1.0;
        public double GridSize
        {
            get => _gridSize;
            set
            {
                if (_gridSize != value)
                {
                    _gridSize = value; 
                    OnPropertyChanged(nameof(GridSize));
                }
            }
        }

        private double RoundToGrid(double value)
            => Math.Round(value / GridSize) * GridSize;

        /* ---------------------------------------------------------
         * Canvas 論理座標
         * --------------------------------------------------------- */
        private double _canvasViewLogicalWidth;
        public double CanvasViewLogicalWidth
        {
            get => _canvasViewLogicalWidth;
            set
            {
                if (_canvasViewLogicalWidth != value)
                {
                    _canvasViewLogicalWidth = value;
                    OnPropertyChanged(nameof(CanvasViewLogicalWidth));
                }
            }
        }

        private double _canvasViewLogicalHeight;
        public double CanvasViewLogicalHeight
        {
            get => _canvasViewLogicalHeight;
            set
            {
                if (_canvasViewLogicalHeight != value)
                {
                    _canvasViewLogicalHeight = value;
                    OnPropertyChanged(nameof(CanvasViewLogicalHeight));
                }
            }
        }

        private double _canvasViewOriginX;
        public double CanvasViewOriginX
        {
            get => _canvasViewOriginX;
            set
            {
                if (_canvasViewOriginX != value)
                {
                    _canvasViewOriginX = value;
                    OnPropertyChanged(nameof(CanvasViewOriginX));
                }
            }
        }

        private double _canvasViewOriginY;
        public double CanvasViewOriginY
        {
            get => _canvasViewOriginY;
            set
            {
                if (_canvasViewOriginY != value)
                {
                    _canvasViewOriginY = value;
                    OnPropertyChanged(nameof(CanvasViewOriginY));
                }
            }
        }

        /* ---------------------------------------------------------
         * 座標クランプ
         * --------------------------------------------------------- */
        public Point ClampPosition(double x, double y, NodeViewModel node)
        {
            double logicalStartX = CanvasViewOriginX;
            double logicalStartY = CanvasViewOriginY;

            double logicalEndX = CanvasViewOriginX + CanvasViewLogicalWidth;
            double logicalEndY = CanvasViewOriginY + CanvasViewLogicalHeight;

            double clampedX = Math.Max(logicalStartX, Math.Min(x, logicalEndX - node.Width));
            double clampedY = Math.Max(logicalStartY, Math.Min(y, logicalEndY - node.Height));

            clampedX = RoundToGrid(clampedX);
            clampedY = RoundToGrid(clampedY);

            return new Point(clampedX, clampedY);
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}