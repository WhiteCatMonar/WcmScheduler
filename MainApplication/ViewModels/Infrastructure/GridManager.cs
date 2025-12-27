using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MainApplication.ViewModels.Infrastructure
{
    public class GridManager : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * ズーム・パン状態
         * --------------------------------------------------------- */
        private double _zoom = 1.0;
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
         * Canvas の論理サイズ(ズーム後のサイズ)
         * --------------------------------------------------------- */
        private double _canvasViewLogicalWidth;
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
         * Canvas の表示原点(論理座標)
         * --------------------------------------------------------- */
        private double _canvasViewOriginX;
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
         * Canvas の表示領域(論理座標)
         * --------------------------------------------------------- */
        public double CanvasViewAreaStartX => CanvasViewOriginX;
        public double CanvasViewAreaStartY => CanvasViewOriginY;
        public double CanvasViewAreaEndX => CanvasViewOriginX + CanvasViewLogicalWidth;
        public double CanvasViewAreaEndY => CanvasViewOriginY + CanvasViewLogicalHeight;

        /* ---------------------------------------------------------
         * グリッドスナップ
         * --------------------------------------------------------- */
        private double _gridSize = 1.0;
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

        public double RoundToGrid(double value)
            => Math.Round(value / GridSize) * GridSize;

        /* ---------------------------------------------------------
         * 座標クランプ(ノード用)
         * --------------------------------------------------------- */
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
        public ObservableCollection<LineViewModel> GridLines { get; }
            = new ObservableCollection<LineViewModel>();
        public double GridSpacing => GridSize * 20;

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
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}