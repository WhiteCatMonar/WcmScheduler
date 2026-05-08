using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MainApplication.ViewModels.DependencyEditorModel
{
    /// <summary>
    /// ノード(タスク)を表すViewModel。
    /// プロパティ編集、日時編集、Undo/Redo、ポート管理など
    /// ノードに関するすべてのロジックを担当する。
    /// </summary>
    /// <param name="undoRedo">Undo/Redo管理用オブジェクト</param>
    /// <param name="dateTimeEditor">ノードが利用する時刻編集サービス</param>
    public class TaskNodeViewModel(UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor) : ViewModelBase
    {
        /// <summary>
        /// タスクの状態。
        /// </summary>
        public enum TaskStatus
        {
            Ready,
            Pending,
            InProgress,
            Done
        }

        /* ---------------------------------------------------------
         * ノード種別
         * --------------------------------------------------------- */

        private string _nodeType = "TaskNode";

        /// <summary>
        /// ノードの種類
        /// </summary>
        public string NodeType
        {
            get => _nodeType;
            set => SetProperty(ref _nodeType, value);
        }

        /* ---------------------------------------------------------
         * ノードのサイズ
         * --------------------------------------------------------- */
        private const double MinWidthValue = 100;

        /// <summary>ノードの最小幅</summary>
        public static double MinWidth
        {
            get => MinWidthValue;
        }

        private const double MinHeightValue = 60;

        /// <summary>ノードの最小高さ</summary>
        public static double MinHeight
        {
            get => MinHeightValue;
        }

        private double _width = MinWidthValue;

        /// <summary>ノードの幅</summary>
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height = MinHeightValue;

        /// <summary>ノードの高さ</summary>
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /* ---------------------------------------------------------
         * 選択状態
         * --------------------------------------------------------- */

        private bool _isSelected;

        /// <summary>ノードが選択されているかどうか</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private TaskStatus _status = TaskStatus.Ready;

        /// <summary>
        /// 現在のタスク状態。
        /// </summary>
        public TaskStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /* ---------------------------------------------------------
         * ノード固有情報
         * --------------------------------------------------------- */

        private Guid _nodeGuid = Guid.NewGuid();

        [DisplayName("タスクID")]
        public Guid NodeGuid
        {
            get => _nodeGuid;
            set => SetProperty(ref _nodeGuid, value);
        }

        /* その他ノード固有詳細情報 */
        public TaskDetailViewModel Detail { get; } = new(undoRedo, dateTimeEditor);

        /* ---------------------------------------------------------
         * ノード位置
         * --------------------------------------------------------- */

        private Point _position;
        public Point Position
        {
            get => _position;
            set => SetProperty(
                ref _position,
                value,
                CreateHooksFromValue(
                    value,
                    chain: () => UpdateAllPortPositions()
                )
            );
        }


        /* ---------------------------------------------------------
         * ポート管理
         * --------------------------------------------------------- */

        /// <summary>入力ポート一覧</summary>
        public ObservableCollection<PortViewModel> InputPorts { get; } = [];

        /// <summary>出力ポート一覧</summary>
        public ObservableCollection<PortViewModel> OutputPorts { get; } = [];

        /// <summary>すべてのポート</summary>
        public IEnumerable<PortViewModel> AllPorts => InputPorts.Concat(OutputPorts);

        /// <summary>
        /// ノードの位置変更に伴い、すべてのポートの絶対座標を更新する。
        /// </summary>
        public void UpdateAllPortPositions()
        {
            foreach (var port in AllPorts)
            {
                port.ControlPoint = Position;
            }
        }
    }
}

/* --- End of file --- */
