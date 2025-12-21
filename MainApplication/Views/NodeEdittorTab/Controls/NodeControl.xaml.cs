using MainApplication.Helpers;
using MainApplication.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// NodeControl.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public NodeControl()
        {
            InitializeComponent();
            Loaded += NodeControl_Loaded;
        }

        private void NodeControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* 一度だけ実行 */
            Loaded -= NodeControl_Loaded;

            /* レイアウトが完全に終わってから実行 */
            Dispatcher.BeginInvoke(new Action(() =>
            {
                /* UI → VM 相対座標更新 */
                UpdatePortPositions();

                /* VM → 絶対座標更新 */
                if (DataContext is NodeViewModel node)
                {
                    node.UpdateAllPortPositions();
                }

                // 3. 接続線更新（将来、初期接続がある場合に備えて）
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
                editorVM?.Connections.UpdateAllConnections();
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdatePortPositions()
        {
            foreach (var portControl in VisualTreeUtils.FindChildren<PortControl>(this))
            {
                portControl.UpdateRelativePositionFromUI();
            }
        }

        private bool _isDragging;
        private Point _lastMousePos;
        private double _startX, _startY;

        private void NodeControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
                if (editor?.DataContext is NodeEditorViewModel vm)
                {
                    _isDragging = true;

                    /* Canvas 基準のマウス位置(ズーム・パンの影響を受けない) */
                    _lastMousePos = e.GetPosition(editor.NodeEditorCanvas);

                    /* Undo/Redo 用に開始位置を記録 */
                    _startX = node.X;
                    _startY = node.Y;

                    CaptureMouse();
                    e.Handled = true;
                }
            }
        }

        private void NodeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && DataContext is NodeViewModel node)
            {
                var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
                if (editor?.DataContext is NodeEditorViewModel vm)
                {
                    Point current = e.GetPosition(editor.NodeEditorCanvas);

                    /* 画面上の移動量 */
                    Vector screenDelta = current - _lastMousePos;
                    _lastMousePos = current;

                    /* ズーム倍率を考慮して論理座標系に変換 */
                    double logicalDeltaX = screenDelta.X / editor.ZoomTransform.ScaleX;
                    double logicalDeltaY = screenDelta.Y / editor.ZoomTransform.ScaleY;

                    /* ノードの座標を更新(差分加算) */
                    double newX = node.X + logicalDeltaX;
                    double newY = node.Y + logicalDeltaY;

                    /* キャンバス内に制限 */
                    var clamped = vm.ClampPosition(newX, newY, node);

                    node.X = clamped.X;
                    node.Y = clamped.Y;
                    
                    node.UpdateAllPortPositions();
                    vm.Connections.UpdateAllConnections();
                }
            }
        }

        private void NodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is NodeViewModel node)
            {
                _isDragging = false;
                ReleaseMouseCapture();

                var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
                if (editor?.DataContext is NodeEditorViewModel vm)
                {
                    vm.Connections.UpdateAllConnections();
                    /* Undo/Redo 用に移動履歴を登録 */
                    vm.MoveNode(node, _startX, _startY, node.X, node.Y);
                }
            }
        }

        private void NodeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
                editorVM?.SelectNode(node);
            }
        }

        private void NodeControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                node.Width = Math.Max(node.MinWidth, e.NewSize.Width);
                node.Height = Math.Max(node.MinHeight, e.NewSize.Height);
            }
        }

        private void TaskName_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /* TaskNameの高さが変わった(ノード形状が変わった)ので、ポート位置を再計算 */
            UpdatePortPositions();

            if (DataContext is NodeViewModel node)
            {
                node.UpdateAllPortPositions();
            }

            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
            editorVM?.Connections.UpdateAllConnections();
        }
    }
}