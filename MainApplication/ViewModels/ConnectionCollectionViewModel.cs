using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class ConnectionCollectionViewModel : INotifyPropertyChanged
    {
        private readonly UndoRedoManager _undoRedo;
        private readonly NodeEditorViewModel _editor;

        public ConnectionCollectionViewModel(UndoRedoManager undoRedo, NodeEditorViewModel editor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            SelectConnectionCommand = new RelayCommand<ConnectionViewModel>(SelectConnection);
            DeleteSelectedConnectionCommand = new RelayCommand(DeleteSelectedConnection, () => SelectedConnection != null);
        }

        /* 接続線一覧 */
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        public void UpdateConnectionsForNode(NodeViewModel node)
        {
            foreach (var port in node.AllPorts)
            {
                foreach (var conn in port.ConnectedConnections)
                {
                    conn.UpdatePathGeometry();
                }
            }
        }

        public void UpdateAllConnections()
        {
            foreach (var c in Connections)
            {
                c.UpdatePathGeometry();
            }
        }

        /* ドラッグ中の接続開始ポート */
        private PortViewModel _draggingFromPort;
        public PortViewModel DraggingFromPort
        {
            get => _draggingFromPort;
            set
            {
                if (_draggingFromPort != value)
                {
                    _draggingFromPort = value;
                    OnPropertyChanged(nameof(DraggingFromPoint));
                    OnPropertyChanged(nameof(DraggingFromPort));
                    OnPropertyChanged(nameof(IsDraggingConnection));
                    OnPropertyChanged(nameof(DraggingFromPortBezierControlPoint));
                }
            }
        }

        /* 始点(論理座標) */
        public Point DraggingFromPoint =>
            DraggingFromPort == null ? new Point(0, 0) : DraggingFromPort.AbsolutePosition;

        /* 始点側のベジェ制御点(論理座標) */
        public Point DraggingFromPortBezierControlPoint =>
            DraggingFromPort == null
                ? new Point(0, 0)
                : new Point(DraggingFromPort.AbsolutePosition.X + 50, DraggingFromPort.AbsolutePosition.Y);

        /* ドラッグ中の終点(論理座標) */
        private Point _draggingToPoint;
        public Point DraggingToPoint
        {
            get => _draggingToPoint;
            set
            {
                var clamped = _editor.Grid.ClampPoint(value);
                if (_draggingToPoint != clamped)
                {
                    _draggingToPoint = clamped;
                    OnPropertyChanged(nameof(DraggingToPoint));
                    OnPropertyChanged(nameof(DraggingToPointBezierControl));
                }
            }
        }

        /* 終点側のベジェ制御点(論理座標) */
        public Point DraggingToPointBezierControl =>
            new Point(DraggingToPoint.X - 50, DraggingToPoint.Y);

        public bool IsDraggingConnection => _draggingFromPort != null;

        /* 接続線作成 */
        public void CreateConnection(PortViewModel fromPort, PortViewModel toPort)
        {
            if (fromPort == null || toPort == null)
            {
                return;
            }

            if (fromPort == toPort)
            {
                return; /* 自己接続禁止 */
            }

            if (fromPort.ParentNode == toPort.ParentNode)
            {
                return; /* 同一ノード内禁止 */
            }

            var connection = new ConnectionViewModel(fromPort, toPort, _editor);

            fromPort.ConnectedConnections.Add(connection);
            toPort.ConnectedConnections.Add(connection);

            var action = new AddConnectionAction(Connections, connection);
            _undoRedo.Execute(action);
        }

        /* 選択管理 */
        private ConnectionViewModel _selectedConnection;
        public ConnectionViewModel SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                if (_selectedConnection != value)
                {
                    if (_selectedConnection != null)
                    {
                        _selectedConnection.IsSelected = false;
                    }
                    _selectedConnection = value;
                    if (_selectedConnection != null)
                    {
                        _selectedConnection.IsSelected = true;
                    }

                    OnPropertyChanged(nameof(SelectedConnection));
                }
            }
        }

        public ICommand SelectConnectionCommand { get; }
        private void SelectConnection(ConnectionViewModel connection)
        {
            SelectedConnection = connection;
        }

        public void UnselectConnection()
        {
            SelectedConnection = null;
            foreach (var conn in Connections)
            {
                conn.IsSelected = false;
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}