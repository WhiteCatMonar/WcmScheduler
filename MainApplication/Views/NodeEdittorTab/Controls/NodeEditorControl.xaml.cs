using MainApplication.ViewModels;
using MainApplication.Views.NodeEditorTab.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainApplication.Views.NodeEditorTab
{
    /// <summary>
    /// NodeEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeEditorControl : UserControl
    {
        private const double MinZoom = 0.2;
        private const double MaxZoom = 3.0;

        public NodeEditorControl()
        {
            InitializeComponent();
        }

        public double Zoom
        {
            get => ZoomTransform.ScaleX;
            set
            {
                ZoomTransform.ScaleX = value;
                ZoomTransform.ScaleY = value;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);

            if (DataContext is NodeEditorViewModel vm)
            {
                vm.UpdateGrid(
                    0,
                    0,
                    NodeEditorCanvas.ActualWidth,
                    NodeEditorCanvas.ActualHeight,
                    vm.GridSpacing
                );
            }
        }

        private void NodeEditorCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as DependencyObject;

            if (clickedElement is TextBoxBase)
            {
                return;
            }

            var node = FindParent<NodeControl>(clickedElement);
            var port = FindParent<PortControl>(clickedElement);
            var connectionPath = FindParent<Path>(clickedElement);

            if (DataContext is NodeEditorViewModel nevm)
            {
                if (node == null)
                {
                    nevm.UnselectNode();
                }
            }

            if (DataContext is NodeEditorViewModel vm)
            {
                /* 接続線をクリックした場合は UnselectConnection しない */
                if (connectionPath != null)
                {
                    return;
                }

                /* ポートでも接続線でもない場合だけ解除 */
                if (port == null)
                {
                    vm.Connections.UnselectConnection();
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                {
                    return parent;
                }

                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void NodeEditorArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is NodeEditorViewModel vm)
            {
                vm.BaseCanvasWidth = e.NewSize.Width;
                vm.BaseCanvasHeight = e.NewSize.Height;
                vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / Zoom;
                vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / Zoom;
                vm.Connections.CanvasViewLogicalWidth = vm.ZoomedCanvasWidth;
                vm.Connections.CanvasViewLogicalHeight = vm.ZoomedCanvasHeight;
                
                /* ViewModelにZoom/Panを反映 */
                vm.Zoom = Zoom;
                vm.PanX = PanTransform.X;
                vm.PanY = PanTransform.Y;

                vm.UpdateGrid(
                    PanTransform.X,
                    PanTransform.Y,
                    vm.BaseCanvasWidth,
                    vm.BaseCanvasHeight,
                    vm.GridSpacing
                );
            }
        }

        private Point _lastPanPoint;
        private bool _isPanning;

        private void NodeEditorCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(DataContext is NodeEditorViewModel vm))
            {
                return;
            }

            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            
            /* ズーム値を計算（制限付き） */
            double limitedZoom = Zoom * zoomFactor;
            limitedZoom = Math.Max(MinZoom, Math.Min(MaxZoom, limitedZoom));

            /* 実際に適用された倍率（中心補正に必要） */
            double appliedFactor = limitedZoom / Zoom;
            
            Zoom = limitedZoom;

            /* ズーム中心をマウス位置に合わせる */
            {
                var mousePos = e.GetPosition(NodeEditorCanvas);

                /* Canvas論理原点とマウス位置の座標差分を算出 */
                var deltaX = mousePos.X - PanTransform.X;
                var deltaY = mousePos.Y - PanTransform.Y;

                /* 座標差分から拡縮時にCanvas論理原点が移動する量を算出 */
                PanTransform.X += deltaX - (deltaX * appliedFactor);
                PanTransform.Y += deltaY - (deltaY * appliedFactor);
            }
            
            /* ViewModelにZoom/Panを反映 */
            vm.Zoom = ZoomTransform.ScaleX;
            vm.PanX = PanTransform.X;
            vm.PanY = PanTransform.Y;

            /* ズーム後サイズを更新 */
            vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / Zoom;
            vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / Zoom;

            /* グリッド更新 */
            vm.UpdateGrid(
                PanTransform.X / Zoom,
                PanTransform.Y / Zoom,
                vm.ZoomedCanvasWidth,
                vm.ZoomedCanvasHeight,
                vm.GridSpacing * Zoom
            );
        }

        private void NodeEditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastPanPoint = e.GetPosition(NodeEditorCanvas);
                NodeEditorCanvas.CaptureMouse();
            }
        }

        private void NodeEditorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(DataContext is NodeEditorViewModel vm))
            {
                return;
            }

            if (_isPanning)
            {
                var pos = e.GetPosition(NodeEditorCanvas);
                var delta = pos - _lastPanPoint;
                _lastPanPoint = pos;

                PanTransform.X += delta.X;
                PanTransform.Y += delta.Y;
                
                /* ViewModelにPanを反映 */
                vm.PanX = PanTransform.X;
                vm.PanY = PanTransform.Y;

                vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / Zoom;
                vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / Zoom;
                /* グリッド更新 */
                vm.UpdateGrid(
                    PanTransform.X / Zoom,
                    PanTransform.Y / Zoom,
                    vm.ZoomedCanvasWidth,
                    vm.ZoomedCanvasHeight,
                    vm.GridSpacing * Zoom
                );
            }
        }

        private void NodeEditorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            NodeEditorCanvas.ReleaseMouseCapture();
        }
    }
}