using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// キャンバスのズーム・パン・表示領域・グリッド生成など、
    /// エディタ全体の座標系管理を行うクラス。
    /// </summary>
    public class GridManager : INotifyPropertyChanged
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
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    OnPropertyChanged(nameof(Zoom));
                }
            }
        }

        private double _panX;

        /// <summary>
        /// キャンバスのパン位置(X)
        /// </summary>
        public double PanX
        {
            get => _panX;
            set
            {
                if (_panX != value)
                {
                    _panX = value;
                    OnPropertyChanged(nameof(PanX));
                }
            }
        }

        private double _panY;

        /// <summary>
        /// キャンバスのパン位置(Y)
        /// </summary>
        public double PanY
        {
            get => _panY;
            set
            {
                if (_panY != value)
                {
                    _panY = value;
                    OnPropertyChanged(nameof(PanY));
                }
            }
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
            set
            {
                if (_canvasViewLogicalWidth != value)
                {
                    _canvasViewLogicalWidth = value;
                    OnPropertyChanged(nameof(CanvasViewAreaEndX));
                    OnPropertyChanged(nameof(CanvasViewLogicalWidth));
                }
            }
        }

        private double _canvasViewLogicalHeight;

        /// <summary>
        /// 表示領域の論理高さ(ズーム後)
        /// </summary>
        public double CanvasViewLogicalHeight
        {
            get => _canvasViewLogicalHeight;
            set
            {
                if (_canvasViewLogicalHeight != value)
                {
                    _canvasViewLogicalHeight = value;
                    OnPropertyChanged(nameof(CanvasViewAreaEndY));
                    OnPropertyChanged(nameof(CanvasViewLogicalHeight));
                }
            }
        }

        /* ---------------------------------------------------------
         * キャンバスの表示原点(論理座標)
         * --------------------------------------------------------- */

        private double _canvasViewOriginX;

        /// <summary>
        /// 表示領域の左上X座標(論理座標)
        /// </summary>
        public double CanvasViewOriginX
        {
            get => _canvasViewOriginX;
            set
            {
                if (_canvasViewOriginX != value)
                {
                    _canvasViewOriginX = value;
                    OnPropertyChanged(nameof(CanvasViewAreaStartX));
                    OnPropertyChanged(nameof(CanvasViewAreaEndX));
                    OnPropertyChanged(nameof(CanvasViewOriginX));
                }
            }
        }

        private double _canvasViewOriginY;

        /// <summary>
        /// 表示領域の左上Y座標(論理座標)
        /// </summary>
        public double CanvasViewOriginY
        {
            get => _canvasViewOriginY;
            set
            {
                if (_canvasViewOriginY != value)
                {
                    _canvasViewOriginY = value;
                    OnPropertyChanged(nameof(CanvasViewAreaStartY));
                    OnPropertyChanged(nameof(CanvasViewAreaEndY));
                    OnPropertyChanged(nameof(CanvasViewOriginY));
                }
            }
        }

        /* ---------------------------------------------------------
         * キャンバスの表示領域(論理座標)
         * --------------------------------------------------------- */

        /// <summary>表示領域の開始X座標</summary>
        public double CanvasViewAreaStartX => CanvasViewOriginX;

        /// <summary>表示領域の開始Y座標</summary>
        public double CanvasViewAreaStartY => CanvasViewOriginY;

        /// <summary>表示領域の終了X座標</summary>
        public double CanvasViewAreaEndX => CanvasViewOriginX + CanvasViewLogicalWidth;

        /// <summary>表示領域の終了Y座標</summary>
        public double CanvasViewAreaEndY => CanvasViewOriginY + CanvasViewLogicalHeight;

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
            set
            {
                if (_gridSize != value)
                {
                    _gridSize = value;
                    OnPropertyChanged(nameof(GridSize));
                    OnPropertyChanged(nameof(GridSpacing));
                    UpdateGrid();
                }
            }
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
        public Point ClampNodePosition(double x, double y, NodeViewModel node)
        {
            double clampedX = Math.Max(CanvasViewOriginX,
                Math.Min(x, CanvasViewOriginX + CanvasViewLogicalWidth - node.Width));

            double clampedY = Math.Max(CanvasViewOriginY,
                Math.Min(y, CanvasViewOriginY + CanvasViewLogicalHeight - node.Height));

            clampedX = RoundToGrid(clampedX);
            clampedY = RoundToGrid(clampedY);

            return new Point(clampedX, clampedY);
        }

        /* ---------------------------------------------------------
         * 座標クランプ(接続線ドラッグ用)
         * --------------------------------------------------------- */

        /// <summary>
        /// 任意の点を表示領域内に収める
        /// </summary>
        public Point ClampPoint(Point p)
        {
            double x = Math.Max(CanvasViewOriginX,
                Math.Min(p.X, CanvasViewOriginX + CanvasViewLogicalWidth));

            double y = Math.Max(CanvasViewOriginY,
                Math.Min(p.Y, CanvasViewOriginY + CanvasViewLogicalHeight));

            return new Point(x, y);
        }

        /* ---------------------------------------------------------
         * グリッド線生成
         * --------------------------------------------------------- */

        /// <summary>
        /// 描画用のグリッド線一覧
        /// </summary>
        public ObservableCollection<LineViewModel> GridLines { get; }
            = new ObservableCollection<LineViewModel>();

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

            double originX = CanvasViewOriginX;
            double originY = CanvasViewOriginY;

            double gridOriginX = Math.Floor(originX / GridSpacing) * GridSpacing;
            double gridOriginY = Math.Floor(originY / GridSpacing) * GridSpacing;

            double endX = gridOriginX + CanvasViewLogicalWidth + GridSpacing;
            double endY = gridOriginY + CanvasViewLogicalHeight + GridSpacing;

            double actualSpacing = GridSpacing * Zoom;
            bool showSubGrid = actualSpacing >= 8;

            for (double x = gridOriginX; x < endX; x += GridSpacing)
            {
                int index = (int)Math.Round(x / GridSpacing);
                bool isMajor = (index % 10 == 0);

                if (!isMajor && !showSubGrid)
                    continue;

                GridLines.Add(new LineViewModel
                {
                    X1 = x,
                    Y1 = gridOriginY,
                    X2 = x,
                    Y2 = endY,
                    IsMajor = isMajor
                });
            }

            for (double y = gridOriginY; y < endY; y += GridSpacing)
            {
                int index = (int)Math.Round(y / GridSpacing);
                bool isMajor = (index % 10 == 0);

                if (!isMajor && !showSubGrid)
                    continue;

                GridLines.Add(new LineViewModel
                {
                    X1 = gridOriginX,
                    Y1 = y,
                    X2 = endX,
                    Y2 = y,
                    IsMajor = isMajor
                });
            }
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
