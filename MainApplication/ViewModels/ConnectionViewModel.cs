using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// 2つのポート間を結ぶ接続線を表すViewModel。
    /// ポート座標の変化に追従し、ベジェ曲線のジオメトリを更新する。
    /// </summary>

    public class ConnectionViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * 接続線の識別子
         * --------------------------------------------------------- */

        private Guid _connectionGuid;

        /// <summary>
        /// 接続線を一意に識別するGUID
        /// </summary>
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

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線ViewModelを生成する。
        /// ポート座標の変化を監視し、線を自動更新する。
        /// </summary>
        public ConnectionViewModel(PortViewModel from, PortViewModel to)
        {
            ConnectionGuid = Guid.NewGuid();
            FromPort = from;
            ToPort = to;

            /* 初期座標を設定 */
            FromPosition = FromPort?.AbsolutePosition ?? new Point(0, 0);
            ToPosition = ToPort?.AbsolutePosition ?? new Point(0, 0);

            /* ポート座標の更新を監視 */
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

        /* ---------------------------------------------------------
         * 接続線ジオメトリ
         * --------------------------------------------------------- */

        private Geometry _connectionGeometry;

        /// <summary>
        /// 描画用のベジェ曲線ジオメトリ
        /// </summary>
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

        /// <summary>
        /// ベジェ曲線のジオメトリを再計算する。
        /// </summary>
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

        /* ---------------------------------------------------------
         * ポートと座標
         * --------------------------------------------------------- */

        /// <summary>接続元ポート</summary>
        public PortViewModel FromPort { get; set; }

        /// <summary>接続先ポート</summary>
        public PortViewModel ToPort { get; set; }

        private Point _fromPosition = new Point(0, 0);

        /// <summary>
        /// 接続元ポートの現在位置(論理座標)
        /// </summary>
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

        /// <summary>
        /// 接続先ポートの現在位置(論理座標)
        /// </summary>
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

        /// <summary>
        /// 接続元側のベジェ制御点
        /// </summary>
        public Point FromBezierControlPoint => new Point(FromPosition.X + 50, FromPosition.Y);
        
        /// <summary>
        /// 接続先側のベジェ制御点
        /// </summary>
        public Point ToBezierControlPoint => new Point(ToPosition.X - 50, ToPosition.Y);

        /* ---------------------------------------------------------
         * 選択状態
         * --------------------------------------------------------- */

        private bool _isSelected;

        /// <summary>
        /// 接続線が選択されているかどうか
        /// </summary>
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

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/* --- End of file --- */
