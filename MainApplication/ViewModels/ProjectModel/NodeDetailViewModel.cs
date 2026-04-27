using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
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
            AddSuspensionPeriodCommand = new RelayCommand(AddSuspensionPeriod);
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

            SuspensionPeriods.CollectionChanged += OnSuspensionPeriodsChanged;
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

        /// <summary>
        /// 中断期間一覧。
        /// </summary>
        public ObservableCollection<SuspensionPeriodViewModel> SuspensionPeriods { get; } = [];

        /// <summary>
        /// 中断期間に重複区間があるかどうか。
        /// </summary>
        public bool HasOverlappingSuspensionPeriods => ContainsMergeableSuspensionPeriods(false);

        /// <summary>
        /// 中断期間に重複または連続区間があるかどうか。
        /// </summary>
        public bool HasMergeableSuspensionPeriods => ContainsMergeableSuspensionPeriods(true);

        /// <summary>
        /// 中断期間の補助表示文。
        /// </summary>
        public string SuspensionPeriodWarningText
        {
            get
            {
                if (HasOverlappingSuspensionPeriods)
                {
                    return "中断期間に重複があります。ガントチャート算定時に統合されます。";
                }

                if (HasMergeableSuspensionPeriods)
                {
                    return "連続する中断期間があります。ガントチャート算定時に統合されます。";
                }

                return "";
            }
        }

        /// <summary>
        /// ガントチャート算定用に正規化した中断期間一覧。
        /// </summary>
        public IReadOnlyList<SuspensionPeriodRange> NormalizedSuspensionPeriods => GetNormalizedSuspensionPeriods();

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
        public ICommand AddSuspensionPeriodCommand { get; }

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
         * 中断期間編集
         * --------------------------------------------------------- */

        /// <summary>
        /// 中断期間を追加する。
        /// </summary>
        private void AddSuspensionPeriod()
        {
            var period = CreateSuspensionPeriod(null, null);
            _undoRedo.Execute(
                new AddSuspensionPeriodAction(this, period, SuspensionPeriods.Count)
            );
        }

        /// <summary>
        /// 中断期間ViewModelを生成する。
        /// </summary>
        /// <param name="startDateTime">中断開始日時。</param>
        /// <param name="endDateTime">中断終了日時。</param>
        /// <returns>中断期間ViewModel。</returns>
        public SuspensionPeriodViewModel CreateSuspensionPeriod(DateTime? startDateTime, DateTime? endDateTime)
        {
            return new SuspensionPeriodViewModel(this, _undoRedo, _dateTimeEditor, startDateTime, endDateTime);
        }

        /// <summary>
        /// 中断期間を削除する。
        /// </summary>
        /// <param name="period">削除対象の中断期間。</param>
        public void DeleteSuspensionPeriod(SuspensionPeriodViewModel period)
        {
            var index = SuspensionPeriods.IndexOf(period);
            if (index < 0)
            {
                return;
            }

            _undoRedo.Execute(
                new DeleteSuspensionPeriodAction(this, period, index)
            );
        }

        /// <summary>
        /// 中断期間を指定位置に追加する。
        /// </summary>
        /// <param name="period">追加する中断期間。</param>
        /// <param name="index">追加位置。</param>
        public void InsertSuspensionPeriodDirect(SuspensionPeriodViewModel period, int index)
        {
            if (SuspensionPeriods.Contains(period))
            {
                return;
            }

            if (index < 0 || index > SuspensionPeriods.Count)
            {
                SuspensionPeriods.Add(period);
                return;
            }

            SuspensionPeriods.Insert(index, period);
        }

        /// <summary>
        /// 中断期間を削除する。
        /// </summary>
        /// <param name="period">削除対象の中断期間。</param>
        public void RemoveSuspensionPeriodDirect(SuspensionPeriodViewModel period)
        {
            SuspensionPeriods.Remove(period);
        }

        /// <summary>
        /// 指定した中断期間の開始日時が他の中断期間と重複しているかどうかを判定する。
        /// </summary>
        /// <param name="target">判定対象の中断期間。</param>
        /// <returns>開始日時が他の中断期間内にある場合はtrue。</returns>
        public bool IsSuspensionPeriodStartOverlapping(SuspensionPeriodViewModel target)
        {
            return IsDateTimeOverlapping(target, target.StartDateTime, true);
        }

        /// <summary>
        /// 指定した中断期間の終了日時が他の中断期間と重複しているかどうかを判定する。
        /// </summary>
        /// <param name="target">判定対象の中断期間。</param>
        /// <returns>終了日時が他の中断期間内にある場合はtrue。</returns>
        public bool IsSuspensionPeriodEndOverlapping(SuspensionPeriodViewModel target)
        {
            return IsDateTimeOverlapping(target, target.EndDateTime, false);
        }

        /// <summary>
        /// 中断期間一覧変更時に監視対象を更新する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnSuspensionPeriodsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SuspensionPeriodViewModel period in e.OldItems)
                {
                    period.PropertyChanged -= OnSuspensionPeriodPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (SuspensionPeriodViewModel period in e.NewItems)
                {
                    period.PropertyChanged += OnSuspensionPeriodPropertyChanged;
                }
            }

            NotifySuspensionPeriodStateChanged();
        }

        /// <summary>
        /// 中断期間の日時変更時に正規化状態を更新する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnSuspensionPeriodPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(SuspensionPeriodViewModel.StartDateTime) or nameof(SuspensionPeriodViewModel.EndDateTime))
            {
                NotifySuspensionPeriodStateChanged();
            }
        }

        /// <summary>
        /// 中断期間の検出結果と正規化結果の変更通知を行う。
        /// </summary>
        private void NotifySuspensionPeriodStateChanged()
        {
            OnPropertyChangedA(nameof(HasOverlappingSuspensionPeriods));
            OnPropertyChangedA(nameof(HasMergeableSuspensionPeriods));
            OnPropertyChangedA(nameof(SuspensionPeriodWarningText));
            OnPropertyChangedA(nameof(NormalizedSuspensionPeriods));

            foreach (var period in SuspensionPeriods)
            {
                period.NotifyNormalizationStateChanged();
            }
        }

        /// <summary>
        /// 指定日時が他の中断期間内に含まれるかどうかを判定する。
        /// </summary>
        /// <param name="target">判定対象の中断期間。</param>
        /// <param name="dateTime">判定対象日時。</param>
        /// <param name="isStartDateTime">開始日時として判定する場合はtrue。</param>
        /// <returns>他の中断期間内に含まれる場合はtrue。</returns>
        private bool IsDateTimeOverlapping(SuspensionPeriodViewModel target, DateTime? dateTime, bool isStartDateTime)
        {
            if (dateTime == null || target.StartDateTime == null || target.EndDateTime == null)
            {
                return false;
            }

            foreach (var period in SuspensionPeriods)
            {
                if (ReferenceEquals(period, target))
                {
                    continue;
                }

                if (period.StartDateTime == null || period.EndDateTime == null)
                {
                    continue;
                }

                if (isStartDateTime)
                {
                    if (period.StartDateTime <= dateTime && dateTime < period.EndDateTime)
                    {
                        return true;
                    }
                }
                else if (period.StartDateTime < dateTime && dateTime <= period.EndDateTime)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 重複または連続して統合可能な中断期間があるかどうかを判定する。
        /// </summary>
        /// <param name="includeAdjacent">連続区間を統合可能として扱う場合はtrue。</param>
        /// <returns>統合可能な中断期間がある場合はtrue。</returns>
        private bool ContainsMergeableSuspensionPeriods(bool includeAdjacent)
        {
            var periods = GetCompleteSuspensionPeriods();

            for (var i = 1; i < periods.Count; i++)
            {
                if (includeAdjacent)
                {
                    if (periods[i].StartDateTime <= periods[i - 1].EndDateTime)
                    {
                        return true;
                    }
                }
                else if (periods[i].StartDateTime < periods[i - 1].EndDateTime)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ガントチャート算定用に中断期間の重複と連続区間を統合する。
        /// </summary>
        /// <returns>正規化済み中断期間一覧。</returns>
        private IReadOnlyList<SuspensionPeriodRange> GetNormalizedSuspensionPeriods()
        {
            var periods = GetCompleteSuspensionPeriods();
            var normalized = new List<SuspensionPeriodRange>();

            foreach (var period in periods)
            {
                if (normalized.Count == 0)
                {
                    normalized.Add(period);
                    continue;
                }

                var last = normalized[^1];
                if (period.StartDateTime <= last.EndDateTime)
                {
                    normalized[^1] = new SuspensionPeriodRange(
                        last.StartDateTime,
                        period.EndDateTime > last.EndDateTime ? period.EndDateTime : last.EndDateTime
                    );
                    continue;
                }

                normalized.Add(period);
            }

            return normalized;
        }

        /// <summary>
        /// 開始日時と終了日時が設定済みの中断期間を開始日時順に取得する。
        /// </summary>
        /// <returns>入力済み中断期間一覧。</returns>
        private List<SuspensionPeriodRange> GetCompleteSuspensionPeriods()
        {
            return SuspensionPeriods
                .Where(period => period.StartDateTime != null && period.EndDateTime != null)
                .Select(period => new SuspensionPeriodRange(period.StartDateTime!.Value, period.EndDateTime!.Value))
                .OrderBy(period => period.StartDateTime)
                .ThenBy(period => period.EndDateTime)
                .ToList();
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
