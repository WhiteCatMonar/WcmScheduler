using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート上に表示するタスク1件分のViewModel
    /// </summary>
    public class GanttTaskItemViewModel : ViewModelBase
    {
        /// <summary>
        /// 対象ノード
        /// </summary>
        public required NodeViewModel Node { get; init; }

        /// <summary>
        /// タスク名
        /// </summary>
        public required string TaskName { get; init; }

        /// <summary>
        /// 担当者名
        /// </summary>
        public required string AssigneeName { get; init; }

        /// <summary>
        /// 担当者バッジ表示文字列
        /// </summary>
        public required string AssigneeInitials { get; init; }

        /// <summary>
        /// 予定開始日時
        /// </summary>
        public required DateTime StartDateTime { get; init; }

        /// <summary>
        /// 予定終了日時
        /// </summary>
        public required DateTime EndDateTime { get; init; }

        /// <summary>
        /// タスクステータス
        /// </summary>
        public required NodeViewModel.TaskStatus Status { get; init; }

        /// <summary>
        /// タスク名を警告色で表示するか
        /// </summary>
        public required bool HasWarning { get; init; }

        /// <summary>
        /// 予定バーを表示できるかどうか
        /// </summary>
        public required bool HasSchedule { get; init; }

        /// <summary>
        /// エラー表示文字列
        /// </summary>
        public required string ErrorText { get; init; }

        /// <summary>
        /// 行上位置
        /// </summary>
        public required double RowTop { get; init; }

        /// <summary>
        /// バー左位置
        /// </summary>
        public required double BarLeft { get; init; }

        /// <summary>
        /// バー幅
        /// </summary>
        public required double BarWidth { get; init; }

        /// <summary>
        /// 中断期間表示一覧
        /// </summary>
        public ObservableCollection<GanttSuspensionItemViewModel> Suspensions { get; } = [];

        /// <summary>
        /// 予定期間表示文字列
        /// </summary>
        public string PeriodText => $"{StartDateTime:yyyy/MM/dd HH:mm} - {EndDateTime:yyyy/MM/dd HH:mm}";

        /// <summary>
        /// 担当者バッジを表示するかどうか
        /// </summary>
        public bool HasAssigneeBadge => !string.IsNullOrWhiteSpace(AssigneeInitials);
    }
}

/* --- End of file --- */
