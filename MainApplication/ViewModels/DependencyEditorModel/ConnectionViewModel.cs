using MainApplication.ViewModels.Core;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels.DependencyEditorModel
{
    /// <summary>
    /// 2つのポート間を結ぶ接続線を表すViewModel。
    /// ポート座標の変化に追従し、ベジェ曲線のジオメトリを更新する。
    /// </summary>

    public class ConnectionViewModel : ViewModelBase
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
            set => SetProperty(ref _connectionGuid, value);
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

            /* 初期座標を設定 */
            FromPosition = from.AbsolutePosition;
            ToPosition = to.AbsolutePosition;

            /* ポート座標の更新を監視 */
            from.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PortViewModel.AbsolutePosition))
                {
                    FromPosition = from.AbsolutePosition;
                }
            };

            to.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PortViewModel.AbsolutePosition))
                {
                    ToPosition = to.AbsolutePosition;
                }
            };
        }

        /* ---------------------------------------------------------
         * 接続線ジオメトリ
         * --------------------------------------------------------- */

        private Geometry? _connectionGeometry;

        /// <summary>
        /// 描画用のベジェ曲線ジオメトリ
        /// </summary>
        public Geometry? ConnectionGeometry
        {
            get => _connectionGeometry;
            private set => SetProperty(ref _connectionGeometry, value);
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
                Segments =
                [
                    new BezierSegment(p1, p2, end, true)
                ],
                IsClosed = false
            };

            ConnectionGeometry = new PathGeometry([ figure ]);
        }

        /* ---------------------------------------------------------
         * ポートと座標
         * --------------------------------------------------------- */

        /// <summary>接続元ポート</summary>
        public required PortViewModel FromPort { get; init; }

        /// <summary>接続先ポート</summary>
        public required PortViewModel ToPort { get; init; }

        private Point _fromPosition = new(0, 0);

        /// <summary>
        /// 接続元ポートの現在位置(論理座標)
        /// </summary>
        public Point FromPosition
        {
            get => _fromPosition;
            private set => SetProperty(
                ref _fromPosition,
                value,
                [
                    nameof(FromBezierControlPoint)
                ],
                CreateHooksFromValue(
                    value,
                    chain: () => UpdatePathGeometry()
                )
            );
        }

        private Point _toPosition = new(0, 0);

        /// <summary>
        /// 接続先ポートの現在位置(論理座標)
        /// </summary>
        public Point ToPosition
        {
            get => _toPosition;
            private set => SetProperty(
                ref _toPosition,
                value,
                [
                    nameof(ToBezierControlPoint)
                ],
                CreateHooksFromValue(
                    value,
                    chain: () => UpdatePathGeometry()
                )
            );
        }

        /// <summary>
        /// 接続元側のベジェ制御点
        /// </summary>
        public Point FromBezierControlPoint => new(FromPosition.X + 50, FromPosition.Y);
        
        /// <summary>
        /// 接続先側のベジェ制御点
        /// </summary>
        public Point ToBezierControlPoint => new(ToPosition.X - 50, ToPosition.Y);

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
            set => SetProperty(ref _isSelected, value);
        }
    }
}

/* --- End of file --- */
