using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノード(タスク)を表すViewModel。
    /// プロパティ編集、日時編集、Undo/Redo、ポート管理など
    /// ノードに関するすべてのロジックを担当する。
    /// </summary>
    public class NodeViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly UndoRedoManager _undoRedo;
        private readonly IDateTimeEditorService _dateTimeEditor;
        private readonly DispatcherTimer _editTimer;
        private readonly List<EditableField<string>> _editableFields;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードViewModelを生成する。
        /// Undo/Redo管理と日時編集サービスを受け取る。
        /// </summary>
        public NodeViewModel(UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _dateTimeEditor = dateTimeEditor;
            _nodeGuid = Guid.NewGuid();
            EditStartDateTimeCommand = new RelayCommand(() => EditDateTime(true));
            EditEndDateTimeCommand = new RelayCommand(() => EditDateTime(false));
            NotifyEditedCommand = new RelayCommand(() => NotifyEdited());

            /* 編集遅延コミット用タイマー(5秒間入力がなければ確定) */
            _editTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _editTimer.Tick += (s, e) =>
            {
                _editTimer.Stop();
                CommitEdits();
            };

            /* 遅延コミット対象フィールド */
            _editableFields =
            [
                new EditableField<string>("TaskName", () => TaskName, v => TaskName = v),
                new EditableField<string>("Person", () => Person, v => Person = v),
                new EditableField<string>("Comment", () => Comment, v => Comment = v)
           ];
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
         * Undo/Redo 履歴登録
         * --------------------------------------------------------- */

        /// <summary>
        /// 遅延コミットされたプロパティ変更をUndo/Redoに登録する。
        /// </summary>
        public void CommitHistory(string propertyName, object? oldValue, object? newValue)
        {
            if (!_undoRedo.IsApplyingHistory)
            {
                var action = new EditNodePropertyAction(this, propertyName, oldValue, newValue);
                _undoRedo.Execute(action);
            }
        }

        /* ---------------------------------------------------------
         * 日時編集
         * --------------------------------------------------------- */

        public ICommand EditStartDateTimeCommand { get; }
        public ICommand EditEndDateTimeCommand { get; }

        /// <summary>
        /// 日時編集ダイアログを開き、開始/終了日時を更新する。
        /// </summary>
        private void EditDateTime(bool isStart)
        {
            var initial = isStart ? StartDateTime : EndDateTime;
            var picked = _dateTimeEditor.EditDateTime(
                initial,
                isStart ? (Func<DateTime?, bool>)CanSetStartDateTime : CanSetEndDateTime
            );

            if (picked == initial)
            {
                return;
            }

            if (isStart)
            {
                if (!CanSetStartDateTime(picked))
                {
                    return;
                }
                StartDateTime = picked;
            }
            else
            {
                if (!CanSetEndDateTime(picked))
                {
                    return;
                }
                EndDateTime = picked;
            }
        }

        /// <summary>
        /// 開始日時が終了日時より後になってないかチェックする。
        /// </summary>
        public bool CanSetStartDateTime(DateTime? newStartDateTime)
        {
            if ((newStartDateTime == null) || (EndDateTime == null))
            {
                return true;
            }
            return newStartDateTime <= EndDateTime;
        }

        /// <summary>
        /// 終了日時が開始日時より前になってないかチェックする。
        /// </summary>
        public bool CanSetEndDateTime(DateTime? newEndDateTime)
        {
            if ((newEndDateTime == null) || (StartDateTime == null))
            {
                return true;
            }
            return StartDateTime <= newEndDateTime;
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

        private Guid _nodeGuid;

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

        private string? _taskName = $"(New Task)";

        [DisplayName("タスク名")]
        public string? TaskName
        {
            get => _taskName;
            set
            {
                if (_taskName == value)
                {
                    return;
                }
                _taskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        private string? _person;
        
        [DisplayName("担当者")]
        public string? Person
        {
            get => _person;
            set
            {
                if (_person == value)
                {
                    return;
                }
                _person = value;
                OnPropertyChanged(nameof(Person));
            }
        }

        private DateTime? _startDateTime;

        [DisplayName("開始日時")]
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set
            {
                if (_startDateTime == value)
                {
                    return;
                }
                if (!_undoRedo.IsApplyingHistory)
                {
                    _undoRedo.Execute(new EditNodePropertyAction(this, nameof(StartDateTime), _startDateTime, value));
                }
                _startDateTime = value;
                OnPropertyChanged(nameof(StartDateTime));
            }
        }

        private DateTime? _endDateTime;

        [DisplayName("終了日時")]
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set
            {
                if (_endDateTime == value)
                {
                    return;
                }
                if (!_undoRedo.IsApplyingHistory)
                {
                    _undoRedo.Execute(new EditNodePropertyAction(this, nameof(EndDateTime), _endDateTime, value));
                }
                _endDateTime = value;
                OnPropertyChanged(nameof(EndDateTime));
            }
        }

        private string? _comment;

        [DisplayName("コメント")]
        public string? Comment
        {
            get => _comment;
            set
            {
                if (_comment == value)
                {
                    return;
                }
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

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
         * 遅延コミット(テキスト編集)
         * --------------------------------------------------------- */

        /// <summary>編集通知コマンド(5秒後にCommitEditsが実行される)</summary>
        public ICommand NotifyEditedCommand { get; }

        /// <summary>
        /// 編集が行われたことを通知し、遅延コミットタイマーをリセットする。
        /// </summary>
        public void NotifyEdited()
        {
            _editTimer.Stop();
            _editTimer.Start();
        }

        /// <summary>
        /// 遅延コミット対象フィールドをチェックし、変更があれば Undo/Redoに登録する。
        /// </summary>
        public void CommitEdits()
        {
            foreach (var field in _editableFields)
            {
                field.TryCommit(CommitHistory);
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
