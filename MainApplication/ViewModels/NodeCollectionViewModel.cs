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

            var initial = _editor.Grid.ClampNodePosition(20, 20, node);
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

            var clamped = _editor.Grid.ClampNodePosition(newX, newY, node);
            node.X = clamped.X;
            node.Y = clamped.Y;
        }

        public void MoveNode(NodeViewModel node, double oldX, double oldY, double newX, double newY)
        {
            if (node == null)
            {
                return;
            }

            var clamped = _editor.Grid.ClampNodePosition(newX, newY, node);
            if ((oldX != clamped.X) || (oldY != clamped.Y))
            {
                var action = new MoveNodeAction(node, oldX, oldY, clamped.X, clamped.Y);
                _undoRedo.Execute(action);
            }
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}