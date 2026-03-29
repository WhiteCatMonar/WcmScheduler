using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
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
    /// ノードエディタ全体のUI操作(ズーム・パン・選択解除など)を担当するUserControl。
    /// NodeEditorCanvas上でのユーザー操作をViewModelに橋渡しする。
    /// </summary>
    public partial class NodeEditorControl : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// NodeEditorControlを初期化する。
        /// </summary>
        public NodeEditorControl()
        {
            InitializeComponent();
        }

        /* ---------------------------------------------------------
         * Loaded(初期化処理)
         * --------------------------------------------------------- */

        /// <summary>
        /// 初回ロード時にフォーカスを設定し、
        /// ViewModelのGridManagerにUI状態を反映する。
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);

            if (DataContext is NodeEditorViewModel vm)
            {
                /* 初期状態を GridManager に反映 */
                vm.UpdateGridState();
            }
        }

        /* ---------------------------------------------------------
         * キャンバスクリック処理(選択解除など)
         * --------------------------------------------------------- */

        /// <summary>
        /// キャンバス上でクリックされたとき、
        /// ノードや接続線以外をクリックした場合は選択解除を行う。
        /// </summary>
        private void NodeEditorCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeEditorViewModel editor)
            {
                /* 他ノードの編集内容を確定 */
                editor.CommitCurrentNodeEdits();
            }

            var clickedElement = e.OriginalSource as DependencyObject;

            /* TextBoxをクリックした場合は選択解除しない */
            if (clickedElement is TextBoxBase)
            {
                return;
            }

            /* クリック位置のノードor接続線を探索 */
            var node = FindParent<NodeControl>(clickedElement);
            var connectionPath = FindParent<Path>(clickedElement);

            if (DataContext is NodeEditorViewModel vm)
            {
                /* ViewModelに選択状態の更新を依頼 */
                vm.RequestSelectStatusUpdate(node, connectionPath);
            }
        }

        /// <summary>
        /// VisualTreeを遡って指定型の親要素を探す。
        /// </summary>
        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
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

        /* ---------------------------------------------------------
         * キャンバスサイズ変更(論理座標系の更新)
         * --------------------------------------------------------- */

        /// <summary>
        /// キャンバスの表示領域が変わったら、GridManagerに反映する。
        /// </summary>
        private void NodeEditorArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is NodeEditorViewModel vm)
            {
                vm.BaseCanvasWidth = e.NewSize.Width;
                vm.BaseCanvasHeight = e.NewSize.Height;
            }
        }

        /* ---------------------------------------------------------
         * ズーム処理(マウスホイール)
         * --------------------------------------------------------- */

        private Point _lastPanPoint;
        private bool _isPanning;

        /// <summary>
        /// マウスホイールでズームし、ズーム中心をマウス位置に合わせる。
        /// </summary>
        private void NodeEditorCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is not NodeEditorViewModel vm)
            {
                return;
            }

            /* ズーム倍率を計算 */
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            /* マウス位置に合わせて、ズーム処理 */
            vm.RequestZoom(e.GetPosition(NodeEditorCanvas), zoomFactor);
        }

        /* ---------------------------------------------------------
         * パン処理(中クリックドラッグ)
         * --------------------------------------------------------- */

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
            if (DataContext is not NodeEditorViewModel vm)
            {
                return;
            }

            if (_isPanning)
            {
                var pos = e.GetPosition(NodeEditorCanvas);
                var screenDelta = pos.Sub(_lastPanPoint);
                _lastPanPoint = pos;

                /* 画面座標 → 論理座標へ変換 */
                var logicalDelta = vm.ScreenDeltaToLogical(screenDelta);

                vm.RequestPanDelta(logicalDelta);
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

/* --- End of file --- */
