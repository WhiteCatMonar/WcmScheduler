using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// ノード一覧の管理、選択状態、追加・削除・移動操作、
    /// Undo/Redo連携を担当するViewModel。
    /// </summary>
    public class NodeCollectionViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly UndoRedoManager _undoRedo;
        private readonly IDateTimeEditorService _dateTimeEditor;
        private readonly NodeEditorViewModel _editor;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード管理ViewModelを生成する。
        /// </summary>
        public NodeCollectionViewModel(UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor, NodeEditorViewModel editor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _dateTimeEditor = dateTimeEditor ?? throw new ArgumentNullException(nameof(dateTimeEditor));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            AddNodeCommand = new RelayCommand(AddNode);
            DeleteSelectedNodeCommand = new RelayCommand(DeleteSelectedNode, () => SelectedNode != null);
        }

        /* ---------------------------------------------------------
         * ノード一覧
         * --------------------------------------------------------- */

        /// <summary>
        /// すべてのノードを保持するコレクション。
        /// </summary>
        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();

        /// <summary>
        /// すべてのノードのポート位置を更新する。
        /// </summary>
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
        
        /// <summary>
        /// 現在選択されているノード。
        /// </summary>
        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode == value)
                {
                    return;
                }

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

        /// <summary>
        /// ノードを選択する。
        /// </summary>
        public void SelectNode(NodeViewModel node)
        {
            SelectedNode = node;
        }

        /// <summary>
        /// 選択状態を解除する。
        /// </summary>
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

        /// <summary>ノード追加コマンド</summary>
        public ICommand AddNodeCommand { get; }

        /// <summary>
        /// 新しいノードを作成し、初期位置に配置して追加する。
        /// </summary>
        public void AddNode()
        {
            var node = CreateDefaultNode();

            /* 初期位置をクランプして配置 */
            var initial = _editor.Grid.ClampNodePosition(20, 20, node);
            node.X = initial.X;
            node.Y = initial.Y;

            var action = new AddNodeAction(Nodes, node);
            _undoRedo.Execute(action);

            SelectedNode = node;
        }

        /// <summary>
        /// デフォルト構成のノードを生成する。
        /// 入出力ポートを1つずつ持つ。
        /// </summary>
        private NodeViewModel CreateDefaultNode()
        {
            var node = new NodeViewModel(_undoRedo, _dateTimeEditor)
            {
                NodeGuid = Guid.NewGuid()
            };

            node.InputPorts.Add(new PortViewModel
            {
                PortGuid = Guid.NewGuid(),
                Name = "Input",
                Type = PortViewModel.PortType.Input,
                ParentNode = node
            });

            node.OutputPorts.Add(new PortViewModel
            {
                PortGuid = Guid.NewGuid(),
                Name = "Output",
                Type = PortViewModel.PortType.Output,
                ParentNode = node
            });

            return node;
        }

        /// <summary>選択中ノード削除コマンド</summary>
        public ICommand DeleteSelectedNodeCommand { get; }

        /// <summary>
        /// 選択中のノードを削除する。
        /// </summary>
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

        /// <summary>
        /// ノードの位置を更新する(Undo/Redoなし)。
        /// </summary>
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

        /// <summary>
        /// ノードを移動し、Undo/Redo対応のアクションとして記録する。
        /// </summary>
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

        /// <summary>
        /// プロパティ変更通知を発行する。
        /// </summary>
        private void OnPropertyChanged(string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
