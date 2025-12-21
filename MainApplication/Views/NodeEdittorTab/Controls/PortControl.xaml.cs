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
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();

                var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

                /* ドロップ位置(画面座標) */
                Point dropScreen = e.GetPosition(editor);

                /* HitTestはeditor(NodeEditorControl)に対して行う(画面座標系で判定) */
                var hit = VisualTreeHelper.HitTest(editor, dropScreen);

                if (hit?.VisualHit is FrameworkElement fe && fe.DataContext is PortViewModel targetPort)
                {
                    if (targetPort.Type == PortType.Input)
                    {
                        editorVM?.Connections.CreateConnection((PortViewModel)DataContext, targetPort);
                    }
                }

                editorVM.Connections.DraggingFromPort = null;
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

            if ((nodeControl == null) || (editor == null))
            {
                return;
            }

            /* NodeEditorCanvas基準でPortControlの中心を取得(ズーム・パン後の見た目座標) */
            GeneralTransform gtPort = this.TransformToVisual(editor.NodeEditorCanvas);
            Point portOnCanvas = gtPort.Transform(new Point(ActualWidth / 2, ActualHeight / 2));

            /* NodeEditorCanvas基準でNodeControlの左上を取得 */
            GeneralTransform gtNode = nodeControl.TransformToVisual(editor.NodeEditorCanvas);
            Point nodeOnCanvas = gtNode.Transform(new Point(0, 0));

            /* Canvas の RenderTransform（ズーム・パン）を逆変換して論理座標へ */
            double zoom = editor.ZoomTransform.ScaleX;
            double panX = editor.PanTransform.X;
            double panY = editor.PanTransform.Y;

            Point portLogical = new Point(
                (portOnCanvas.X - panX) / zoom,
                (portOnCanvas.Y - panY) / zoom
            );

            Point nodeLogical = new Point(
                (nodeOnCanvas.X - panX) / zoom,
                (nodeOnCanvas.Y - panY) / zoom
            );

            /* Node 内の相対座標 */
            Point posInNode = new Point(
                portLogical.X - nodeLogical.X,
                portLogical.Y - nodeLogical.Y
            );

            port.RelativeX = posInNode.X;
            port.RelativeY = posInNode.Y;

            port.UpdateAbsolutePosition();
        }
    }
}
