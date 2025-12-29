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
                (screenPos.X - editorVM.PanX) / editorVM.Zoom,
                (screenPos.Y - editorVM.PanY) / editorVM.Zoom
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
            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            if ((nodeControl == null) || (editor == null) || (editorVM == null))
            {
                return;
            }

            /* Port の中心(画面座標) */
            var portScreen = this.TransformToVisual(editor.NodeEditorArea).Transform(new Point(ActualWidth / 2, ActualHeight / 2));

            /* Node の左上(画面座標) */
            var nodeScreen = nodeControl.TransformToVisual(editor.NodeEditorArea).Transform(new Point(0, 0));

            /* 論理座標に変換 */
            var portLogical = new Point(
                (portScreen.X - editorVM.PanX) / editorVM.Zoom,
                (portScreen.Y - editorVM.PanY) / editorVM.Zoom
            );

            var nodeLogical = new Point(
                (nodeScreen.X - editorVM.PanX) / editorVM.Zoom,
                (nodeScreen.Y - editorVM.PanY) / editorVM.Zoom
            );

            /* Node 内の相対座標 */
            port.RelativeX = portLogical.X - nodeLogical.X;
            port.RelativeY = portLogical.Y - nodeLogical.Y;

            port.UpdateAbsolutePosition();
        }
    }
}
