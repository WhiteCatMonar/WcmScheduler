using MainApplication.Helpers;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MainApplication.Views.ProjectEditor.DependencyEditor
{
    /// <summary>
    /// ノード(タスク)を表示・操作するためのUserControl。
    /// ドラッグ移動、ポート位置更新、Undo/Redo連携など、
    /// NodeEditorのUI操作を担当する。
    /// </summary>
    public partial class TaskNodeControl : UserControl
    {
        /* ---------------------------------------------------------
         * フィールド(キャッシュ)
         * --------------------------------------------------------- */

        private DependencyEditorView? _editor;
        private DependencyEditorViewModel? _editorVM;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// TaskNodeControlを初期化し、Loaded時に初期セットアップを行う。
        /// </summary>
        public TaskNodeControl()
        {
            InitializeComponent();
            Loaded += TaskNodeControl_Loaded;
        }

        /* ---------------------------------------------------------
         * 初期化処理(Loaded)
         * --------------------------------------------------------- */

        /// <summary>
        /// 初回ロード時にVisualTreeから親要素やViewModelを取得し、
        /// ポート位置の初期計算を行う。
        /// </summary>
        private void TaskNodeControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* 一度だけ実行 */
            Loaded -= TaskNodeControl_Loaded;

            /* VisualTreeをキャッシュ */
            _editor = VisualTreeUtils.FindAncestor<DependencyEditorView>(this);
            _editorVM = VisualTreeUtils.FindParentViewModel<DependencyEditorViewModel>(this);

            UpdateVisualBoundsSize();
        }

        /* ---------------------------------------------------------
         * ノードのドラッグ移動
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードをドラッグ開始する。
        /// 選択状態の更新、開始位置の記録、マウスキャプチャを行う。
        /// </summary>
        private void TaskNodeControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _editorVM?.RequestSelectNode(DataContext);

            /* Undo/Redo用に開始位置を記録 */
            _editorVM?.RequestBeginNodeDrag(DataContext, e.GetPosition(_editor?.NodeEditorCanvas));

            CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// ドラッグ中のノードを移動させる。
        /// ズーム倍率を考慮して論理座標に変換し、GridManagerでクランプする。
        /// </summary>
        private void TaskNodeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_editor == null)
            {
                return;
            }
            Point current = e.GetPosition(_editor?.NodeEditorCanvas);

            /* 画面上の移動量 */
            _editorVM?.RequestDragNode(DataContext, current);
        }

        /// <summary>
        /// ドラッグ終了時にUndo/Redo用の移動履歴を登録する。
        /// </summary>
        private void TaskNodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            _editorVM?.RequestEndNodeDrag(DataContext);
        }

        /* ---------------------------------------------------------
         * ノード選択(枠クリック)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード枠クリック時に選択状態を更新する。
        /// </summary>
        private void NodeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _editorVM?.RequestSelectNode(DataContext);
        }

        /* ---------------------------------------------------------
         * ノードサイズ変更
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードのサイズが変わったらViewModelに反映し、
        /// ポート位置と接続線を更新する。
        /// </summary>
        private void TaskNodeControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualBoundsSize();
        }

        /// <summary>
        /// ノードの実描画範囲をViewModelのサイズへ反映する。
        /// </summary>
        private void UpdateVisualBoundsSize()
        {
            if (_editorVM == null)
            {
                return;
            }

            Rect layoutBounds = new(new Point(0.0, 0.0), RenderSize);
            Rect descendantBounds = VisualTreeHelper.GetDescendantBounds(this);
            Rect visualBounds = descendantBounds.IsEmpty ? layoutBounds : Rect.Union(layoutBounds, descendantBounds);

            var visualBoundsSize = new Size(
                visualBounds.Right - Math.Min(0.0, visualBounds.Left),
                visualBounds.Bottom - Math.Min(0.0, visualBounds.Top)
            );

            _editorVM?.RequestNodeSizeChanged(DataContext, visualBoundsSize);
        }

        /// <summary>
        /// TaskNameの高さが変わった場合(ノード形状が変化)、
        /// ポート位置と接続線を再計算する。
        /// </summary>
        private void TaskName_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var portControl in VisualTreeUtils.FindChildren<PortControl>(this))
            {
                /* UI上のPortControlの位置を取得し、PortViewModelのRelativePositionに反映する。 */
                portControl.UpdateRelativePositionFromUI();
            }
            _editorVM?.RequestTaskNameSizeChanged(DataContext);
        }
    }
}

/* --- End of file --- */
