using MainApplication.Models.SaveData;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using MainApplication.Mappers;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノードエディタ全体を統括するViewModel。
    /// UI状態、ノード・接続線管理、Undo/Redo、データ入出力などを扱う。
    /// </summary>
    public class NodeEditorViewModel : ViewModelBase
    {
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
         * GridManager(論理座標系の中枢)
         * --------------------------------------------------------- */

        /// <summary>
        /// ズーム・パン・論理座標系を管理するGridManager
        /// </summary>
        public GridManager Grid { get; }

        /* ---------------------------------------------------------
         * ノード・接続線管理
         * --------------------------------------------------------- */
        
        /// <summary>ノード一覧管理</summary>
        public NodeCollectionViewModel Nodes { get; }

        /// <summary>接続線一覧管理</summary>
        public ConnectionCollectionViewModel Connections { get; }

        /// <summary>
        /// ノード・接続線の位置を再計算する。
        /// Undo/Redo やズーム変更後に使用。
        /// </summary>
        private void RefreshNodeAndConnectionPositions()
        {
            Nodes.UpdateAllNodes();
            Connections.UpdateAllConnections();
        }

        /// <summary>
        /// 選択中ノードの編集内容を確定する。
        /// </summary>
        public void CommitCurrentNodeEdits()
        {
            Nodes.SelectedNode?.Detail.CommitEdits();
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
            NodeEditorViewModel loadedData = NodeEditorMapper.ToViewModel(data, this);


            foreach (var loadedNodes in loadedData.Nodes.Nodes)
            {
                Nodes.Nodes.Add(loadedNodes);
            }

            foreach (var loadedConnections in loadedData.Connections.Connections)
            {
                Connections.Connections.Add(loadedConnections);
            }

            RefreshNodeAndConnectionPositions();

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

        /* ---------------------------------------------------------
         * GridManagerにUI状態を反映
         * --------------------------------------------------------- */

        /// <summary>
        /// ズーム・パン・キャンバスサイズをGridManagerに反映し、
        /// グリッド線を更新する。
        /// </summary>
        public void UpdateGridState()
        {
            /* ズーム・パン */
            Grid.Zoom = Zoom;
            Grid.Pan  = Pan;

            /* 論理座標系のサイズ */
            Grid.CanvasViewLogicalWidth = BaseCanvasWidth / Zoom;
            Grid.CanvasViewLogicalHeight = BaseCanvasHeight / Zoom;

            /* 論理原点 */
            Grid.CanvasViewOrigin = Pan.MirrorPoint().Div(Zoom);

            /* グリッド線更新 */
            Grid.UpdateGrid();
        }
    }
}

/* --- End of file --- */
