using MainApplication.ViewModels;
using MainApplication.Views.NodeEditorTab.Controls;
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
        public NodeEditorControl()
        {
            InitializeComponent();
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
                vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / ZoomTransform.ScaleX;
                vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / ZoomTransform.ScaleY;
                vm.Connections.CanvasViewLogicalWidth = vm.ZoomedCanvasWidth;
                vm.Connections.CanvasViewLogicalHeight = vm.ZoomedCanvasHeight;

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

            ZoomTransform.ScaleX *= zoomFactor;
            ZoomTransform.ScaleY *= zoomFactor;

            /* ズーム中心をマウス位置に合わせる */
            {
                var mousePos = e.GetPosition(NodeEditorCanvas);

                /* Canvas論理原点とマウス位置の座標差分を算出 */
                var deltaX = mousePos.X - PanTransform.X;
                var deltaY = mousePos.Y - PanTransform.Y;

                /* 座標差分から拡縮時にCanvas論理原点が移動する量を算出 */
                var zoomedDeltaX = deltaX - (deltaX * zoomFactor);
                var zoomedDeltaY = deltaY - (deltaY * zoomFactor);
                PanTransform.X = PanTransform.X + zoomedDeltaX;
                PanTransform.Y = PanTransform.Y + zoomedDeltaY;
            }

            /* ズーム後サイズを更新 */
            vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / ZoomTransform.ScaleX;
            vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / ZoomTransform.ScaleY;

            /* グリッド更新 */
            vm.UpdateGrid(
                PanTransform.X / ZoomTransform.ScaleX,
                PanTransform.Y / ZoomTransform.ScaleY,
                vm.ZoomedCanvasWidth,
                vm.ZoomedCanvasHeight,
                vm.GridSpacing * ZoomTransform.ScaleX
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
                vm.ZoomedCanvasWidth = vm.BaseCanvasWidth / ZoomTransform.ScaleX;
                vm.ZoomedCanvasHeight = vm.BaseCanvasHeight / ZoomTransform.ScaleY;
                /* グリッド更新 */
                vm.UpdateGrid(
                    PanTransform.X / ZoomTransform.ScaleX,
                    PanTransform.Y / ZoomTransform.ScaleY,
                    vm.ZoomedCanvasWidth,
                    vm.ZoomedCanvasHeight,
                    vm.GridSpacing * ZoomTransform.ScaleX
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