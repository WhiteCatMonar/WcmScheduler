using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.TeamModel
{
    /// <summary>
    /// メンバー作業可能時間カレンダーの1日分を表すViewModel。
    /// </summary>
    public class MemberWorkCalendarDayViewModel : ViewModelBase
    {
        /// <summary>
        /// 1日分のカレンダー表示を生成する。
        /// </summary>
        /// <param name="workDate">対象日付。</param>
        public MemberWorkCalendarDayViewModel(DateOnly workDate)
        {
            WorkDate = workDate;
        }

        /// <summary>
        /// 対象日付。
        /// </summary>
        public DateOnly WorkDate { get; }

        /// <summary>
        /// 日付表示文字列。
        /// </summary>
        public string DateText => WorkDate.ToString("MM/dd");

        /// <summary>
        /// 曜日表示文字列。
        /// </summary>
        public string DayOfWeekText => WorkDate.DayOfWeek switch
        {
            DayOfWeek.Monday => "月",
            DayOfWeek.Tuesday => "火",
            DayOfWeek.Wednesday => "水",
            DayOfWeek.Thursday => "木",
            DayOfWeek.Friday => "金",
            DayOfWeek.Saturday => "土",
            DayOfWeek.Sunday => "日",
            _ => ""
        };

        /// <summary>
        /// プロジェクト別作業可能時間一覧。
        /// </summary>
        public ObservableCollection<ProjectMemberWorkTimeViewModel> ProjectWorkTimes { get; } = [];
    }
}

/* --- End of file --- */
