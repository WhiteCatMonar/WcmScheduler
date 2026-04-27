using MainApplication.Mappers;
using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using MainApplication.Views.NodeEditorTab.Controls;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノードエディタ全体を統括するViewModel。
    /// UI状態、ノード・接続線管理、Undo/Redo、データ入出力などを扱う。
    /// </summary>
    public class NodeEditorViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _statusTimer;

        /* ---------------------------------------------------------
         * 基本プロパティ(UIの表示状態)
         * --------------------------------------------------------- */

        private double _baseCanvasWidth;

        /// <summary>
        /// キャンバスの実際の幅(ズーム前)
        /// </summary>
        public double BaseCanvasWidth
        {
            get => _baseCanvasWidth;
            set => SetProperty(
                ref _baseCanvasWidth,
                value,
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateGridState()
                )
            );
        }

        private double _baseCanvasHeight;
        
        /// <summary>
        /// キャンバスの実際の高さ(ズーム前)
        /// </summary>
        public double BaseCanvasHeight
        {
            get => _baseCanvasHeight;
            set => SetProperty(
                ref _baseCanvasHeight,
                value,
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateGridState()
                )
            );
        }

        private double _zoom = 1.0;

        /// <summary>
        /// ズーム倍率
        /// </summary>
        public double Zoom
        {
            get => _zoom;
            set => SetProperty(
                ref _zoom,
                value,
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateGridState()
                )
            );
        }


        private Point _pan;

        /// <summary>
        /// パン位置
        /// </summary>
        public Point Pan
        {
            get => _pan;
            set => SetProperty(
                ref _pan,
                value,
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateGridState()
                )
            );
        }

        /* ---------------------------------------------------------
         * 座標系管理
         * --------------------------------------------------------- */

        /// <summary>
        /// ズーム・パン・論理座標系を管理するGridManager
        /// </summary>
        public GridManager Grid { get; }

        /// <summary>
        /// 画面座標を論理座標に変換する。
        /// ズーム倍率とパン位置を考慮して変換を行う。
        /// </summary>
        /// <param name="screen">画面座標</param>
        /// <returns>論理座標</returns>
        public Point ScreenToLogical(Point screen)
            => Grid.ScreenToLogical(screen);

        /// <summary>
        /// 論理座標を画面座標に変換する。
        /// ズーム倍率とパン位置を考慮して変換を行う。
        /// </summary>
        /// <param name="logical">論理座標</param>
        /// <returns>画面座標</returns>
        public Point LogicalToScreen(Point logical)
            => Grid.LogicalToScreen(logical);

        /// <summary>
        /// 画面上の移動量を論理座標系の移動量に変換する。
        /// </summary>
        /// <param name="screenDelta">画面座標での移動量</param>
        /// <returns>論理座標での移動量</returns>
        public Point ScreenDeltaToLogical(Point delta)
            => Grid.ScreenDeltaToLogical(delta);


        /// <summary>
        /// ズーム・パン・キャンバスサイズをGridManagerに反映し、
        /// グリッド線を更新する。
        /// </summary>
        public void UpdateGridState()
        {
            /* ズーム・パン */
            Grid.Zoom = Zoom;
            Grid.Pan = Pan;

            /* 論理座標系のサイズ */
            Grid.CanvasViewLogicalWidth = BaseCanvasWidth / Zoom;
            Grid.CanvasViewLogicalHeight = BaseCanvasHeight / Zoom;

            /* 論理原点 */
            Grid.CanvasViewOrigin = Pan.MirrorPoint().Div(Zoom);

            /* グリッド線更新 */
            Grid.UpdateGrid();
        }

        /// <summary>
        /// ズーム制限値(最小値)
        /// </summary>
        private const double MinZoom = 0.2;

        /// <summary>
        /// ズーム制限値(最大値)
        /// </summary>
        private const double MaxZoom = 3.0;

        /// <summary>
        /// ズーム処理を行う。
        /// </summary>
        /// <remarks>
        /// ズーム倍率は[0.2<=zoom<=3.0]の範囲内で制限される。
        /// </remarks>
        /// <param name="zoomCenter">
        /// ズームを行う際の中心座標。
        /// </param>
        /// <param name="scaleFactor">
        /// 現在のズーム値に乗算する倍率。
        /// 例：Zoom=1.5 のとき scaleFactor=2.0 → Zoom=3.0
        /// </param>
        public void RequestZoom(Point zoomCenter, double scaleFactor)
        {
            /* ズーム値を計算 */
            double limitedZoom = Zoom * scaleFactor;

            /* ズーム値を制限 */
            limitedZoom = Math.Clamp(limitedZoom, MinZoom, MaxZoom);

            /* 実際に適用された倍率(中心補正に必要) */
            double appliedFactor = limitedZoom / Zoom;
            Zoom = limitedZoom;

            /* ズーム中心が画面上で固定されるように Pan を補正 */
            {
                /* 論理原点とズーム中心の座標差分を算出 */
                var delta = zoomCenter.Sub(Pan);

                /* 座標差分から拡縮時に論理原点が移動する量を算出 */
                Pan = Pan.Add(delta.Sub(delta.Mul(appliedFactor)));
            }
        }

        /// <summary>
        /// 論理座標系でのパン移動量を適用する。
        /// </summary>
        /// <param name="logicalDelta">論理座標系での移動量</param>
        public void RequestPanDelta(Point logicalDelta)
        {
            Pan = Pan.Add(logicalDelta);
        }

        /* ---------------------------------------------------------
         * 選択状態管理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード・接続線の選択状態を更新する。
        /// </summary>
        /// <param name="nodeContext">
        /// View側で選択されているノードのDataContext（未選択の場合は null）
        /// </param>
        /// <param name="connectionContext">
        /// View側で選択されている接続線のDataContext（未選択の場合は null）
        /// </param>

        public void RequestSelectStatusUpdate(object? nodeContext, object? connectionContext)
        {
            /* ノードがView側で未選択 → ノード選択解除 */
            if (nodeContext is null)
            {
                Nodes.UnselectNode();
            }

            /* 接続線がView側で未選択 → 接続線選択解除 */
            if (connectionContext is null)
            {
                Connections.UnselectConnection();
            }
        }

        /* ---------------------------------------------------------
         * ノード管理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード一覧を管理する ViewModel。
        /// ノードの追加・削除・選択・移動などの操作を提供する。
        /// </summary>
        public NodeCollectionViewModel Nodes { get; }
        
        /// <summary>
        /// ノードとそのポート一覧を関連付ける辞書。
        /// PortViewModel は NodeViewModel に依存しないため、
        /// 所属ノードの逆引きに使用する。
        /// </summary>
        public Dictionary<NodeViewModel, List<PortViewModel>> NodePorts { get; } = [];

        /// <summary>
        /// ノード・接続線の位置を再計算する。
        /// Undo/Redo やズーム変更後に使用。
        /// </summary>
        private void RefreshNodeAndConnectionPositions()
        {
            Nodes.UpdateAllNodes();
            Connections.UpdateAllConnections();
            RefreshTaskStatuses();
        }

        /// <summary>
        /// 選択中ノードの編集内容を確定する。
        /// </summary>
        public void CommitCurrentNodeEdits()
        {
            Nodes.SelectedNode?.Detail.CommitEdits();
        }

        /// <summary>
        /// すべてのタスク状態を再計算する。
        /// </summary>
        public void RefreshTaskStatuses()
        {
            foreach (var node in Nodes.Nodes)
            {
                node.Status = CalculateTaskStatus(node);
            }
        }

        /// <summary>
        /// 指定タスクの状態を算出する。
        /// </summary>
        /// <param name="node">算出対象のノード。</param>
        /// <returns>算出されたタスク状態。</returns>
        private NodeViewModel.TaskStatus CalculateTaskStatus(NodeViewModel node)
        {
            var now = DateTime.Now;

            if (node.Detail.EndDateTime != null && node.Detail.EndDateTime <= now)
            {
                return NodeViewModel.TaskStatus.Done;
            }

            if (IsInSuspensionPeriod(node, now) || GetUpstreamNodes(node).Any(upstream => !IsDone(upstream, now)))
            {
                return NodeViewModel.TaskStatus.Pending;
            }

            if (node.Detail.StartDateTime != null && node.Detail.StartDateTime <= now)
            {
                return NodeViewModel.TaskStatus.InProgress;
            }

            return NodeViewModel.TaskStatus.Ready;
        }

        /// <summary>
        /// 指定タスクが完了済みかどうかを判定する。
        /// </summary>
        /// <param name="node">判定対象のノード。</param>
        /// <param name="now">現在日時。</param>
        /// <returns>完了済みならtrue。</returns>
        private static bool IsDone(NodeViewModel node, DateTime now)
        {
            return node.Detail.EndDateTime != null && node.Detail.EndDateTime <= now;
        }

        /// <summary>
        /// 指定タスクが現在中断期間中かどうかを判定する。
        /// </summary>
        /// <param name="node">判定対象のノード。</param>
        /// <param name="now">現在日時。</param>
        /// <returns>中断期間中ならtrue。</returns>
        private static bool IsInSuspensionPeriod(NodeViewModel node, DateTime now)
        {
            return node.Detail.SuspensionPeriods.Any(period =>
                period.StartDateTime != null &&
                period.EndDateTime != null &&
                period.StartDateTime <= now &&
                now <= period.EndDateTime
            );
        }

        /// <summary>
        /// 入力ポートへ接続している前段タスクを取得する。
        /// </summary>
        /// <param name="node">取得対象のノード。</param>
        /// <returns>前段タスク一覧。</returns>
        private IEnumerable<NodeViewModel> GetUpstreamNodes(NodeViewModel node)
        {
            foreach (var connection in Connections.Connections)
            {
                var toNode = FindNodeByPort(connection.ToPort);
                if (!ReferenceEquals(toNode, node))
                {
                    continue;
                }

                var fromNode = FindNodeByPort(connection.FromPort);
                if (fromNode != null)
                {
                    yield return fromNode;
                }
            }
        }

        /// <summary>
        /// ポートの所属ノードを取得する。
        /// </summary>
        /// <param name="port">検索対象ポート。</param>
        /// <returns>所属ノード。見つからない場合はnull。</returns>
        private NodeViewModel? FindNodeByPort(PortViewModel port)
        {
            return NodePorts.FirstOrDefault(kv => kv.Value.Contains(port)).Key;
        }

        /// <summary>
        /// ノードを状態再計算の監視対象として登録する。
        /// </summary>
        /// <param name="node">登録対象ノード。</param>
        private void RegisterNodeForStatusRefresh(NodeViewModel node)
        {
            node.Detail.PropertyChanged -= OnNodeDetailStatusPropertyChanged;
            node.Detail.PropertyChanged += OnNodeDetailStatusPropertyChanged;
            node.Detail.SuspensionPeriods.CollectionChanged -= OnSuspensionPeriodsChanged;
            node.Detail.SuspensionPeriods.CollectionChanged += OnSuspensionPeriodsChanged;

            foreach (var period in node.Detail.SuspensionPeriods)
            {
                period.PropertyChanged -= OnSuspensionPeriodStatusPropertyChanged;
                period.PropertyChanged += OnSuspensionPeriodStatusPropertyChanged;
            }
        }

        /// <summary>
        /// ノードを状態再計算の監視対象から解除する。
        /// </summary>
        /// <param name="node">解除対象ノード。</param>
        private void UnregisterNodeForStatusRefresh(NodeViewModel node)
        {
            node.Detail.PropertyChanged -= OnNodeDetailStatusPropertyChanged;
            node.Detail.SuspensionPeriods.CollectionChanged -= OnSuspensionPeriodsChanged;

            foreach (var period in node.Detail.SuspensionPeriods)
            {
                period.PropertyChanged -= OnSuspensionPeriodStatusPropertyChanged;
            }
        }

        /// <summary>
        /// ノード一覧変更時に状態再計算の監視対象を更新する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (NodeViewModel node in e.OldItems)
                {
                    UnregisterNodeForStatusRefresh(node);
                }
            }

            if (e.NewItems != null)
            {
                foreach (NodeViewModel node in e.NewItems)
                {
                    RegisterNodeForStatusRefresh(node);
                }
            }

            RefreshTaskStatuses();
        }

        /// <summary>
        /// 接続線一覧変更時にタスク状態を再計算する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTaskStatuses();
        }

        /// <summary>
        /// ノード詳細変更時にタスク状態を再計算する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnNodeDetailStatusPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(NodeDetailViewModel.StartDateTime) or nameof(NodeDetailViewModel.EndDateTime))
            {
                RefreshTaskStatuses();
            }
        }

        /// <summary>
        /// 中断期間一覧変更時に監視対象を更新し、タスク状態を再計算する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnSuspensionPeriodsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SuspensionPeriodViewModel period in e.OldItems)
                {
                    period.PropertyChanged -= OnSuspensionPeriodStatusPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (SuspensionPeriodViewModel period in e.NewItems)
                {
                    period.PropertyChanged += OnSuspensionPeriodStatusPropertyChanged;
                }
            }

            RefreshTaskStatuses();
        }

        /// <summary>
        /// 中断期間変更時にタスク状態を再計算する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnSuspensionPeriodStatusPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(SuspensionPeriodViewModel.StartDateTime) or nameof(SuspensionPeriodViewModel.EndDateTime))
            {
                RefreshTaskStatuses();
            }
        }

        private Point _start;
        private Point _lastMousePos;

        private bool _isNodeDragging;

        /// <summary>
        /// ノードドラッグ中かどうか
        /// </summary>
        public bool IsNodeDragging => _isNodeDragging;

        /// <summary>
        /// ノードを選択状態にする。
        /// NodeControl からの選択要求を受け取り、
        /// NodeCollectionViewModel に処理を委譲する。
        /// </summary>
        /// <param name="dataContext">選択対象のノードのDataContext</param>
        public void RequestSelectNode(object? dataContext)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }
            Nodes.SelectNode(node);
        }

        /// <summary>
        /// ノードのドラッグ操作を開始する。
        /// 開始位置とマウス座標を記録し、Undo/Redo用の初期状態を保持する。
        /// </summary>
        /// <param name="dataContext">ドラッグ対象ノードのDataContext</param>
        /// <param name="screenPos">ドラッグ開始時の画面座標</param>
        public void RequestBeginNodeDrag(object? dataContext, Point screenPos)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }
            _isNodeDragging = true;
            _lastMousePos = screenPos;
            _start = node.Position;
        }

        /// <summary>
        /// ノードをドラッグ中に移動させる。
        /// 画面座標の差分を論理座標に変換し、
        /// GridManagerによるクランプ処理を行った上で
        /// ノードの位置とポート位置を更新する。
        /// </summary>
        /// <param name="dataContext">移動対象ノードのDataContext</param>
        /// <param name="screenPos">現在のマウス位置(画面座標)</param>
        public void RequestDragNode(object? dataContext, Point screenPos)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }

            if (!_isNodeDragging)
            {
                return;
            }

            /* 画面上の移動量 */
            Point screenDelta = screenPos.Sub(_lastMousePos);
            _lastMousePos = screenPos;

            /* ズーム倍率を考慮して論理座標系に変換 */
            Point logicalDelta = ScreenDeltaToLogical(screenDelta);

            /* ノードの座標を更新(差分加算) */
            Point newPosition = node.Position.Add(logicalDelta);

            /* キャンバス内に制限 */
            node.Position = Grid.ClampNodePosition(newPosition, node);

            /* ポート位置更新 */
            node.UpdateAllPortPositions();
            ConnectionCollectionViewModel.UpdateConnectionsForNode(node);
        }


        /// <summary>
        /// ノードのドラッグ操作を終了する。
        /// Undo/Redo 用に移動履歴を登録し、
        /// 接続線の最終位置を更新する。
        /// </summary>
        /// <param name="dataContext">ドラッグ終了ノードのDataContext</param>
        public void RequestEndNodeDrag(object? dataContext)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }

            if (!_isNodeDragging)
            {
                return;
            }

            _isNodeDragging = false;
            ConnectionCollectionViewModel.UpdateConnectionsForNode(node);

            /* Undo/Redo用に移動履歴を登録 */
            Nodes.MoveNode(node, _start, node.Position);
        }

        /// <summary>
        /// ノードのサイズ変更を反映し、
        /// ポート位置と接続線を再計算する。
        /// </summary>
        /// <param name="dataContext">対象ノードのDataContext</param>
        /// <param name="newSize">新しいサイズ</param>
        public void RequestNodeSizeChanged(object? dataContext, Size newSize)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }
            node.Width = Math.Max(NodeViewModel.MinWidth, newSize.Width);
            node.Height = Math.Max(NodeViewModel.MinHeight, newSize.Height);
            node.Position = Grid.ClampNodePosition(node.Position, node);
            
            node.UpdateAllPortPositions();
            ConnectionCollectionViewModel.UpdateConnectionsForNode(node);
        }

        /// <summary>
        /// TaskName の高さ変更に伴い、
        /// ノード形状が変化した際にポート位置と接続線を再計算する。
        /// </summary>
        /// <param name="dataContext">対象ノードのDataContext</param>
        public void RequestTaskNameSizeChanged(object? dataContext)
        {
            if (dataContext is not NodeViewModel node)
            {
                return;
            }
            node.UpdateAllPortPositions();
            ConnectionCollectionViewModel.UpdateConnectionsForNode(node);
        }

        /* ---------------------------------------------------------
         * 接続線管理
         * --------------------------------------------------------- */

        /// <summary> 接続線一覧管理 </summary>
        public ConnectionCollectionViewModel Connections { get; }

        private bool _isConnectionDragging;

        /// <summary>
        /// 接続線ドラッグ中かどうか
        /// </summary>
        public bool IsConnectionDragging => _isConnectionDragging;

        /// <summary>
        /// UIからの接続線作成リクエストを受け取り、
        /// 入力ポート判定・自己接続禁止・同一ノード禁止などの
        /// 妥当性チェックを行った上で接続線を作成する。
        /// </summary>
        /// <param name="dataContextForFrom">接続元ポートのDataContext</param>
        /// <param name="dataContextForTo">接続先ポートのDataContext</param>
        public void RequestCreateConnection(object? dataContextForFrom, object? dataContextForTo)
        {
            if (dataContextForFrom is not PortViewModel fromPort)
            {
                return;
            }

            if (dataContextForTo is not PortViewModel toPort)
            {
                return;
            }

            /* 入力ポートにドロップされた場合のみ接続線を作成 */
            if (toPort.Type != PortViewModel.PortType.Input)
            {
                return;
            }

            if (fromPort == toPort)
            {
                return; /* 自己接続禁止 */
            }

            /* NodePortsを使って所属ノードを逆引き */
            var fromNode = NodePorts.FirstOrDefault(kv => kv.Value.Contains(fromPort)).Key;
            var toNode = NodePorts.FirstOrDefault(kv => kv.Value.Contains(toPort)).Key;

            if ((fromNode == null) || (toNode == null))
            {
                return; /* 辞書に登録されていない */
            }

            if (fromNode == toNode)
            {
                return; /* 同一ノード内禁止 */
            }

            /* リクエストOK: 接続線作成 */
            Connections.CreateConnection(fromPort, toPort);
        }

        /// <summary>
        /// 接続線ドラッグ操作の開始を要求する。
        /// PortControlから呼び出され、
        /// 出力ポートである場合のみドラッグ状態に遷移する。
        /// </summary>
        /// <param name="dataContext">
        /// ドラッグ開始対象となるポートのDataContext(PortViewModel)
        /// </param>
        public void RequestBeginConnectionDrag(object? dataContext)
        {
            if (dataContext is not PortViewModel port)
            {
                return;
            }

            /* 出力ポート以外はドラッグ開始不可 */
            if (port.Type != PortViewModel.PortType.Output)
            {
                return;
            }

            /* ドラッグ開始ポートを記録 */
            Connections.DraggingFromPort = port;
            _isConnectionDragging = true;
        }

        /// <summary>
        /// 接続線ドラッグ中に、マウス等のポインタ位置に応じて
        /// 接続線の終端座標(DraggingToPoint)を更新する。
        /// </summary>
        /// <param name="screenPos">画面座標系でのマウス等のポインタ位置</param>
        public void RequestUpdateConnectionDrag(Point screenPos)
        {
            /* 論理座標を取得して保持 */
            Connections.DraggingToPoint = ScreenToLogical(screenPos);
        }

        /// <summary>
        /// 接続線ドラッグ操作の終了を要求する。
        /// ドラッグ開始ポートを解除し、ドラッグ状態をリセットする。
        /// </summary>
        public void RequestEndConnectionDrag()
        {
            Connections.DraggingFromPort = null;
            _isConnectionDragging = false;
        }

        /// <summary>
        /// UI上のポート位置をもとに、
        /// ノード内の相対座標(RelativePosition)を更新する。
        /// </summary>
        /// <param name="dataContext">対象ポートのDataContext(PortViewModel)</param>
        /// <param name="screenCtrlPoint">ノード左上の画面座標</param>
        /// <param name="screenAbdPos">ポート中心の画面座標</param>
        public void RequestUpdatePortRelativePosition(object? dataContext, Point screenCtrlPoint, Point screenAbdPos)
        {
            if (dataContext is not PortViewModel port)
            {
                return;
            }

            /* 基準点からの相対座標(論理座標) */
            /* NOTE: 絶対座標は相対座標更新時に自動計算 */
            port.RelativePosition = ScreenDeltaToLogical(screenAbdPos.Sub(screenCtrlPoint));
        }

        /* ---------------------------------------------------------
         * 操作履歴管理(Undo/Redo)
         * --------------------------------------------------------- */

        private UndoRedoManager _undoredo = new();

        /// <summary>
        /// Undo/Redo管理クラス
        /// </summary>
        public UndoRedoManager UndoRedo
        {
            get => _undoredo;

            /* NOTE: UndoRedoManagerは参照型のため、同一インスタンス再代入では通知されない。 */
            set => SetProperty(ref _undoredo, value);
        }

        /// <summary>Undo コマンド</summary>
        public ICommand UndoCommand { get; }

        /// <summary>Redo コマンド</summary>
        public ICommand RedoCommand { get; }

        /// <summary>履歴ジャンプコマンド</summary>
        public ICommand MoveToHistoryCommand { get; }

        /// <summary>現在の履歴位置が変化したときに発火するイベント</summary>
        public event EventHandler<UndoRedoManager.HistoryItem?>? CurrentHistoryChanged;

        private void OnCurrentHistoryChanged(object? sender, UndoRedoManager.HistoryItem? e)
        {
            CurrentHistoryChanged?.Invoke(this, e);
            RefreshNodeAndConnectionPositions();
        }

        /* ---------------------------------------------------------
         * DateTimeEditorService(日時編集サービス)
         * --------------------------------------------------------- */

        private IDateTimeEditorService _dateTimeEditor = new DateTimeEditorService();

        /// <summary>
        /// 日時編集ダイアログを提供するサービス
        /// </summary>
        public IDateTimeEditorService DateTimeEditor
        {
            get => _dateTimeEditor;
            set => SetProperty(ref _dateTimeEditor, value);
        }

        /* ---------------------------------------------------------
         * データ読み込み
         * --------------------------------------------------------- */

        /// <summary>
        /// 保存データを読み込み、ノード・接続線を復元する。
        /// </summary>
        public void LoadFromTaskEditorDataModel(TaskEditorDataModel data)
        {
            Nodes.Nodes.Clear();
            Connections.Connections.Clear();
            NodePorts.Clear();
            NodeEditorViewModel loadedData = NodeEditorMapper.ToViewModel(data, this);


            foreach (var loadedNodes in loadedData.Nodes.Nodes)
            {
                Nodes.Nodes.Add(loadedNodes);
            }

            foreach (var loadedNodePort in loadedData.NodePorts)
            {
                NodePorts.Add(loadedNodePort.Key, loadedNodePort.Value);
            }

            foreach (var loadedConnections in loadedData.Connections.Connections)
            {
                Connections.Connections.Add(loadedConnections);
            }

            RefreshNodeAndConnectionPositions();
            RefreshTaskStatuses();

            /* 表示領域をリセット */
            Zoom = 1.0;
            Pan = new(0.0, 0.0);
            UpdateGridState();

            /* 編集履歴をリセット */
            UndoRedo.Clear();
        }

        /* ---------------------------------------------------------
         * データ保存
         * --------------------------------------------------------- */

        /// <summary>
        /// 現在の状態を保存用データモデルに変換する。
        /// </summary>
        public void SaveToTaskEditorDataModel(out TaskEditorDataModel data)
        {
            data = NodeEditorMapper.ToDataModel(this);
        }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// NodeEditorViewModel を生成し、各管理クラスを初期化する。
        /// </summary>
        public NodeEditorViewModel()
        {
            Nodes = new NodeCollectionViewModel(UndoRedo, DateTimeEditor, this);
            Connections = new ConnectionCollectionViewModel(UndoRedo, this);
            Grid = new GridManager();
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _statusTimer.Tick += (s, e) => RefreshTaskStatuses();
            _statusTimer.Start();

            Nodes.Nodes.CollectionChanged += OnNodesCollectionChanged;
            Connections.Connections.CollectionChanged += OnConnectionsCollectionChanged;

            UndoCommand = new RelayCommand(() =>
            {
                UndoRedo.Undo();
                RefreshNodeAndConnectionPositions();
            }, () => UndoRedo.CanUndo);

            RedoCommand = new RelayCommand(() =>
            {
                UndoRedo.Redo();
                RefreshNodeAndConnectionPositions();
            }, () => UndoRedo.CanRedo);
            MoveToHistoryCommand = new RelayCommand<UndoRedoManager.HistoryItem>(item => UndoRedo.MoveToHistory(item));

            UndoRedo.CurrentHistoryChanged += OnCurrentHistoryChanged;
        }
    }
}

/* --- End of file --- */
