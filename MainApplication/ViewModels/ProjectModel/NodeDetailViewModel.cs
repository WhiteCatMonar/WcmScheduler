using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using MainApplication.ViewModels.TeamModel;
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
        private ObservableCollection<TeamMemberViewModel>? _members;
        private bool _isRefreshingMemberOptions;
        private bool _isRefreshingCollaboratorSelections;
        private bool _isUpdatingCollaboratorSelectionValue;

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
            AddCollaboratorCommand = new RelayCommand(AddCollaborator);
            NotifyEditedCommand = new RelayCommand(() => NotifyEdited());
            CommitEditsCommand = new RelayCommand(CommitEdits);

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

        private Guid? _assigneeMemberId;
        [DisplayName("担当者")]
        public Guid? AssigneeMemberId
        {
            get => _assigneeMemberId;
            set
            {
                if (_isRefreshingMemberOptions && value == null && _assigneeMemberId != null)
                {
                    return;
                }

                SetProperty(
                    ref _assigneeMemberId,
                    value,
                    [
                        nameof(HasInvalidAssigneeMember),
                        nameof(AssigneeWarningText),
                        nameof(AssigneeDisplayName),
                        nameof(AssigneeInitials),
                        nameof(HasAssigneeBadge)
                    ],
                    CreateHooksFromValue(
                        value,
                        pre: (oldValue, newValue) =>
                        {
                            if (!_undoRedo.IsApplyingHistory)
                            {
                                _undoRedo.Execute(
                                    new EditNodeDetailPropertyAction(
                                        this,
                                        nameof(AssigneeMemberId),
                                        _assigneeMemberId,
                                        newValue
                                    )
                                );
                            }
                        },
                        chain: OnAssigneeMemberChanged
                    )
                );
            }
        }

        private List<Guid> _collaboratorMemberIds = [];
        [DisplayName("作業協力者")]
        public List<Guid> CollaboratorMemberIds
        {
            get => _collaboratorMemberIds;
            set => SetProperty(
                ref _collaboratorMemberIds,
                NormalizeCollaboratorMemberIds(value),
                [
                    nameof(HasInvalidCollaboratorMember),
                    nameof(CollaboratorWarningText)
                ],
                CreateHooksFromValue(
                    value,
                    chain: OnCollaboratorMemberIdsChanged
                )
            );
        }

        /// <summary>
        /// 担当者選択肢。
        /// </summary>
        public ObservableCollection<MemberOptionViewModel> AssigneeOptions { get; } = [];

        /// <summary>
        /// 作業協力者選択肢。
        /// </summary>
        public ObservableCollection<CollaboratorOptionViewModel> CollaboratorOptions { get; } = [];

        /// <summary>
        /// 作業協力者の選択行一覧。
        /// </summary>
        public ObservableCollection<CollaboratorSelectionViewModel> CollaboratorSelections { get; } = [];

        /// <summary>
        /// 担当者が無効または存在しないメンバーを参照しているかどうか。
        /// </summary>
        public bool HasInvalidAssigneeMember =>
            AssigneeMemberId != null && !IsActiveMember(AssigneeMemberId.Value);

        /// <summary>
        /// 作業協力者が無効または存在しないメンバーを参照しているかどうか。
        /// </summary>
        public bool HasInvalidCollaboratorMember =>
            CollaboratorMemberIds.Any(memberId => !IsActiveMember(memberId));

        /// <summary>
        /// 担当者警告表示文字列。
        /// </summary>
        public string AssigneeWarningText => HasInvalidAssigneeMember
            ? "担当者が無効または存在しないメンバーを参照しています。"
            : "";

        /// <summary>
        /// 担当者表示名
        /// </summary>
        public string AssigneeDisplayName => ResolveAssigneeMember()?.DisplayText ?? "(未担当)";

        /// <summary>
        /// 担当者バッジ表示文字列
        /// </summary>
        public string AssigneeInitials => ResolveAssigneeMember()?.Initials ?? "";

        /// <summary>
        /// 担当者バッジを表示するかどうか
        /// </summary>
        public bool HasAssigneeBadge => !string.IsNullOrWhiteSpace(AssigneeInitials);

        /// <summary>
        /// 作業協力者警告表示文字列。
        /// </summary>
        public string CollaboratorWarningText => HasInvalidCollaboratorMember
            ? "協力者が無効または存在しないメンバーを参照しています。"
            : "";

        /// <summary>
        /// メンバー選択に使用するチームメンバー一覧を設定する。
        /// </summary>
        /// <param name="members">チームメンバー一覧。</param>
        public void SetMembers(ObservableCollection<TeamMemberViewModel> members)
        {
            if (_members != null)
            {
                _members.CollectionChanged -= OnMembersCollectionChanged;
                foreach (var member in _members)
                {
                    member.PropertyChanged -= OnMemberPropertyChanged;
                }
            }

            _members = members;
            _members.CollectionChanged += OnMembersCollectionChanged;
            foreach (var member in _members)
            {
                member.PropertyChanged += OnMemberPropertyChanged;
            }

            RefreshMemberOptions();
        }

        /// <summary>
        /// 担当者変更に伴い、担当者と重複する作業協力者を除外する。
        /// </summary>
        private void OnAssigneeMemberChanged()
        {
            SetCollaboratorMemberIdsWithHistory(
                CollaboratorMemberIds.ToList(),
                CollaboratorMemberIds.ToList()
            );
            RefreshCollaboratorSelections();
        }

        /// <summary>
        /// 作業協力者を追加する。
        /// </summary>
        private void AddCollaborator()
        {
            var nextMemberId = GetNextAvailableCollaboratorMemberId();
            if (nextMemberId == null)
            {
                return;
            }

            var oldValue = CollaboratorMemberIds.ToList();
            var newValue = CollaboratorMemberIds.Append(nextMemberId.Value).ToList();
            SetCollaboratorMemberIdsWithHistory(oldValue, newValue);
        }

        /// <summary>
        /// 作業協力者選択行を削除する。
        /// </summary>
        /// <param name="selection">削除対象の選択行。</param>
        private void RemoveCollaborator(CollaboratorSelectionViewModel selection)
        {
            var oldValue = CollaboratorMemberIds.ToList();
            var newValue = CollaboratorSelections
                .Where(item => !ReferenceEquals(item, selection))
                .Select(item => item.SelectedMemberId)
                .Where(memberId => memberId != null && memberId != Guid.Empty)
                .Select(memberId => memberId!.Value)
                .Distinct()
                .ToList();
            if (oldValue.SequenceEqual(newValue))
            {
                CollaboratorSelections.Remove(selection);
                return;
            }

            SetCollaboratorMemberIdsWithHistory(oldValue, newValue);
        }

        /// <summary>
        /// 作業協力者選択行の選択値を保存値へ反映する。
        /// </summary>
        private void UpdateCollaboratorMemberIdsFromSelections()
        {
            if (_isRefreshingCollaboratorSelections)
            {
                return;
            }

            var oldValue = CollaboratorMemberIds.ToList();
            var newValue = CollaboratorSelections
                .Select(item => item.SelectedMemberId)
                .Where(memberId => memberId != null && memberId != Guid.Empty)
                .Select(memberId => memberId!.Value)
                .Distinct()
                .ToList();
            _isUpdatingCollaboratorSelectionValue = true;
            try
            {
                SetCollaboratorMemberIdsWithHistory(oldValue, newValue);
            }
            finally
            {
                _isUpdatingCollaboratorSelectionValue = false;
            }
        }

        /// <summary>
        /// 作業協力者ID一覧を履歴対象として更新する。
        /// </summary>
        /// <param name="oldValue">変更前の値。</param>
        /// <param name="newValue">変更後の値。</param>
        private void SetCollaboratorMemberIdsWithHistory(List<Guid> oldValue, List<Guid> newValue)
        {
            newValue = NormalizeCollaboratorMemberIds(newValue);
            if (oldValue.SequenceEqual(newValue))
            {
                RefreshCollaboratorSelections();
                return;
            }

            if (_undoRedo.IsApplyingHistory)
            {
                CollaboratorMemberIds = newValue;
                return;
            }

            _undoRedo.Execute(
                new EditNodeDetailPropertyAction(
                    this,
                    nameof(CollaboratorMemberIds),
                    oldValue,
                    newValue
                )
            );
        }

        /// <summary>
        /// 作業協力者の選択状態を更新する。
        /// </summary>
        /// <param name="option">変更された選択肢。</param>
        /// <param name="isSelected">選択済みにする場合はtrue。</param>
        private void SetCollaboratorSelected(CollaboratorOptionViewModel option, bool isSelected)
        {
            var oldValue = CollaboratorMemberIds.ToList();
            var newValue = CollaboratorMemberIds.ToList();

            if (isSelected)
            {
                if (!newValue.Contains(option.MemberId))
                {
                    newValue.Add(option.MemberId);
                }
            }
            else
            {
                newValue.Remove(option.MemberId);
            }

            if (oldValue.SequenceEqual(newValue))
            {
                return;
            }

            if (_undoRedo.IsApplyingHistory)
            {
                CollaboratorMemberIds = newValue;
                return;
            }

            _undoRedo.Execute(
                new EditNodeDetailPropertyAction(
                    this,
                    nameof(CollaboratorMemberIds),
                    oldValue,
                    newValue
                )
            );
        }

        /// <summary>
        /// メンバー一覧変更時に選択肢を更新する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnMembersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TeamMemberViewModel member in e.OldItems)
                {
                    member.PropertyChanged -= OnMemberPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (TeamMemberViewModel member in e.NewItems)
                {
                    member.PropertyChanged += OnMemberPropertyChanged;
                }
            }

            RefreshMemberOptions();
        }

        /// <summary>
        /// メンバー情報変更時に選択肢を更新する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RefreshMemberOptions();
        }

        /// <summary>
        /// 担当者と協力者の選択肢を更新する。
        /// </summary>
        private void RefreshMemberOptions()
        {
            _isRefreshingMemberOptions = true;
            try
            {
                RefreshAssigneeOptions();
                RefreshCollaboratorOptions();
                RefreshCollaboratorSelections();
                OnPropertyChangedA(nameof(HasInvalidAssigneeMember));
                OnPropertyChangedA(nameof(HasInvalidCollaboratorMember));
                OnPropertyChangedA(nameof(AssigneeWarningText));
                OnPropertyChangedA(nameof(CollaboratorWarningText));
                OnPropertyChangedA(nameof(AssigneeDisplayName));
                OnPropertyChangedA(nameof(AssigneeInitials));
                OnPropertyChangedA(nameof(HasAssigneeBadge));
            }
            finally
            {
                _isRefreshingMemberOptions = false;
            }
        }

        /// <summary>
        /// 担当者選択肢を更新する。
        /// </summary>
        private void RefreshAssigneeOptions()
        {
            AssigneeOptions.Clear();
            AssigneeOptions.Add(new MemberOptionViewModel());

            foreach (var member in GetSelectableMembers(AssigneeMemberId))
            {
                AssigneeOptions.Add(new MemberOptionViewModel(member));
            }
        }

        /// <summary>
        /// 協力者選択肢を更新する。
        /// </summary>
        private void RefreshCollaboratorOptions()
        {
            CollaboratorOptions.Clear();

            foreach (var member in GetSelectableMembers(null, CollaboratorMemberIds))
            {
                CollaboratorOptions.Add(
                    new CollaboratorOptionViewModel(
                        member,
                        CollaboratorMemberIds.Contains(member.MemberId),
                        SetCollaboratorSelected
                    )
                );
            }
        }

        /// <summary>
        /// 作業協力者の選択行を更新する。
        /// </summary>
        private void RefreshCollaboratorSelections()
        {
            _isRefreshingCollaboratorSelections = true;
            CollaboratorSelections.Clear();

            foreach (var memberId in CollaboratorMemberIds)
            {
                CollaboratorSelections.Add(
                    new CollaboratorSelectionViewModel(
                        CreateCollaboratorOptions(memberId),
                        memberId == Guid.Empty ? null : memberId,
                        UpdateCollaboratorMemberIdsFromSelections,
                        RemoveCollaborator
                    )
                );
            }

            _isRefreshingCollaboratorSelections = false;
        }

        /// <summary>
        /// 作業協力者ID一覧の変更後に協力者選択行を更新する。
        /// </summary>
        private void OnCollaboratorMemberIdsChanged()
        {
            if (_isUpdatingCollaboratorSelectionValue)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(RefreshCollaboratorSelections);
                return;
            }

            RefreshCollaboratorSelections();
        }

        /// <summary>
        /// 次に追加できる作業協力者のメンバーIDを取得する。
        /// </summary>
        /// <returns>追加可能なメンバーID。追加できない場合はnull。</returns>
        private Guid? GetNextAvailableCollaboratorMemberId()
        {
            var excludedMemberIds = CollaboratorMemberIds
                .Append(AssigneeMemberId ?? Guid.Empty)
                .Where(memberId => memberId != Guid.Empty)
                .ToHashSet();

            return _members?
                .Where(member => member.IsActive)
                .Where(member => !excludedMemberIds.Contains(member.MemberId))
                .Select(member => member.MemberId)
                .Cast<Guid?>()
                .FirstOrDefault();
        }

        /// <summary>
        /// 作業協力者1行分の選択肢を生成する。
        /// </summary>
        /// <param name="selectedMemberId">選択中メンバーID。</param>
        /// <returns>選択肢一覧。</returns>
        private ObservableCollection<MemberOptionViewModel> CreateCollaboratorOptions(Guid selectedMemberId)
        {
            Guid? selected = selectedMemberId == Guid.Empty ? null : selectedMemberId;
            var excludedMemberIds = CollaboratorMemberIds
                .Where(memberId => selected == null || memberId != selected.Value)
                .Append(AssigneeMemberId ?? Guid.Empty)
                .Where(memberId => memberId != Guid.Empty)
                .ToHashSet();
            var options = new ObservableCollection<MemberOptionViewModel>();

            foreach (var member in GetSelectableMembers(selected, excludedMemberIds: excludedMemberIds))
            {
                options.Add(new MemberOptionViewModel(member));
            }

            return options;
        }

        /// <summary>
        /// 選択肢として表示するメンバーを取得する。
        /// </summary>
        /// <param name="selectedMemberId">単一選択中メンバーID。</param>
        /// <param name="selectedMemberIds">複数選択中メンバーID。</param>
        /// <returns>表示対象メンバー一覧。</returns>
        private IEnumerable<TeamMemberViewModel> GetSelectableMembers(
            Guid? selectedMemberId,
            IEnumerable<Guid>? selectedMemberIds = null,
            IEnumerable<Guid>? excludedMemberIds = null
        )
        {
            if (_members == null)
            {
                yield break;
            }

            var selectedIds = new HashSet<Guid>(selectedMemberIds ?? []);
            if (selectedMemberId != null)
            {
                selectedIds.Add(selectedMemberId.Value);
            }
            var excludedIds = new HashSet<Guid>(excludedMemberIds ?? []);

            foreach (var member in _members)
            {
                if (excludedIds.Contains(member.MemberId) && !selectedIds.Contains(member.MemberId))
                {
                    continue;
                }

                if (member.IsActive || selectedIds.Contains(member.MemberId))
                {
                    yield return member;
                }
            }
        }

        /// <summary>
        /// 作業協力者ID一覧から未選択、担当者、重複を除外する。
        /// </summary>
        /// <param name="memberIds">正規化対象のメンバーID一覧。</param>
        /// <returns>正規化済みメンバーID一覧。</returns>
        private List<Guid> NormalizeCollaboratorMemberIds(IEnumerable<Guid> memberIds)
        {
            return memberIds
                .Where(memberId => memberId != Guid.Empty)
                .Where(memberId => AssigneeMemberId == null || memberId != AssigneeMemberId.Value)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 指定メンバーが有効メンバーとして存在するかどうかを判定する。
        /// </summary>
        /// <param name="memberId">対象メンバーID。</param>
        /// <returns>有効メンバーの場合はtrue。</returns>
        private bool IsActiveMember(Guid memberId)
        {
            return _members?.Any(member => member.MemberId == memberId && member.IsActive) == true;
        }

        /// <summary>
        /// 担当者メンバーを取得する
        /// </summary>
        /// <returns>担当者メンバー。未設定または未登録の場合はnull。</returns>
        private TeamMemberViewModel? ResolveAssigneeMember()
        {
            return AssigneeMemberId == null || _members == null
                ? null
                : _members.FirstOrDefault(member => member.MemberId == AssigneeMemberId.Value);
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
        public ICommand AddCollaboratorCommand { get; }

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
            CommitEdits();
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

        /// <summary>
        /// 遅延コミット対象の編集内容を確定するコマンド。
        /// </summary>
        public ICommand CommitEditsCommand { get; }

        public void NotifyEdited()
        {
            _editTimer.Stop();
            _editTimer.Start();
        }

        public void CommitEdits()
        {
            _editTimer.Stop();
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
