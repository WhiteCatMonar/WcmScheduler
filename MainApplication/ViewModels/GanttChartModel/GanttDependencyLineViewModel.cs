using MainApplication.ViewModels.Core;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート上に表示する依存関係線のViewModel
    /// </summary>
    public class GanttDependencyLineViewModel : ViewModelBase
    {
        /// <summary>
        /// 依存関係線を生成する
        /// </summary>
        /// <param name="fromTaskName">接続元タスク名</param>
        /// <param name="toTaskName">接続先タスク名</param>
        /// <param name="startX">開始X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="endX">終了X座標</param>
        /// <param name="endY">終了Y座標</param>
        public GanttDependencyLineViewModel(
            string fromTaskName,
            string toTaskName,
            double startX,
            double startY,
            double endX,
            double endY
        )
        {
            FromTaskName = fromTaskName;
            ToTaskName = toTaskName;
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            Geometry = CreateGeometry(startX, startY, endX, endY);
        }

        /// <summary>
        /// 接続元タスク名
        /// </summary>
        public string FromTaskName { get; }

        /// <summary>
        /// 接続先タスク名
        /// </summary>
        public string ToTaskName { get; }

        /// <summary>
        /// 開始X座標
        /// </summary>
        public double StartX { get; }

        /// <summary>
        /// 開始Y座標
        /// </summary>
        public double StartY { get; }

        /// <summary>
        /// 終了X座標
        /// </summary>
        public double EndX { get; }

        /// <summary>
        /// 終了Y座標
        /// </summary>
        public double EndY { get; }

        /// <summary>
        /// 描画用ジオメトリ
        /// </summary>
        public Geometry Geometry { get; }

        /// <summary>
        /// 矢印の先端点一覧
        /// </summary>
        public PointCollection ArrowPoints =>
        [
            new Point(EndX, EndY),
            new Point(EndX - 8.0, EndY - 4.0),
            new Point(EndX - 8.0, EndY + 4.0)
        ];

        /// <summary>
        /// 依存関係表示文字列
        /// </summary>
        public string DisplayText => $"{FromTaskName} -> {ToTaskName}";

        /// <summary>
        /// 依存関係線のジオメトリを生成する
        /// </summary>
        /// <param name="startX">開始X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="endX">終了X座標</param>
        /// <param name="endY">終了Y座標</param>
        /// <returns>描画用ジオメトリ</returns>
        private static Geometry CreateGeometry(double startX, double startY, double endX, double endY)
        {
            var horizontalGap = Math.Max(24.0, Math.Abs(endX - startX) * 0.45);
            var firstControlPoint = new Point(startX + horizontalGap, startY);
            var secondControlPoint = new Point(endX - horizontalGap, endY);
            var figure = new PathFigure
            {
                StartPoint = new Point(startX, startY),
                Segments =
                [
                    new BezierSegment(firstControlPoint, secondControlPoint, new Point(endX, endY), true)
                ],
                IsClosed = false
            };

            return new PathGeometry([figure]);
        }
    }
}

/* --- End of file --- */
