using MainApplication.Models.SaveData;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using static MainApplication.ViewModels.PortViewModel;

namespace MainApplication.ViewModels
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        private readonly NodeEditorViewModel _editor;

        private Guid _connectionGuid;
        public Guid ConnectionGuid
        {
            get => _connectionGuid;
            set
            {
                if (_connectionGuid == value)
                {
                    return;
                }
                _connectionGuid = value;
                OnPropertyChanged(nameof(ConnectionGuid));
            }
        }

        public ConnectionViewModel(PortViewModel from, PortViewModel to, NodeEditorViewModel editor)
        {
            ConnectionGuid = Guid.NewGuid();
            FromPort = from;
            ToPort = to;
            _editor = editor;

            /* 初期値を設定 */
            FromPosition = FromPort?.AbsolutePosition ?? new Point(0, 0);
            ToPosition = ToPort?.AbsolutePosition ?? new Point(0, 0);

            /* 座標更新を監視 */
            FromPort.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PortViewModel.AbsolutePosition))
                {
                    FromPosition = FromPort.AbsolutePosition;
                }
            };

            ToPort.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PortViewModel.AbsolutePosition))
                {
                    ToPosition = ToPort.AbsolutePosition;
                }
            };
        }

        private Geometry _connectionGeometry;
        public Geometry ConnectionGeometry
        {
            get => _connectionGeometry;
            private set
            {
                if (_connectionGeometry != value)
                {
                    _connectionGeometry = value;
                    OnPropertyChanged(nameof(ConnectionGeometry));
                }
            }
        }

        public void UpdatePathGeometry()
        {
            var start = FromPosition;
            var end = ToPosition;

            double dx = Math.Abs(end.X - start.X) * 0.5;

            var p1 = new Point(start.X + dx, start.Y);
            var p2 = new Point(end.X - dx, end.Y);

            var figure = new PathFigure
            {
                StartPoint = start,
                Segments = new PathSegmentCollection {
                    new BezierSegment(p1, p2, end, true)
                },
                IsClosed = false
            };

            ConnectionGeometry = new PathGeometry(new[] { figure });
        }


        public PortViewModel FromPort { get; set; }
        public PortViewModel ToPort { get; set; }

        private Point _fromPosition = new Point(0, 0);
        public Point FromPosition
        {
            get => _fromPosition;
            private set
            {
                if (_fromPosition != value)
                {
                    _fromPosition = value;
                    OnPropertyChanged(nameof(FromPosition));
                    OnPropertyChanged(nameof(FromBezierControlPoint));
                    UpdatePathGeometry();
                }
            }
        }

        private Point _toPosition = new Point(0, 0);
        public Point ToPosition
        {
            get => _toPosition;
            private set
            {
                if (_toPosition != value)
                {
                    _toPosition = value;
                    OnPropertyChanged(nameof(ToPosition));
                    OnPropertyChanged(nameof(ToBezierControlPoint));
                    UpdatePathGeometry();
                }
            }
        }

        public Point FromBezierControlPoint => new Point(FromPosition.X + 50, FromPosition.Y);
        public Point ToBezierControlPoint => new Point(ToPosition.X - 50, ToPosition.Y);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public static ConnectionViewModel FromDataModel(ConnectionDataModel data, NodeEditorViewModel editor)
        {
            PortViewModel from = null;
            PortViewModel to = null;

            foreach (NodeViewModel node in editor.Nodes.Nodes)
            {
                foreach (PortViewModel port in node.AllPorts)
                {
                    if (port.PortGuid.ToString() == data.FromPortId)
                    {
                        from = port;
                    }
                    if (port.PortGuid.ToString() == data.ToPortId)
                    {
                        to = port;
                    }
                }
            }
            if ((from == null) || (to == null))
            {
                return null;
            }
            ConnectionViewModel loadedConnection = new ConnectionViewModel(from, to, editor);
            loadedConnection.ConnectionGuid = Guid.Parse(data.Id);
            return loadedConnection;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
