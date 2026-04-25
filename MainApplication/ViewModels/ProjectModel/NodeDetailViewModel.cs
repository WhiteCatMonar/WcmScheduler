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
    public class NodeDetailViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly UndoRedoManager _undoRedo;
        private readonly IDateTimeEditorService _dateTimeEditor;
        private const int WorkEstimateStepMinutes = 1;
        private const int WorkEstimateLargeStepMinutes = 60;

        private readonly DispatcherTimer _editTimer;
        private readonly List<IEditableField> _editableFields;

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
            IncreaseWorkEstimateMinutesCommand = new RelayCommand(IncreaseWorkEstimateMinutes);
            DecreaseWorkEstimateMinutesCommand = new RelayCommand(DecreaseWorkEstimateMinutes);
            IncreaseWorkEstimateHourCommand = new RelayCommand(IncreaseWorkEstimateHour);
            DecreaseWorkEstimateHourCommand = new RelayCommand(DecreaseWorkEstimateHour);
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
                new EditableField<string?>("TaskName", () => TaskName, v => TaskName = v),
                new EditableField<string?>("Person",   () => Person,   v => Person = v),
                new EditableField<string?>("Comment",  () => Comment,  v => Comment = v),
                new EditableField<int?>("WorkEstimateMinutes", () => WorkEstimateMinutes, v => WorkEstimateMinutes = v)
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
            set => SetProperty(ref _taskName, value);
        }

        private string? _person;
        [DisplayName("担当者")]
        public string? Person
        {
            get => _person;
            set => SetProperty(ref _person, value);
        }

        private string? _comment;
        [DisplayName("コメント")]
        public string? Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        private int? _workEstimateMinutes;
        [DisplayName("見積時間")]
        public int? WorkEstimateMinutes
        {
            get => _workEstimateMinutes;
            set => SetProperty(
                ref _workEstimateMinutes,
                NormalizeWorkEstimateMinutes(value),
                [nameof(WorkEstimateDisplayText)]
            );
        }

        /// <summary>
        /// 作業見積時間を時間と分に換算した表示文字列。
        /// </summary>
        public string WorkEstimateDisplayText => FormatWorkEstimate(WorkEstimateMinutes);

        private DateTime? _startDateTime;
        [DisplayName("開始日時")]
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set => SetProperty(
                ref _startDateTime,
                value,
                CreateHooksFromValue(
                    value,
                    pre: (oldValue, newValue) =>
                    {
                        if (!_undoRedo.IsApplyingHistory)
                        {
                            _undoRedo.Execute(
                                new EditNodeDetailPropertyAction(
                                    this,
                                    nameof(StartDateTime),
                                    _startDateTime,
                                    newValue
                                )
                            );
                        }
                    }
                )
            );
        }

        private DateTime? _endDateTime;
        [DisplayName("終了日時")]
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set => SetProperty(
                ref _endDateTime,
                value,
                CreateHooksFromValue(
                    value,
                    pre: (oldValue, newValue) =>
                    {
                        if (!_undoRedo.IsApplyingHistory)
                        {
                            _undoRedo.Execute(
                                new EditNodeDetailPropertyAction(
                                    this,
                                    nameof(EndDateTime),
                                    _endDateTime,
                                    newValue
                                )
                            );
                        }
                    }
                )
            );
        }

        /* ---------------------------------------------------------
         * 日時編集
         * --------------------------------------------------------- */

        public ICommand EditStartDateTimeCommand { get; }
        public ICommand EditEndDateTimeCommand { get; }
        public ICommand IncreaseWorkEstimateMinutesCommand { get; }
        public ICommand DecreaseWorkEstimateMinutesCommand { get; }
        public ICommand IncreaseWorkEstimateHourCommand { get; }
        public ICommand DecreaseWorkEstimateHourCommand { get; }

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
         * 作業見積時間編集
         * --------------------------------------------------------- */

        /// <summary>
        /// 作業見積時間を1ステップ増加させる。
        /// </summary>
        private void IncreaseWorkEstimateMinutes()
        {
            AddWorkEstimateMinutes(WorkEstimateStepMinutes);
        }

        /// <summary>
        /// 作業見積時間を1ステップ減少させる。
        /// </summary>
        private void DecreaseWorkEstimateMinutes()
        {
            AddWorkEstimateMinutes(-WorkEstimateStepMinutes);
        }

        /// <summary>
        /// 作業見積時間を60分増加させる。
        /// </summary>
        private void IncreaseWorkEstimateHour()
        {
            AddWorkEstimateMinutes(WorkEstimateLargeStepMinutes);
        }

        /// <summary>
        /// 作業見積時間を60分減少させる。
        /// </summary>
        private void DecreaseWorkEstimateHour()
        {
            AddWorkEstimateMinutes(-WorkEstimateLargeStepMinutes);
        }

        /// <summary>
        /// 作業見積時間へ指定分数を加算する。
        /// </summary>
        /// <param name="deltaMinutes">加算する分数。</param>
        private void AddWorkEstimateMinutes(int deltaMinutes)
        {
            var current = WorkEstimateMinutes ?? 0;
            WorkEstimateMinutes = Math.Max(0, current + deltaMinutes);
            NotifyEdited();
        }

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

        /// <summary>
        /// 作業見積時間を保存可能な値に正規化する。
        /// </summary>
        /// <param name="value">入力された作業見積時間。</param>
        /// <returns>正規化後の作業見積時間。</returns>
        private static int? NormalizeWorkEstimateMinutes(int? value)
        {
            if (value is null or < 0)
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// 分単位の作業見積時間を表示文字列へ変換する。
        /// </summary>
        /// <param name="minutes">分単位の作業見積時間。</param>
        /// <returns>時間と分の表示文字列。</returns>
        private static string FormatWorkEstimate(int? minutes)
        {
            if (minutes == null)
            {
                return "";
            }

            var hours = minutes.Value / 60;
            var remainingMinutes = minutes.Value % 60;
            return $"{hours}h{remainingMinutes}m";
        }
    }
}

/* --- End of file --- */
