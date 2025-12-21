using MainApplication.Helpers;
using MainApplication.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MainApplication.ViewModels.PortViewModel;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// PortControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PortControl : UserControl
    {
        private bool _isDragging;

        public PortControl()
        {
            InitializeComponent();
        }

        private void Port_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DataContext is PortViewModel port && port.Type == PortType.Output)
            {
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
                editorVM.Connections.DraggingFromPort = port;


                Debug.WriteLine($"IsDraggingConnection: {editorVM.Connections.IsDraggingConnection}");

                _isDragging = true;
                CaptureMouse();
            }
        }

        private void Port_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                return;
            }
            var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            if ((editor == null) || (editorVM == null))
            {
                return;
            }

            var screenPos = e.GetPosition(editor.NodeEditorCanvas);

            /* 論理座標に変換 */
            var logicalPos = new Point(
                (screenPos.X - editor.PanTransform.X) / editor.ZoomTransform.ScaleX,
                (screenPos.Y - editor.PanTransform.Y) / editor.ZoomTransform.ScaleY
            );

            editorVM.Connections.DraggingToPoint = logicalPos;
        }

        private void Port_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
                ReleaseMouseCapture();
                editorVM.Connections.DraggingFromPort = null;
            }

            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                
                var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);


                Point dropPoint = e.GetPosition(editor.NodeEditorCanvas);

                /* 入力ポートを探す */
                var hit = VisualTreeHelper.HitTest(editor.NodeEditorCanvas, dropPoint);
                if (hit?.VisualHit is FrameworkElement fe && fe.DataContext is PortViewModel targetPort)
                {
                    if (targetPort.Type == PortType.Input)
                    {
                        var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);
                        editorVM?.Connections.CreateConnection((PortViewModel)DataContext, targetPort);
                    }
                }
            }
        }

        public void UpdateRelativePositionFromUI()
        {
            if (!(DataContext is PortViewModel port))
            {
                return;
            }

            var nodeControl = VisualTreeUtils.FindAncestor<NodeControl>(this);
            var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);

            if (nodeControl == null || editor == null)
            {
                return;
            }

            /* PortControl の中心を NodeEditorCanvas 基準で取得（ズーム・パンの影響なし） */
            var posInCanvas = TransformToAncestor(editor.NodeEditorCanvas)
                .Transform(new Point(ActualWidth / 2, ActualHeight / 2));

            /* NodeControl の左上を Canvas 基準で取得 */
            var nodePosInCanvas = nodeControl.TransformToAncestor(editor.NodeEditorCanvas)
                .Transform(new Point(0, 0));

            /* NodeControl 内の相対座標に変換（ズーム・パンの影響なし） */
            var posInNode = new Point(
                posInCanvas.X - nodePosInCanvas.X,
                posInCanvas.Y - nodePosInCanvas.Y
            );

            port.RelativeX = posInNode.X;
            port.RelativeY = posInNode.Y;

            port.UpdateAbsolutePosition();
        }
    }
}
