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
    /// NodeEditorControl.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeEditorControl : UserControl
    {
        private const double MinZoom = 0.2;
        private const double MaxZoom = 3.0;

        public NodeEditorControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);

            if (DataContext is NodeEditorViewModel vm)
            {
                /* 初期状態を GridManager に反映 */
                vm.UpdateGridState();
            }
        }

        private void NodeEditorCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeEditorViewModel editor)
            {
                editor.Nodes.SelectedNode?.CommitEdits();
            }

            var clickedElement = e.OriginalSource as DependencyObject;

            if (clickedElement is TextBoxBase)
            {
                return;
            }

            var node = FindParent<NodeControl>(clickedElement);
            var connectionPath = FindParent<Path>(clickedElement);

            if (DataContext is NodeEditorViewModel vm)
            {
                /* ポートを選択した場合はnodeとconnectionPathの両方がnullになる */
                
                /* ノード以外をクリックした場合はノードの選択を解除 */
                if (node == null)
                {
                    vm.Nodes.UnselectNode();
                }

                /* 接続線以外をクリックした場合は接続線の選択を解除 */
                if (connectionPath == null)
                {
                    vm.Connections.UnselectConnection();
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
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

            /* ズーム値を計算(制限付き) */
            double limitedZoom = Math.Max(MinZoom, Math.Min(MaxZoom, vm.Zoom * zoomFactor));

            /* 実際に適用された倍率(中心補正に必要) */
            double appliedFactor = limitedZoom / vm.Zoom;
            vm.Zoom = limitedZoom;

            /* ズーム中心をマウス位置に合わせる */
            {
                var mousePos = e.GetPosition(NodeEditorCanvas);

                /* Canvas論理原点とマウス位置の座標差分を算出 */
                var deltaX = mousePos.X - vm.PanX;
                var deltaY = mousePos.Y - vm.PanY;

                /* 座標差分から拡縮時にCanvas論理原点が移動する量を算出 */
                vm.PanX += deltaX - (deltaX * appliedFactor);
                vm.PanY += deltaY - (deltaY * appliedFactor);
            }
        }

        private void StartPanning(Point startPoint)
        {
            _isPanning = true;
            _lastPanPoint = startPoint;
            NodeEditorCanvas.CaptureMouse();
        }
        
        private void EndPanning()
        {
            _isPanning = false;
            NodeEditorCanvas.ReleaseMouseCapture();
        }

        private void NodeEditorCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                StartPanning(e.GetPosition(NodeEditorCanvas));
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

                vm.PanX += delta.X;
                vm.PanY += delta.Y;
            }
        }

        private void NodeEditorCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                EndPanning();
            }
        }
    }
}