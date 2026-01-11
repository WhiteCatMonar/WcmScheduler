using MainApplication.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// ノードのポート(入出力端子)を表すUIコントロール。
    /// 接続線ドラッグ開始・ドラッグ中の座標更新・ドロップ判定、
    /// およびUI上のポート位置をViewModelに反映する役割を持つ。
    /// </summary>
    public partial class PortControl : UserControl
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private bool _isDragging;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// PortControlを初期化する。
        /// </summary>
        public PortControl()
        {
            InitializeComponent();
        }

        /* ---------------------------------------------------------
         * 接続線ドラッグ開始(左クリック)
         * --------------------------------------------------------- */

        /// <summary>
        /// 出力ポートを左クリックしたとき、接続線ドラッグを開始する。
        /// </summary>
        private void Port_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DataContext is PortViewModel port && port.Type == PortViewModel.PortType.Output)
            {
                var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

                /* ドラッグ開始ポートを記録 */
                editorVM?.Connections.DraggingFromPort = port;

                _isDragging = true;
                CaptureMouse();
            }
        }

        /* ---------------------------------------------------------
         * 接続線ドラッグ中(マウス移動)
         * --------------------------------------------------------- */

        /// <summary>
        /// マウス移動に合わせて接続線の終端座標(DraggingToPoint)を更新する。
        /// </summary>
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

            /* 画面座標 → 論理座標に変換 */
            var logicalPos = new Point(
                (screenPos.X - editorVM.Pan.X) / editorVM.Zoom,
                (screenPos.Y - editorVM.Pan.Y) / editorVM.Zoom
            );

            editorVM.Connections.DraggingToPoint = logicalPos;
        }

        /* ---------------------------------------------------------
         * 接続線ドロップ(左ボタン離し)
         * --------------------------------------------------------- */

        /// <summary>
        /// ドロップ位置に入力ポートがあれば接続線を作成する。
        /// </summary>
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

                /* HitTest(画面座標) */
                var hit = VisualTreeHelper.HitTest(editor, dropScreen);

                if (hit?.VisualHit is FrameworkElement fe && fe.DataContext is PortViewModel targetPort)
                {
                    /* 入力ポートにドロップされた場合のみ接続線を作成 */
                    if (targetPort.Type == PortViewModel.PortType.Input)
                    {
                        editorVM?.Connections.CreateConnection((PortViewModel)DataContext, targetPort);
                    }
                }

                /* ドラッグ状態解除 */
                editorVM?.Connections.DraggingFromPort = null;
            }
        }

        /* ---------------------------------------------------------
         * UI → ViewModel：ポート位置の反映
         * --------------------------------------------------------- */

        /// <summary>
        /// UI上のポート位置(画面座標)を取得し、
        /// Node内の相対座標(RelativePosition)としてViewModelに反映する。
        /// </summary>
        public void UpdateRelativePositionFromUI()
        {
            if (DataContext is not PortViewModel port)
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

            /* 画面座標 → 論理座標 */
            var portLogical = new Point(
                (portScreen.X - editorVM.Pan.X) / editorVM.Zoom,
                (portScreen.Y - editorVM.Pan.Y) / editorVM.Zoom
            );

            var nodeLogical = new Point(
                (nodeScreen.X - editorVM.Pan.X) / editorVM.Zoom,
                (nodeScreen.Y - editorVM.Pan.Y) / editorVM.Zoom
            );

            /* Node 内の相対座標 */
            port.RelativePosition = new(
                portLogical.X - nodeLogical.X,
                portLogical.Y - nodeLogical.Y
            );

            /* 絶対座標も更新 */
            port.UpdateAbsolutePosition();
        }
    }
}

/* --- End of file --- */
