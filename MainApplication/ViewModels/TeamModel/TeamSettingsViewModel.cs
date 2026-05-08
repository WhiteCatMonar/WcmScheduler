using MainApplication.Models.SaveData;
using MainApplication.Views;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// チーム設定タブのViewModel
    /// </summary>
    public class TeamSettingsViewModel : ViewModelBase
    {
        private const int CalendarDays = 7;
        private readonly TeamProjectsViewModel _teamProjects;
        private readonly ObservableCollection<DateOnly> _specialHolidays;
        private readonly Dictionary<(Guid ProjectId, Guid MemberId, DateOnly WorkDate), int> _workTimeOverrides = [];
        private readonly Dictionary<(Guid ProjectId, Guid MemberId), (DateOnly? StartDate, DateOnly? EndDate)> _projectParticipations = [];
        private TeamMemberViewModel? _selectedMember;
        private DateOnly _calendarStartDate = GetWeekStartDate(DateOnly.FromDateTime(DateTime.Today));
        private bool _isLoading;

        /// <summary>
        /// チーム設定ViewModelを生成する
        /// </summary>
        /// <param name="teamProjects">プロジェクト一覧ViewModel。</param>
        /// <param name="members">チームメンバー一覧。</param>
        public TeamSettingsViewModel(
            TeamProjectsViewModel teamProjects,
            ObservableCollection<TeamMemberViewModel> members,
            ObservableCollection<DateOnly> specialHolidays
        )
        {
            _teamProjects = teamProjects;
            _specialHolidays = specialHolidays;
            Members = members;
            Members.CollectionChanged += OnMembersCollectionChanged;
            _teamProjects.Projects.CollectionChanged += OnProjectsCollectionChanged;

            AddMemberCommand = new RelayCommand(AddMember);
            DeleteMemberCommand = new RelayCommand(DeleteSelectedMember, () => SelectedMember != null);
            MoveMemberUpCommand = new RelayCommand(MoveSelectedMemberUp, () => SelectedMember != null && Members.IndexOf(SelectedMember) > 0);
            MoveMemberDownCommand = new RelayCommand(MoveSelectedMemberDown, () => SelectedMember != null && Members.IndexOf(SelectedMember) < Members.Count - 1);
            PreviousCalendarCommand = new RelayCommand(() => MoveCalendar(-CalendarDays));
            NextCalendarCommand = new RelayCommand(() => MoveCalendar(CalendarDays));
            TodayCalendarCommand = new RelayCommand(() => CalendarStartDate = DateOnly.FromDateTime(DateTime.Today));
            OpenSpecialHolidaySettingsCommand = new RelayCommand(OpenSpecialHolidaySettings);
        }

        /// <summary>
        /// メンバー一覧
        /// </summary>
        public ObservableCollection<TeamMemberViewModel> Members { get; }

        /// <summary>
        /// 選択中メンバーのプロジェクト参加期間一覧
        /// </summary>
        public ObservableCollection<ProjectMemberParticipationViewModel> ProjectParticipations { get; } = [];

        /// <summary>
        /// 選択中メンバーの作業可能時間カレンダー
        /// </summary>
        public ObservableCollection<MemberWorkCalendarDayViewModel> WorkCalendarDays { get; } = [];

        /// <summary>
        /// 特別休日一覧
        /// </summary>
        public ObservableCollection<DateOnly> SpecialHolidays => _specialHolidays;

        /// <summary>
        /// 選択中のメンバー
        /// </summary>
        public TeamMemberViewModel? SelectedMember
        {
            get => _selectedMember;
            set => SetProperty(
                ref _selectedMember,
                value,
                [nameof(SelectedMemberReferenceText), nameof(CanEditCalendar)],
                CreateHooksFromValue(value, chain: RebuildSelectedMemberViews)
            );
        }

        /// <summary>
        /// カレンダー表示開始日。日曜日始まりに正規化する
        /// </summary>
        public DateOnly CalendarStartDate
        {
            get => _calendarStartDate;
            set
            {
                var normalized = GetWeekStartDate(value);
                SetProperty(
                    ref _calendarStartDate,
                    normalized,
                    [nameof(CalendarRangeText)],
                    CreateHooksFromValue(normalized, chain: RebuildWorkCalendar)
                );
            }
        }

        /// <summary>
        /// カレンダー範囲表示文字列
        /// </summary>
        public string CalendarRangeText => $"{CalendarStartDate:yyyy/MM/dd} - {CalendarStartDate.AddDays(CalendarDays - 1):yyyy/MM/dd}";

        /// <summary>
        /// カレンダーを編集できるかどうか
        /// </summary>
        public bool CanEditCalendar => SelectedMember != null;

        /// <summary>
        /// 選択中メンバーの参照状態
        /// </summary>
        public string SelectedMemberReferenceText
        {
            get
            {
                if (SelectedMember == null)
                {
                    return "";
                }

                return IsMemberReferenced(SelectedMember.MemberId)
                    ? "タスクから参照されています。参照中のメンバーは削除できません。"
                    : "タスクから参照されていません。削除できます。";
            }
        }

        /// <summary>
        /// メンバー追加コマンド
        /// </summary>
        public ICommand AddMemberCommand { get; }

        /// <summary>
        /// メンバー削除コマンド
        /// </summary>
        public ICommand DeleteMemberCommand { get; }

        /// <summary>
        /// メンバーを上へ移動するコマンド
        /// </summary>
        public ICommand MoveMemberUpCommand { get; }

        /// <summary>
        /// メンバーを下へ移動するコマンド
        /// </summary>
        public ICommand MoveMemberDownCommand { get; }

        /// <summary>
        /// カレンダーを前の週へ移動するコマンド
        /// </summary>
        public ICommand PreviousCalendarCommand { get; }

        /// <summary>
        /// カレンダーを次の週へ移動するコマンド
        /// </summary>
        public ICommand NextCalendarCommand { get; }

        /// <summary>
        /// カレンダーを今日の週へ戻すコマンド
        /// </summary>
        public ICommand TodayCalendarCommand { get; }

        /// <summary>
        /// 特別休日設定ウィンドウを開くコマンド
        /// </summary>
        public ICommand OpenSpecialHolidaySettingsCommand { get; }

        /// <summary>
        /// 保存データを適用する
        /// </summary>
        /// <param name="members">メンバー保存データ。</param>
        /// <param name="projects">プロジェクト保存データ一覧</param>
        public void LoadFromDataModels(
            IEnumerable<MemberDataModel> members,
            IEnumerable<ProjectDataModel> projects,
            IEnumerable<DateOnly> specialHolidays
        )
        {
            _isLoading = true;
            Members.Clear();
            _specialHolidays.Clear();
            _workTimeOverrides.Clear();
            _projectParticipations.Clear();
            ProjectParticipations.Clear();
            WorkCalendarDays.Clear();

            foreach (var member in members)
            {
                Members.Add(new TeamMemberViewModel(member));
            }

            foreach (var specialHoliday in specialHolidays.OrderBy(date => date))
            {
                _specialHolidays.Add(specialHoliday);
            }

            foreach (var project in projects)
            {
                foreach (var memberInfo in project.MemberInfo)
                {
                    _projectParticipations[(project.ProjectId, memberInfo.MemberId)] =
                        (memberInfo.ParticipationStartDate, memberInfo.ParticipationEndDate);

                    foreach (var workTime in memberInfo.WorkTimes)
                    {
                        _workTimeOverrides[(project.ProjectId, memberInfo.MemberId, workTime.WorkDate)] =
                            Math.Max(0, workTime.WorkTimeMinutes);
                    }
                }
            }

            _isLoading = false;
            SelectedMember = Members.FirstOrDefault();
            RebuildSelectedMemberViews();
        }

        /// <summary>
        /// メンバー保存データへ変換する
        /// </summary>
        /// <returns>メンバー保存データ一覧。</returns>
        public List<MemberDataModel> ToMemberDataModels()
        {
            return [.. Members.Select(member => member.ToDataModel())];
        }

        /// <summary>
        /// 特別休日保存データへ変換する
        /// </summary>
        /// <returns>特別休日一覧。</returns>
        public List<DateOnly> ToSpecialHolidayDataModels()
        {
            return [.. _specialHolidays.OrderBy(date => date)];
        }

        /// <summary>
        /// プロジェクト内メンバー情報の保存データへ変換する
        /// </summary>
        /// <param name="projectId">対象プロジェクトID</param>
        /// <returns>プロジェクト内メンバー情報一覧</returns>
        public List<ProjectMemberInfoDataModel> ToProjectMemberInfoDataModels(Guid projectId)
        {
            var memberIds = _projectParticipations.Keys
                .Where(key => key.ProjectId == projectId)
                .Select(key => key.MemberId)
                .Concat(
                    _workTimeOverrides.Keys
                        .Where(key => key.ProjectId == projectId)
                        .Select(key => key.MemberId)
                )
                .Distinct();

            return
            [
                ..
                memberIds.Select(memberId =>
                {
                    _projectParticipations.TryGetValue((projectId, memberId), out var participation);
                    return new ProjectMemberInfoDataModel
                    {
                        MemberId = memberId,
                        ParticipationStartDate = participation.StartDate,
                        ParticipationEndDate = participation.EndDate,
                        WorkTimes =
                        [
                            ..
                            _workTimeOverrides
                                .Where(item => item.Key.ProjectId == projectId && item.Key.MemberId == memberId)
                                .Select(item => new MemberWorkTimeDataModel
                                {
                                    WorkDate = item.Key.WorkDate,
                                    WorkTimeMinutes = item.Value
                                })
                        ]
                    };
                })
            ];
        }


        /// <summary>
        /// メンバーを追加する
        /// </summary>
        private void AddMember()
        {
            var member = new TeamMemberViewModel();
            Members.Add(member);
            SelectedMember = member;
        }

        /// <summary>
        /// 選択中メンバーを削除する
        /// </summary>
        private void DeleteSelectedMember()
        {
            if (SelectedMember == null || IsMemberReferenced(SelectedMember.MemberId))
            {
                return;
            }

            var removedMemberId = SelectedMember.MemberId;
            Members.Remove(SelectedMember);
            RemoveProjectMemberData(removedMemberId);
            SelectedMember = Members.FirstOrDefault();
        }

        /// <summary>
        /// 選択中メンバーを一つ上へ移動する
        /// </summary>
        private void MoveSelectedMemberUp()
        {
            MoveSelectedMember(-1);
        }

        /// <summary>
        /// 選択中メンバーを一つ下へ移動する
        /// </summary>
        private void MoveSelectedMemberDown()
        {
            MoveSelectedMember(1);
        }

        /// <summary>
        /// 選択中メンバーを指定方向へ移動する
        /// </summary>
        /// <param name="delta">移動量。</param>
        private void MoveSelectedMember(int delta)
        {
            if (SelectedMember == null)
            {
                return;
            }

            var oldIndex = Members.IndexOf(SelectedMember);
            var newIndex = oldIndex + delta;
            if (oldIndex < 0 || newIndex < 0 || newIndex >= Members.Count)
            {
                return;
            }

            Members.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// カレンダー範囲を移動する
        /// </summary>
        /// <param name="days">移動日数。</param>
        private void MoveCalendar(int days)
        {
            CalendarStartDate = CalendarStartDate.AddDays(days);
        }

        /// <summary>
        /// 特別休日設定ウィンドウを開く
        /// </summary>
        private void OpenSpecialHolidaySettings()
        {
            var viewModel = new SpecialHolidaySettingsViewModel(_specialHolidays);
            var window = new SpecialHolidaySettingsWindow
            {
                DataContext = viewModel,
                Owner = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(window => window.IsActive)
            };

            window.ShowDialog();
            RefreshVisibleEffectiveWorkTimes();
        }

        /// <summary>
        /// 指定日を含む週の日曜日を取得する
        /// </summary>
        /// <param name="date">対象日。</param>
        /// <returns>週の開始日。</returns>
        private static DateOnly GetWeekStartDate(DateOnly date)
        {
            return date.AddDays(-(int)date.DayOfWeek);
        }

        /// <summary>
        /// メンバー一覧変更時にイベント購読を更新する
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnMembersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TeamMemberViewModel member in e.NewItems)
                {
                    member.PropertyChanged += OnMemberPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (TeamMemberViewModel member in e.OldItems)
                {
                    member.PropertyChanged -= OnMemberPropertyChanged;
                    if (!_isLoading)
                    {
                        RemoveProjectMemberData(member.MemberId);
                    }
                }
            }
        }

        /// <summary>
        /// プロジェクト一覧変更時にプロジェクト関連表示を更新する
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">変更内容</param>
        private void OnProjectsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ProjectViewModel project in e.OldItems)
                {
                    RemoveProjectData(project.ProjectId);
                }
            }

            if (!_isLoading)
            {
                RebuildSelectedMemberViews();
            }
        }

        /// <summary>
        /// メンバー変更時に関連表示を更新する
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">変更内容。</param>
        private void OnMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChangedA(nameof(SelectedMemberReferenceText));

            if (ReferenceEquals(sender, SelectedMember) && IsWeeklyDefaultWorkTimeProperty(e.PropertyName))
            {
                RefreshVisibleEffectiveWorkTimes();
            }
        }

        /// <summary>
        /// 指定プロパティが曜日別デフォルト作業時間かどうかを判定する
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        /// <returns>曜日別デフォルト作業時間の場合はtrue。</returns>
        private static bool IsWeeklyDefaultWorkTimeProperty(string? propertyName)
        {
            return propertyName is
                nameof(TeamMemberViewModel.SundayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.MondayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.TuesdayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.WednesdayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.ThursdayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.FridayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.SaturdayWorkTimeMinutes) or
                nameof(TeamMemberViewModel.SpecialHolidayWorkTimeMinutes);
        }

        /// <summary>
        /// 選択メンバーに紐づく表示を再構築する
        /// </summary>
        private void RebuildSelectedMemberViews()
        {
            RebuildProjectParticipations();
            RebuildWorkCalendar();
        }

        /// <summary>
        /// プロジェクト参加期間一覧を再構築する
        /// </summary>
        private void RebuildProjectParticipations()
        {
            ProjectParticipations.Clear();

            if (SelectedMember == null)
            {
                return;
            }

            foreach (var project in _teamProjects.Projects)
            {
                var key = (project.ProjectId, SelectedMember.MemberId);
                _projectParticipations.TryGetValue(key, out var period);
                ProjectParticipations.Add(
                    new ProjectMemberParticipationViewModel(
                        project,
                        SelectedMember,
                        period.StartDate,
                        period.EndDate,
                        OnProjectMemberParticipationChanged
                    )
                );
            }
        }

        /// <summary>
        /// 作業可能時間カレンダーを再構築する
        /// </summary>
        private void RebuildWorkCalendar()
        {
            WorkCalendarDays.Clear();

            if (SelectedMember == null)
            {
                return;
            }

            for (var offset = 0; offset < CalendarDays; offset++)
            {
                var date = CalendarStartDate.AddDays(offset);
                var day = new MemberWorkCalendarDayViewModel(date);

                foreach (var participation in ProjectParticipations)
                {
                    var key = (participation.Project.ProjectId, SelectedMember.MemberId, date);
                    _workTimeOverrides.TryGetValue(key, out var minutes);

                    day.ProjectWorkTimes.Add(
                        new ProjectMemberWorkTimeViewModel(
                            participation.Project,
                            SelectedMember,
                            participation,
                            date,
                            _workTimeOverrides.ContainsKey(key) ? minutes : null,
                            _specialHolidays,
                            OnProjectMemberWorkTimeChanged
                        )
                    );
                }

                WorkCalendarDays.Add(day);
            }
        }

        /// <summary>
        /// 表示中セルのデフォルト反映値を更新する
        /// </summary>
        private void RefreshVisibleEffectiveWorkTimes()
        {
            foreach (var workTime in WorkCalendarDays.SelectMany(day => day.ProjectWorkTimes))
            {
                workTime.NotifyDefaultWorkTimeChanged();
            }
        }

        /// <summary>
        /// 表示中セルの参加期間反映値を更新する
        /// </summary>
        private void RefreshVisibleParticipationStates()
        {
            foreach (var workTime in WorkCalendarDays.SelectMany(day => day.ProjectWorkTimes))
            {
                workTime.NotifyParticipationChanged();
            }
        }

        /// <summary>
        /// プロジェクト参加期間変更を保存用辞書へ反映する
        /// </summary>
        /// <param name="participation">変更された参加期間。</param>
        private void OnProjectMemberParticipationChanged(ProjectMemberParticipationViewModel participation)
        {
            var key = (participation.Project.ProjectId, participation.Member.MemberId);
            _projectParticipations[key] = (participation.ParticipationStartDate, participation.ParticipationEndDate);
            RefreshVisibleParticipationStates();
        }

        /// <summary>
        /// カレンダーセルの作業時間変更を保存用辞書へ反映する
        /// </summary>
        /// <param name="workTime">変更された作業可能時間。</param>
        private void OnProjectMemberWorkTimeChanged(ProjectMemberWorkTimeViewModel workTime)
        {
            var key = (workTime.Project.ProjectId, workTime.Member.MemberId, workTime.WorkDate);
            if (workTime.WorkTimeMinutes == null)
            {
                _workTimeOverrides.Remove(key);
                return;
            }

            _workTimeOverrides[key] = workTime.WorkTimeMinutes.Value;
        }

        /// <summary>
        /// 指定メンバーのプロジェクト関連データを削除する
        /// </summary>
        /// <param name="memberId">対象メンバーID。</param>
        private void RemoveProjectMemberData(Guid memberId)
        {
            foreach (var key in _workTimeOverrides.Keys.Where(key => key.MemberId == memberId).ToList())
            {
                _workTimeOverrides.Remove(key);
            }

            foreach (var key in _projectParticipations.Keys.Where(key => key.MemberId == memberId).ToList())
            {
                _projectParticipations.Remove(key);
            }
        }

        /// <summary>
        /// 指定プロジェクトのメンバー関連データを削除する
        /// </summary>
        /// <param name="projectId">対象プロジェクトID</param>
        private void RemoveProjectData(Guid projectId)
        {
            foreach (var key in _workTimeOverrides.Keys.Where(key => key.ProjectId == projectId).ToList())
            {
                _workTimeOverrides.Remove(key);
            }

            foreach (var key in _projectParticipations.Keys.Where(key => key.ProjectId == projectId).ToList())
            {
                _projectParticipations.Remove(key);
            }
        }

        /// <summary>
        /// 指定メンバーがタスクから参照されているかどうかを判定する
        /// </summary>
        /// <param name="memberId">対象メンバーID。</param>
        /// <returns>参照されている場合はtrue。</returns>
        private bool IsMemberReferenced(Guid memberId)
        {
            return _teamProjects.Projects
                .SelectMany(project => project.DependencyEditor.Nodes.Nodes)
                .Any(node =>
                    node.Detail.AssigneeMemberId == memberId ||
                    node.Detail.CollaboratorMemberIds.Contains(memberId)
                );
        }
    }
}

/* --- End of file --- */
