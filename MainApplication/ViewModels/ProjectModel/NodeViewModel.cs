using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノード(タスク)を表すViewModel。
    /// プロパティ編集、日時編集、Undo/Redo、ポート管理など
    /// ノードに関するすべてのロジックを担当する。
    /// </summary>
    /// <param name="undoRedo">Undo/Redo管理用オブジェクト</param>
    /// <param name="dateTimeEditor">ノードが利用する時刻編集サービス</param>
    public class NodeViewModel(UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor) : INotifyPropertyChanged
    {
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
            set
            {
                if (_nodeType == value)
                {
                    return;
                }
                _nodeType = value;
                OnPropertyChanged(nameof(NodeType));
            }
        }

        /* ---------------------------------------------------------
         * ノードのサイズ
         * --------------------------------------------------------- */
        public static readonly double _minWidth = 100;

        /// <summary>ノードの最小幅</summary>
        public static double MinWidth
        {
            get => _minWidth;
        }

        public static readonly double _minHeight = 60;

        /// <summary>ノードの最小高さ</summary>
        public static double MinHeight
        {
            get => _minHeight;
        }

        private double _width = _minWidth;

        /// <summary>ノードの幅</summary>
        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _height = _minHeight;

        /// <summary>ノードの高さ</summary>
        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        /* ---------------------------------------------------------
         * 選択状態
         * --------------------------------------------------------- */

        private bool _isSelected;

        /// <summary>ノードが選択されているかどうか</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        /* ---------------------------------------------------------
         * ノード固有情報
         * --------------------------------------------------------- */

        private Guid _nodeGuid = Guid.NewGuid();

        [DisplayName("タスクID")]
        public Guid NodeGuid
        {
            get => _nodeGuid;
            set
            {
                if (_nodeGuid == value)
                {
                    return;
                }
                _nodeGuid = value;
                OnPropertyChanged(nameof(NodeGuid));
            }
        }

        /* その他ノード固有詳細情報 */
        public NodeDetailViewModel Detail { get; } = new(undoRedo, dateTimeEditor);

        /* ---------------------------------------------------------
         * ノード位置
         * --------------------------------------------------------- */

        private double _x;

        /// <summary>ノードのX座標(論理座標)</summary>
        public double X
        {
            get => _x;
            set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        private double _y;

        /// <summary>ノードのY座標(論理座標)</summary>
        public double Y
        {
            get => _y;
            set
            {
                if (_y == value)
                {
                    return;
                }
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
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
            foreach (var port in InputPorts)
            {
                port.UpdateAbsolutePosition();
            }

            foreach (var port in OutputPorts)
            {
                port.UpdateAbsolutePosition();
            }
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する。
        /// </summary>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
