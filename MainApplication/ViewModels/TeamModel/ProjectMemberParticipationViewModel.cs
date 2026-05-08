using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// プロジェクト単位のメンバー参加期間を表すViewModel
    /// </summary>
    public class ProjectMemberParticipationViewModel : ViewModelBase
    {
        private readonly Action<ProjectMemberParticipationViewModel> _participationChanged;
        private DateOnly? _participationStartDate;
        private DateOnly? _participationEndDate;

        /// <summary>
        /// プロジェクト参加期間を生成する
        /// </summary>
        /// <param name="project">対象プロジェクト。</param>
        /// <param name="member">対象メンバー。</param>
        /// <param name="participationStartDate">参加開始日。</param>
        /// <param name="participationEndDate">参加終了日。</param>
        /// <param name="participationChanged">参加期間変更時の処理。</param>
        public ProjectMemberParticipationViewModel(
            ProjectViewModel project,
            TeamMemberViewModel member,
            DateOnly? participationStartDate,
            DateOnly? participationEndDate,
            Action<ProjectMemberParticipationViewModel> participationChanged
        )
        {
            Project = project;
            Member = member;
            _participationStartDate = participationStartDate;
            _participationEndDate = participationEndDate;
            _participationChanged = participationChanged;
            NormalizePeriod();
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
        /// 参加開始日
        /// </summary>
        public DateOnly? ParticipationStartDate
        {
            get => _participationStartDate;
            set
            {
                if (_participationStartDate == value)
                {
                    return;
                }

                _participationStartDate = value;
                if (_participationStartDate != null &&
                    _participationEndDate != null &&
                    _participationEndDate < _participationStartDate)
                {
                    _participationEndDate = _participationStartDate;
                    OnPropertyChangedA(nameof(ParticipationEndDate));
                    OnPropertyChangedA(nameof(ParticipationEndDateTime));
                }

                NotifyPeriodChanged();
            }
        }

        /// <summary>
        /// 参加終了日
        /// </summary>
        public DateOnly? ParticipationEndDate
        {
            get => _participationEndDate;
            set
            {
                if (_participationEndDate == value)
                {
                    return;
                }

                _participationEndDate = value;
                if (_participationStartDate != null &&
                    _participationEndDate != null &&
                    _participationStartDate > _participationEndDate)
                {
                    _participationStartDate = _participationEndDate;
                    OnPropertyChangedA(nameof(ParticipationStartDate));
                    OnPropertyChangedA(nameof(ParticipationStartDateTime));
                }

                NotifyPeriodChanged();
            }
        }

        /// <summary>
        /// DatePickerバインド用の参加開始日
        /// </summary>
        public DateTime? ParticipationStartDateTime
        {
            get => ParticipationStartDate?.ToDateTime(TimeOnly.MinValue);
            set => ParticipationStartDate = value == null ? null : DateOnly.FromDateTime(value.Value);
        }

        /// <summary>
        /// DatePickerバインド用の参加終了日
        /// </summary>
        public DateTime? ParticipationEndDateTime
        {
            get => ParticipationEndDate?.ToDateTime(TimeOnly.MinValue);
            set => ParticipationEndDate = value == null ? null : DateOnly.FromDateTime(value.Value);
        }

        /// <summary>
        /// 参加期間表示文字列
        /// </summary>
        public string DisplayText
        {
            get
            {
                var start = ParticipationStartDate?.ToString("yyyy/MM/dd") ?? "制限なし";
                var end = ParticipationEndDate?.ToString("yyyy/MM/dd") ?? "制限なし";
                return $"{start} - {end}";
            }
        }

        /// <summary>
        /// 指定日がプロジェクト参加期間内かどうかを判定する
        /// </summary>
        /// <param name="date">対象日。</param>
        /// <returns>参加期間内の場合はtrue。</returns>
        public bool IsParticipating(DateOnly date)
        {
            return (ParticipationStartDate == null || ParticipationStartDate <= date) &&
                   (ParticipationEndDate == null || date <= ParticipationEndDate);
        }

        /// <summary>
        /// 参加期間の矛盾を補正する
        /// </summary>
        private void NormalizePeriod()
        {
            if (_participationStartDate != null &&
                _participationEndDate != null &&
                _participationEndDate < _participationStartDate)
            {
                _participationEndDate = _participationStartDate;
            }
        }

        /// <summary>
        /// 参加期間変更を通知する
        /// </summary>
        private void NotifyPeriodChanged()
        {
            OnPropertyChangedA(nameof(ParticipationStartDate));
            OnPropertyChangedA(nameof(ParticipationStartDateTime));
            OnPropertyChangedA(nameof(ParticipationEndDate));
            OnPropertyChangedA(nameof(ParticipationEndDateTime));
            OnPropertyChangedA(nameof(DisplayText));
            _participationChanged(this);
        }
    }
}

/* --- End of file --- */
