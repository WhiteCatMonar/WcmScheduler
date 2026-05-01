using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// プロジェクト単位、メンバー単位、日付単位の作業可能時間を表すViewModel
    /// </summary>
    public class ProjectMemberWorkTimeViewModel : ViewModelBase
    {
        private readonly Action<ProjectMemberWorkTimeViewModel> _workTimeChanged;
        private readonly ObservableCollection<DateOnly> _specialHolidays;
        private int? _workTimeMinutes;

        /// <summary>
        /// 日付別作業可能時間を生成する
        /// </summary>
        /// <param name="project">対象プロジェクト。</param>
        /// <param name="member">対象メンバー。</param>
        /// <param name="participation">プロジェクト参加期間。</param>
        /// <param name="workDate">対象日付。</param>
        /// <param name="workTimeMinutes">上書き作業可能時間。未設定ならnull。</param>
        /// <param name="specialHolidays">特別休日一覧。</param>
        /// <param name="workTimeChanged">作業時間変更時の処理。</param>
        public ProjectMemberWorkTimeViewModel(
            ProjectViewModel project,
            TeamMemberViewModel member,
            ProjectMemberParticipationViewModel participation,
            DateOnly workDate,
            int? workTimeMinutes,
            ObservableCollection<DateOnly> specialHolidays,
            Action<ProjectMemberWorkTimeViewModel> workTimeChanged
        )
        {
            Project = project;
            Member = member;
            Participation = participation;
            WorkDate = workDate;
            _workTimeMinutes = workTimeMinutes;
            _specialHolidays = specialHolidays;
            _workTimeChanged = workTimeChanged;
        }

        /// <summary>
        /// 対象プロジェクト
        /// </summary>
        public ProjectViewModel Project { get; }

        /// <summary>
        /// 対象メンバー
        /// </summary>
        public TeamMemberViewModel Member { get; }

        /// <summary>
        /// プロジェクト参加期間
        /// </summary>
        public ProjectMemberParticipationViewModel Participation { get; }

        /// <summary>
        /// 対象日付
        /// </summary>
        public DateOnly WorkDate { get; }

        /// <summary>
        /// 上書き作業可能時間。未入力ならnull
        /// </summary>
        public int? WorkTimeMinutes
        {
            get => _workTimeMinutes;
            set
            {
                if (!IsParticipating)
                {
                    return;
                }

                int? normalized = value == null ? null : Math.Max(0, value.Value);
                if (SetProperty(
                    ref _workTimeMinutes,
                    normalized,
                    [nameof(EffectiveWorkTimeMinutes), nameof(PlaceholderText), nameof(DurationText), nameof(DisplayText)]
                ))
                {
                    _workTimeChanged(this);
                }
            }
        }

        /// <summary>
        /// 対象日付がプロジェクト参加期間内かどうか
        /// </summary>
        public bool IsParticipating => Participation.IsParticipating(WorkDate);

        /// <summary>
        /// デフォルト反映後の作業可能時間。単位は分
        /// </summary>
        public int EffectiveWorkTimeMinutes => IsParticipating ? WorkTimeMinutes ?? Member.GetDefaultWorkTimeMinutes(WorkDate, _specialHolidays) : 0;

        /// <summary>
        /// 未入力時に表示するデフォルト作業可能時間
        /// </summary>
        public string PlaceholderText => WorkTimeMinutes == null && IsParticipating ? EffectiveWorkTimeMinutes.ToString() : "";

        /// <summary>
        /// デフォルト反映後の作業可能時間の時間表記
        /// </summary>
        public string DurationText => TeamMemberViewModel.FormatDuration(EffectiveWorkTimeMinutes);

        /// <summary>
        /// カレンダーセル表示用文字列
        /// </summary>
        public string DisplayText => IsParticipating
            ? $"{Project.ProjectName}:{EffectiveWorkTimeMinutes}分"
            : $"{Project.ProjectName}:参加期間外";

        /// <summary>
        /// メンバーのデフォルト作業時間変更を表示へ反映する
        /// </summary>
        public void NotifyDefaultWorkTimeChanged()
        {
            OnPropertyChangedA(nameof(EffectiveWorkTimeMinutes));
            OnPropertyChangedA(nameof(PlaceholderText));
            OnPropertyChangedA(nameof(DurationText));
            OnPropertyChangedA(nameof(DisplayText));
        }

        /// <summary>
        /// プロジェクト参加期間変更を表示へ反映する
        /// </summary>
        public void NotifyParticipationChanged()
        {
            OnPropertyChangedA(nameof(IsParticipating));
            OnPropertyChangedA(nameof(EffectiveWorkTimeMinutes));
            OnPropertyChangedA(nameof(PlaceholderText));
            OnPropertyChangedA(nameof(DurationText));
            OnPropertyChangedA(nameof(DisplayText));
        }
    }
}

/* --- End of file --- */
