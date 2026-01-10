using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノードの詳細情報を管理するViewModel。
    /// タスク名・担当者・コメント・日時などの編集と、
    /// Undo/Redo・遅延コミットを担当する。
    /// </summary>
    public class NodeDetailViewModel : INotifyPropertyChanged
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
        /// ノード詳細ViewModelを生成する。
        /// 親ノード・Undo/Redo管理・日時編集サービスを受け取る。
        /// </summary>
        /// <param name="undoRedo">Undo/Redo管理用オブジェクト</param>
        /// <param name="dateTimeEditor">ノード詳細ViewModelが利用する時刻編集サービス</param>
        public NodeDetailViewModel(
            UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor
        )
        {
            _undoRedo = undoRedo;
            _dateTimeEditor = dateTimeEditor;

            EditStartDateTimeCommand = new RelayCommand(() => EditDateTime(true));
            EditEndDateTimeCommand = new RelayCommand(() => EditDateTime(false));
            NotifyEditedCommand = new RelayCommand(() => NotifyEdited());

            /* 編集遅延コミット用タイマー */
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
                new EditableField<string>("Person",   () => Person,   v => Person = v),
                new EditableField<string>("Comment",  () => Comment,  v => Comment = v)
            ];
        }

        /* ---------------------------------------------------------
         * ノード固有情報(編集対象)
         * --------------------------------------------------------- */

        private string? _taskName = $"(New Task)";
        [DisplayName("タスク名")]
        public string? TaskName
        {
            get => _taskName;
            set
            {
                if (_taskName != value)
                {
                    _taskName = value;
                    OnPropertyChanged(nameof(TaskName));
                }
            }
        }

        private string? _person;
        [DisplayName("担当者")]
        public string? Person
        {
            get => _person;
            set
            {
                if (_person != value)
                {
                    _person = value;
                    OnPropertyChanged(nameof(Person));
                }
            }
        }

        private string? _comment;
        [DisplayName("コメント")]
        public string? Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        private DateTime? _startDateTime;
        [DisplayName("開始日時")]
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set
            {
                if (_startDateTime != value)
                {
                    if (!_undoRedo.IsApplyingHistory)
                    {
                        _undoRedo.Execute(
                            new EditNodeDetailPropertyAction(
                                this,
                                nameof(StartDateTime),
                                _startDateTime,
                                value
                            )
                        );
                    }

                    _startDateTime = value;
                    OnPropertyChanged(nameof(StartDateTime));
                }
            }
        }

        private DateTime? _endDateTime;
        [DisplayName("終了日時")]
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set
            {
                if (_endDateTime != value)
                {
                    if (!_undoRedo.IsApplyingHistory)
                    {
                        _undoRedo.Execute(
                            new EditNodeDetailPropertyAction(
                                this,
                                nameof(EndDateTime),
                                _endDateTime,
                                value
                            )
                        );
                    }

                    _endDateTime = value;
                    OnPropertyChanged(nameof(EndDateTime));
                }
            }
        }

        /* ---------------------------------------------------------
         * 日時編集
         * --------------------------------------------------------- */

        public ICommand EditStartDateTimeCommand { get; }
        public ICommand EditEndDateTimeCommand { get; }

        private void EditDateTime(bool isStart)
        {
            var initial = isStart ? StartDateTime : EndDateTime;
            var picked = _dateTimeEditor.EditDateTime(
                initial,
                isStart ? CanSetStartDateTime : CanSetEndDateTime
            );

            if (picked == initial)
            {
                return;
            }

            if (isStart)
            {
                if (CanSetStartDateTime(picked))
                {
                    StartDateTime = picked;
                }
            }
            else
            {
                if (CanSetEndDateTime(picked))
                {
                    EndDateTime = picked;
                }
            }
        }

        public bool CanSetStartDateTime(DateTime? newStart) =>
            (newStart == null) || (EndDateTime == null) || (newStart <= EndDateTime);

        public bool CanSetEndDateTime(DateTime? newEnd) =>
            (newEnd == null) || (StartDateTime == null) || (StartDateTime <= newEnd);

        /* ---------------------------------------------------------
         * 遅延コミット
         * --------------------------------------------------------- */

        public ICommand NotifyEditedCommand { get; }

        public void NotifyEdited()
        {
            _editTimer.Stop();
            _editTimer.Start();
        }

        public void CommitEdits()
        {
            foreach (var field in _editableFields)
            {
                field.TryCommit(CommitHistory);
            }
        }

        public void CommitHistory(string propertyName, object? oldValue, object? newValue)
        {
            if (!_undoRedo.IsApplyingHistory)
            {
                _undoRedo.Execute(
                    new EditNodeDetailPropertyAction(this, propertyName, oldValue, newValue)
                );
            }
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/* --- End of file --- */
