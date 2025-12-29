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
    /// ノード(タスク)を表示・操作するためのUserControl。
    /// ドラッグ移動、ポート位置更新、Undo/Redo連携など、
    /// NodeEditorのUI操作を担当する。
    /// </summary>
    public partial class NodeControl : UserControl
    {
        /* ---------------------------------------------------------
         * フィールド(キャッシュ)
         * --------------------------------------------------------- */

        private NodeEditorControl _editor;
        private NodeEditorViewModel _editorVM;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// NodeControlを初期化し、Loaded時に初期セットアップを行う。
        /// </summary>
        public NodeControl()
        {
            InitializeComponent();
            Loaded += NodeControl_Loaded;
        }

        /* ---------------------------------------------------------
         * 初期化処理(Loaded)
         * --------------------------------------------------------- */

        /// <summary>
        /// 初回ロード時にVisualTreeから親要素やViewModelを取得し、
        /// ポート位置の初期計算を行う。
        /// </summary>
        private void NodeControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* 一度だけ実行 */
            Loaded -= NodeControl_Loaded;

            /* VisualTreeをキャッシュ */
            _editor = VisualTreeUtils.FindAncestor<NodeEditorControl>(this);
            _editorVM = VisualTreeUtils.FindParentViewModel<NodeEditorViewModel>(this);

            /* レイアウト完了後にポート位置を確定 */
            Dispatcher.BeginInvoke(new Action(() =>
            {
                /* UI → VM 相対座標更新 */
                UpdatePortPositions();

                /* VM → 絶対座標更新 */
                if (DataContext is NodeViewModel node)
                {
                    node.UpdateAllPortPositions();
                }

                /* 接続線の再描画 */
                _editorVM?.Connections.UpdateAllConnections();
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /* ---------------------------------------------------------
         * ポート位置更新
         * --------------------------------------------------------- */

        /// <summary>
        /// UI上のPortControlの位置を取得し、
        /// PortViewModelのRelativeX/RelativeYに反映する。
        /// </summary>
        private void UpdatePortPositions()
        {
            foreach (var portControl in VisualTreeUtils.FindChildren<PortControl>(this))
            {
                portControl.UpdateRelativePositionFromUI();
            }
        }

        /* ---------------------------------------------------------
         * ノードのドラッグ移動
         * --------------------------------------------------------- */

        private bool _isDragging;
        private Point _lastMousePos;
        private double _startX, _startY;

        /// <summary>
        /// ノードをドラッグ開始する。
        /// 選択状態の更新、開始位置の記録、マウスキャプチャを行う。
        /// </summary>
        private void NodeControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeViewModel node && _editorVM != null)
            {
                _editorVM.Nodes.SelectNode(node);

                _isDragging = true;

                /* キャンバス基準のマウス位置(ズーム・パンの影響を受けない) */
                _lastMousePos = e.GetPosition(_editor.NodeEditorArea);

                /* Undo/Redo用に開始位置を記録 */
                _startX = node.X;
                _startY = node.Y;

                CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// ドラッグ中のノードを移動させる。
        /// ズーム倍率を考慮して論理座標に変換し、GridManagerでクランプする。
        /// </summary>
        private void NodeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && DataContext is NodeViewModel node && _editorVM != null)
            {
                Point current = e.GetPosition(_editor.NodeEditorCanvas);

                /* 画面上の移動量 */
                Vector screenDelta = current - _lastMousePos;
                _lastMousePos = current;

                /* ズーム倍率を考慮して論理座標系に変換 */
                double logicalDeltaX = screenDelta.X / _editorVM.Zoom;
                double logicalDeltaY = screenDelta.Y / _editorVM.Zoom;

                /* ノードの座標を更新(差分加算) */
                double newX = node.X + logicalDeltaX;
                double newY = node.Y + logicalDeltaY;

                /* キャンバス内に制限 */
                var clamped = _editorVM.Grid.ClampNodePosition(newX, newY, node);

                node.X = clamped.X;
                node.Y = clamped.Y;

                /* ポート位置更新 */
                node.UpdateAllPortPositions();
                _editorVM.Connections.UpdateConnectionsForNode(node);
            }
        }

        /// <summary>
        /// ドラッグ終了時にUndo/Redo用の移動履歴を登録する。
        /// </summary>
        private void NodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && DataContext is NodeViewModel node)
            {
                _isDragging = false;
                ReleaseMouseCapture();

                _editorVM.Connections.UpdateConnectionsForNode(node);

                /* Undo/Redo用に移動履歴を登録 */
                _editorVM.Nodes.MoveNode(node, _startX, _startY, node.X, node.Y);
            }
        }

        /* ---------------------------------------------------------
         * ノード選択(枠クリック)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード枠クリック時に選択状態を更新する。
        /// </summary>
        private void NodeBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                _editorVM?.Nodes.SelectNode(node);
            }
        }

        /* ---------------------------------------------------------
         * ノードサイズ変更
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードのサイズが変わったらViewModelに反映し、
        /// ポート位置と接続線を更新する。
        /// </summary>
        private void NodeControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                node.Width = Math.Max(node.MinWidth, e.NewSize.Width);
                node.Height = Math.Max(node.MinHeight, e.NewSize.Height);

                node.UpdateAllPortPositions();
                _editorVM?.Connections.UpdateConnectionsForNode(node);
            }
        }

        /// <summary>
        /// TaskNameの高さが変わった場合(ノード形状が変化)、
        /// ポート位置と接続線を再計算する。
        /// </summary>
        private void TaskName_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /* TaskNameの高さが変わった(ノード形状が変わった)ので、ポート位置を再計算 */
            UpdatePortPositions();

            if (DataContext is NodeViewModel node)
            {
                node.UpdateAllPortPositions();
                _editorVM?.Connections.UpdateConnectionsForNode(node);
            }
        }
    }
}

/* --- End of file --- */
