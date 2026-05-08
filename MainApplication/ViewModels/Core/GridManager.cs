using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// キャンバスのズーム・パン・表示領域・グリッド生成など、
    /// エディタ全体の座標系管理を行うクラス。
    /// </summary>
    public class GridManager : ViewModelBase
    {
        /* ---------------------------------------------------------
         * ズーム・パン状態
         * --------------------------------------------------------- */

        private double _zoom = 1.0;

        /// <summary>
        /// キャンバスのズーム倍率
        /// </summary>
        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        /// <summary>
        /// キャンバスのパン位置
        /// </summary>
        private Point _pan;

        public Point Pan
        {
            get => _pan;
            set => SetProperty(
                ref _pan,
                value
            );
        }

        /* ---------------------------------------------------------
         * 座標変換
         * --------------------------------------------------------- */

        /// <summary>
        /// 画面座標を論理座標に変換する。
        /// ズーム倍率とパン位置を考慮して変換を行う。
        /// </summary>
        /// <param name="screen">画面座標</param>
        /// <returns>論理座標</returns>
        public Point ScreenToLogical(Point screen)
        {
            return screen.Sub(Pan).Div(Zoom);
        }

        /// <summary>
        /// 論理座標を画面座標に変換する。
        /// ズーム倍率とパン位置を考慮して変換を行う。
        /// </summary>
        /// <param name="logical">論理座標</param>
        /// <returns>画面座標</returns>
        public Point LogicalToScreen(Point logical)
        {
            return logical.Mul(Zoom).Add(Pan);
        }

        /// <summary>
        /// 画面上の移動量を論理座標系の移動量に変換する。
        /// </summary>
        /// <param name="screenDelta">画面座標での移動量</param>
        /// <returns>論理座標での移動量</returns>
        public Point ScreenDeltaToLogical(Point screenDelta)
        {
            return screenDelta.Div(Zoom);
        }


        /* ---------------------------------------------------------
         * キャンバスの論理サイズ(ズーム後のサイズ)
         * --------------------------------------------------------- */

        private double _canvasViewLogicalWidth;

        /// <summary>
        /// 表示領域の論理幅(ズーム後)
        /// </summary>
        public double CanvasViewLogicalWidth
        {
            get => _canvasViewLogicalWidth;
            set => SetProperty(ref _canvasViewLogicalWidth, value, [nameof(CanvasViewAreaEnd)]);
        }

        private double _canvasViewLogicalHeight;

        /// <summary>
        /// 表示領域の論理高さ(ズーム後)
        /// </summary>
        public double CanvasViewLogicalHeight
        {
            get => _canvasViewLogicalHeight;
            set => SetProperty(ref _canvasViewLogicalHeight, value, [nameof(CanvasViewAreaEnd)]);
        }

        /* ---------------------------------------------------------
         * キャンバスの表示原点(論理座標)
         * --------------------------------------------------------- */

        private Point _canvasViewOrigin;

        /// <summary>
        /// 表示領域の左上座標(論理座標)
        /// </summary>
        public Point CanvasViewOrigin
        {
            get => _canvasViewOrigin;
            set => SetProperty(
                ref _canvasViewOrigin,
                value,
                [
                    nameof(CanvasViewAreaStart),
                    nameof(CanvasViewAreaEnd)
                ]
            );
        }

        /* ---------------------------------------------------------
         * キャンバスの表示領域(論理座標)
         * --------------------------------------------------------- */

        /// <summary>表示領域の開始座標</summary>
        public Point CanvasViewAreaStart => CanvasViewOrigin;

        /// <summary>表示領域の終了X座標</summary>
        public Point CanvasViewAreaEnd => CanvasViewOrigin.Add(CanvasViewLogicalWidth, CanvasViewLogicalHeight);

        /* ---------------------------------------------------------
         * グリッドスナップ
         * --------------------------------------------------------- */

        private double _gridSize = 1.0;

        /// <summary>
        /// グリッドの基本サイズ(スナップ単位)
        /// </summary>
        public double GridSize
        {
            get => _gridSize;
            set => SetProperty(
                ref _gridSize,
                value,
                [
                    nameof(GridSpacing)
                ],
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateGrid()
                )
            );
        }

        /// <summary>
        /// 値をグリッド単位に丸める
        /// </summary>
        public double RoundToGrid(double value)
            => Math.Round(value / GridSize) * GridSize;

        /* ---------------------------------------------------------
         * 座標クランプ(ノード用)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの位置を表示領域内に収め、かつグリッドにスナップさせる
        /// </summary>
        public Point ClampNodePosition(Point Position, TaskNodeViewModel node)
        {
            /* ノード配置可能な位置はノードサイズの影響を受けるため、配置可能エリアを計算 */
            Point nodeAreaEnd = CanvasViewAreaEnd.Sub(node.Width, node.Height);

            /* 座標を配置可能エリアでクリップし、グリッドにスナップさせる */
            Point snapped = Position.Clamp(CanvasViewAreaStart, nodeAreaEnd).RoundSnap(GridSize);

            /* スナップにより表示領域外へ丸められる場合があるため、最後に再度クリップする */
            return snapped.Clamp(CanvasViewAreaStart, nodeAreaEnd);
        }

        /* ---------------------------------------------------------
         * 座標クランプ(接続線ドラッグ用)
         * --------------------------------------------------------- */

        /// <summary>
        /// 任意の点を表示領域内に収める
        /// </summary>
        public Point ClampPoint(Point Position)
        {
            return Position.Clamp(CanvasViewAreaStart, CanvasViewAreaEnd);
        }

        /* ---------------------------------------------------------
         * グリッド線生成
         * --------------------------------------------------------- */

        /// <summary>
        /// 描画用のグリッド線一覧
        /// </summary>
        public ObservableCollection<LineViewModel> GridLines { get; } = [];

        /// <summary>
        /// グリッド線の間隔(描画用)
        /// </summary>
        public double GridSpacing => GridSize * 20;

        /// <summary>
        /// 現在のズーム・表示領域に基づいてグリッド線を再生成する
        /// </summary>
        public void UpdateGrid()
        {
            GridLines.Clear();

            Point origin = CanvasViewOrigin;

            Point gridOrigin = origin.FloorSnap(GridSpacing);

            Point end = gridOrigin.Add(
                CanvasViewLogicalWidth + GridSpacing,
                CanvasViewLogicalHeight + GridSpacing
            );

            double actualSpacing = GridSpacing * Zoom;
            bool showSubGrid = actualSpacing >= 8;

            for (double x = gridOrigin.X; x < end.X; x += GridSpacing)
            {
                int index = (int)Math.Round(x / GridSpacing);
                bool isMajor = (index % 10 == 0);

                if (!isMajor && !showSubGrid)
                    continue;

                GridLines.Add(new LineViewModel
                {
                    Start = new(x, gridOrigin.Y),
                    End = new(x, end.Y),
                    IsMajor = isMajor
                });
            }

            for (double y = gridOrigin.Y; y < end.Y; y += GridSpacing)
            {
                int index = (int)Math.Round(y / GridSpacing);
                bool isMajor = (index % 10 == 0);

                if (!isMajor && !showSubGrid)
                    continue;

                GridLines.Add(new LineViewModel
                {
                    Start = new(gridOrigin.X, y),
                    End = new(end.X, y),
                    IsMajor = isMajor
                });
            }
        }
    }
}

/* --- End of file --- */
