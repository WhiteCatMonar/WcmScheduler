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
            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            editorVM?.RequestBeginConnectionDrag(DataContext);
            CaptureMouse();
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

            /* マウス位置の論理座標を取得して保持 */
            editorVM.RequestUpdateConnectionDrag(e.GetPosition(editor.NodeEditorCanvas));
        }

        /* ---------------------------------------------------------
         * 接続線ドロップ(左ボタン離し)
         * --------------------------------------------------------- */

        /// <summary>
        /// ドロップ位置に入力ポートがあれば接続線を作成する。
        /// </summary>
        private void Port_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            ReleaseMouseCapture();

            if (editorVM is null) {
                return;
            }
            if (!editorVM.IsConnectionDragging) {
                return;
            }
            /* ドロップ位置(画面座標) */
            Point dropScreen = e.GetPosition(editor);

            /* HitTest(画面座標) */
            var hit = VisualTreeHelper.HitTest(editor, dropScreen);

            if (hit?.VisualHit is FrameworkElement fe)
            {
                editorVM.RequestCreateConnection(DataContext, fe.DataContext);
            }
            /* ドラッグ状態解除 */
            editorVM.RequestEndConnectionDrag();
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
            var nodeControl = VisualTreeUtils.FindAncestor<NodeControl>(this);
            var editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
            var editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            if ((nodeControl == null) || (editor == null) || (editorVM == null))
            {
                return;
            }

            /* Portの中心(画面座標) */
            var portScreen = TransformToVisual(editor.NodeEditorArea).Transform(new Point(ActualWidth / 2, ActualHeight / 2));

            /* Nodeの左上(画面座標) */
            var nodeScreen = nodeControl.TransformToVisual(editor.NodeEditorArea).Transform(new Point(0, 0));

            /* Node内の相対座標更新(論理座標) */
            editorVM.RequestUpdatePortRelativePosition(DataContext, nodeScreen, portScreen);
        }
    }
}

/* --- End of file --- */
