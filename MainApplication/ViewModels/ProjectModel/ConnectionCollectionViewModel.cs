using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// 接続線(ConnectionViewModel)の管理、選択、ドラッグ操作、
    /// Undo/Redo連携を担当するViewModel。
    /// </summary>
    public class ConnectionCollectionViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly UndoRedoManager _undoRedo;
        private readonly NodeEditorViewModel _editor;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線管理ViewModelを生成する。
        /// </summary>
        public ConnectionCollectionViewModel(UndoRedoManager undoRedo, NodeEditorViewModel editor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            SelectConnectionCommand = new RelayCommand<ConnectionViewModel>(SelectConnection);
            DeleteSelectedConnectionCommand = new RelayCommand(DeleteSelectedConnection, () => SelectedConnection != null);
        }

        /* ---------------------------------------------------------
         * 接続線一覧
         * --------------------------------------------------------- */

        /// <summary>
        /// すべての接続線を保持するコレクション。
        /// </summary>
        public ObservableCollection<ConnectionViewModel> Connections { get; } = [];

        /// <summary>
        /// 指定ノードに関連する接続線のジオメトリを更新する。
        /// </summary>
        public static void UpdateConnectionsForNode(NodeViewModel node)
        {
            foreach (var port in node.AllPorts)
            {
                foreach (var conn in port.ConnectedConnections)
                {
                    conn.UpdatePathGeometry();
                }
            }
        }

        /// <summary>
        /// すべての接続線のジオメトリを更新する。
        /// </summary>
        public void UpdateAllConnections()
        {
            foreach (var c in Connections)
            {
                c.UpdatePathGeometry();
            }
        }

        /* ---------------------------------------------------------
         * ドラッグ中の接続線(始点)
         * --------------------------------------------------------- */

        private PortViewModel? _draggingFromPort;

        /// <summary>
        /// 接続線ドラッグ開始ポート(nullならドラッグ中ではない)
        /// </summary>
        public PortViewModel? DraggingFromPort
        {
            get => _draggingFromPort;
            set => SetProperty(
                ref _draggingFromPort,
                value,
                [
                    nameof(DraggingFromPoint),
                    nameof(IsDraggingConnection),
                    nameof(DraggingFromPortBezierControlPoint)
                ]
            );
        }

        /// <summary>
        /// ドラッグ開始位置(論理座標)
        /// </summary>
        public Point DraggingFromPoint =>
            DraggingFromPort == null ? new Point(0, 0) : DraggingFromPort.AbsolutePosition;

        /// <summary>
        /// 始点側のベジェ制御点(論理座標)
        /// </summary>
        public Point DraggingFromPortBezierControlPoint =>
            DraggingFromPort == null
                ? new Point(0, 0)
                : new Point(DraggingFromPort.AbsolutePosition.X + 50,
                            DraggingFromPort.AbsolutePosition.Y);

        /* ---------------------------------------------------------
         * ドラッグ中の接続線(終点)
         * --------------------------------------------------------- */

        private Point _draggingToPoint;

        /// <summary>
        /// ドラッグ中の終点(論理座標)
        /// </summary>
        public Point DraggingToPoint
        {
            get => _draggingToPoint;
            set => SetProperty(
                ref _draggingToPoint,
                _editor.Grid.ClampPoint(value),
                [
                    nameof(DraggingToPoint),
                    nameof(DraggingToPointBezierControl)
                ]
            );
        }

        /// <summary>
        /// 終点側のベジェ制御点(論理座標)
        /// </summary>
        public Point DraggingToPointBezierControl => new(DraggingToPoint.X - 50, DraggingToPoint.Y);
        /// <summary>
        /// 現在接続線をドラッグ中かどうか
        /// </summary>
        public bool IsDraggingConnection => _draggingFromPort != null;

        /* ---------------------------------------------------------
         * 接続線作成
         * --------------------------------------------------------- */

        /// <summary>
        /// 2つのポート間に接続線を作成する。
        /// </summary>
        public void CreateConnection(PortViewModel fromPort, PortViewModel toPort)
        {
            var connection = new ConnectionViewModel(fromPort, toPort)
            {
                FromPort = fromPort,
                ToPort = toPort
            };

            fromPort.ConnectedConnections.Add(connection);
            toPort.ConnectedConnections.Add(connection);

            var action = new AddConnectionAction(Connections, connection);
            _undoRedo.Execute(action);
        }

        /* ---------------------------------------------------------
         * 選択管理
         * --------------------------------------------------------- */

        private ConnectionViewModel? _selectedConnection;

        /// <summary>
        /// 現在選択されている接続線
        /// </summary>
        public ConnectionViewModel? SelectedConnection
        {
            get => _selectedConnection;
            set => SetProperty(
                ref _selectedConnection,
                value,
                CreateHooksFromValue(
                    value,
                    pre: (oldValue, newValue) => oldValue?.IsSelected = false,
                    post: (oldValue, newValue) => newValue?.IsSelected = true
                )
            );
        }

        /// <summary>
        /// 接続線を選択するコマンド
        /// </summary>
        public ICommand SelectConnectionCommand { get; }

        private void SelectConnection(ConnectionViewModel? connection)
        {
            SelectedConnection = connection;
        }

        /// <summary>
        /// 選択状態を解除する
        /// </summary>
        public void UnselectConnection()
        {
            SelectedConnection = null;
            foreach (var conn in Connections)
            {
                conn.IsSelected = false;
            }
        }

        /// <summary>
        /// 選択中の接続線を削除するコマンド
        /// </summary>
        public ICommand DeleteSelectedConnectionCommand { get; }

        private void DeleteSelectedConnection()
        {
            if (SelectedConnection != null)
            {
                var action = new DeleteConnectionAction(Connections, SelectedConnection);
                _undoRedo.Execute(action);
                SelectedConnection = null;
            }
        }
    }
}

/* --- End of file --- */
