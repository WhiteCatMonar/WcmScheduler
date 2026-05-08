using MainApplication.ViewModels.ProjectModel;
using MainApplication.ViewModels.TeamModel;

namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャート表示用データ生成サービス
    /// </summary>
    public class GanttChartService
    {
        /// <summary>
        /// プロジェクト内ノードから表示可能なガントタスクを生成する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="timelineStartDate">表示開始日</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <param name="rowHeight">1行分の表示高さ</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>ガントタスク一覧</returns>
        public List<GanttTaskItemViewModel> CreateProjectTasks(
            NodeEditorViewModel nodeEditor,
            DateOnly timelineStartDate,
            double dayWidth,
            double rowHeight,
            IEnumerable<DateOnly> specialHolidays
        )
        {
            var holidays = specialHolidays.ToHashSet();
            var schedules = CalculateSchedules(nodeEditor, holidays);
            var result = new List<GanttTaskItemViewModel>();
            var index = 0;

            foreach (var schedule in schedules.OrderBy(item => item.SortDateTime).ThenBy(item => item.Node.Detail.TaskName))
            {
                var left = schedule.HasSchedule ? CalculateLeft(timelineStartDate, schedule.StartDateTime, dayWidth) : 0.0;
                var scheduleWidth = schedule.HasSchedule
                    ? CalculateWidth(schedule.StartDateTime, schedule.EndDateTime, dayWidth)
                    : 0.0;
                var width = schedule.HasSchedule
                    ? Math.Max(1.0, scheduleWidth)
                    : 0.0;
                var task = new GanttTaskItemViewModel
                {
                    Node = schedule.Node,
                    AssigneeName = ResolveAssigneeName(nodeEditor, schedule.Node.Detail.AssigneeMemberId),
                    AssigneeInitials = ResolveAssigneeInitials(nodeEditor, schedule.Node.Detail.AssigneeMemberId),
                    StartDateTime = schedule.StartDateTime,
                    EndDateTime = schedule.EndDateTime,
                    HasWarning = schedule.HasWarning,
                    HasSchedule = schedule.HasSchedule,
                    ErrorText = schedule.ErrorText,
                    RowTop = index * rowHeight,
                    BarLeft = left,
                    BarWidth = width,
                    ScheduleBarWidth = scheduleWidth
                };

                if (schedule.HasSchedule)
                {
                    foreach (var period in schedule.Node.Detail.NormalizedSuspensionPeriods)
                    {
                        var overlapStart = period.StartDateTime > schedule.StartDateTime ? period.StartDateTime : schedule.StartDateTime;
                        var overlapEnd = period.EndDateTime < schedule.EndDateTime ? period.EndDateTime : schedule.EndDateTime;
                        if (overlapStart < overlapEnd)
                        {
                            task.Suspensions.Add(
                                new GanttSuspensionItemViewModel(
                                    CalculateLeft(timelineStartDate, overlapStart, dayWidth) - left,
                                    Math.Max(2.0, CalculateWidth(overlapStart, overlapEnd, dayWidth))
                                )
                            );
                        }
                    }
                }

                result.Add(task);
                index++;
            }

            return result;
        }

        /// <summary>
        /// ノード一覧から算定済み予定を生成する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>算定済み予定一覧</returns>
        private static List<GanttTaskSchedule> CalculateSchedules(
            NodeEditorViewModel nodeEditor,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            var schedules = new Dictionary<NodeViewModel, GanttTaskSchedule>();
            var nodes = nodeEditor.Nodes.Nodes.ToList();

            foreach (var node in nodes)
            {
                CalculateSchedule(node, nodeEditor, schedules, [], specialHolidays);
            }

            foreach (var node in nodes.Where(node => !schedules.ContainsKey(node)))
            {
                schedules[node] = GanttTaskSchedule.CreateError(
                    node,
                    DateTime.Today,
                    "見積時間が未設定です"
                );
            }

            return [.. schedules.Values];
        }

        /// <summary>
        /// 指定ノードの予定期間を算定する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="schedules">算定済み予定</param>
        /// <param name="visiting">循環検出用ノード集合</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>算定済み予定</returns>
        private static GanttTaskSchedule? CalculateSchedule(
            NodeViewModel node,
            NodeEditorViewModel nodeEditor,
            Dictionary<NodeViewModel, GanttTaskSchedule> schedules,
            HashSet<NodeViewModel> visiting,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            if (schedules.TryGetValue(node, out var existing))
            {
                return existing;
            }

            if (!visiting.Add(node))
            {
                return null;
            }

            var detail = node.Detail;
            var estimateMinutes = detail.WorkEstimateMinutes.GetValueOrDefault();
            var isEndOnly = detail.StartDateTime == null && detail.EndDateTime != null;
            DateTime? start = detail.StartDateTime;
            DateTime? end = detail.EndDateTime;

            if (start != null && end != null)
            {
                return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
            }

            if (estimateMinutes <= 0)
            {
                visiting.Remove(node);
                schedules[node] = GanttTaskSchedule.CreateError(
                    node,
                    start ?? end ?? DateTime.Today,
                    "見積時間が未設定です"
                );
                return null;
            }

            if (start != null)
            {
                end = AddWorkMinutes(start.Value, estimateMinutes, ResolveAssignee(nodeEditor, detail.AssigneeMemberId), specialHolidays);
                return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
            }

            if (end != null)
            {
                start = SubtractWorkMinutes(end.Value, estimateMinutes, ResolveAssignee(nodeEditor, detail.AssigneeMemberId), specialHolidays);
                return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
            }

            var predecessorEnd = GetPredecessors(node, nodeEditor)
                .Select(predecessor => CalculateSchedule(predecessor, nodeEditor, schedules, visiting, specialHolidays)?.EndDateTime)
                .Where(value => value != null)
                .DefaultIfEmpty(DateTime.Today)
                .Max();

            start = predecessorEnd ?? DateTime.Today;
            if (node.Status == NodeViewModel.TaskStatus.Ready && start < DateTime.Now)
            {
                start = DateTime.Now;
            }

            end = AddWorkMinutes(start.Value, estimateMinutes, ResolveAssignee(nodeEditor, detail.AssigneeMemberId), specialHolidays);
            return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
        }

        /// <summary>
        /// 予定を算定済み予定へ追加する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="schedules">算定済み予定</param>
        /// <param name="visiting">循環検出用ノード集合</param>
        /// <param name="start">開始日時</param>
        /// <param name="end">終了日時</param>
        /// <param name="isEndOnly">終了日時のみ設定されているか</param>
        /// <returns>追加した予定</returns>
        private static GanttTaskSchedule AddSchedule(
            NodeViewModel node,
            Dictionary<NodeViewModel, GanttTaskSchedule> schedules,
            HashSet<NodeViewModel> visiting,
            DateTime start,
            DateTime end,
            bool isEndOnly
        )
        {
            var normalizedEnd = end <= start ? start.AddMinutes(1) : end;
            var schedule = GanttTaskSchedule.CreateSchedule(node, start, normalizedEnd, isEndOnly);
            schedules[node] = schedule;
            visiting.Remove(node);
            return schedule;
        }

        /// <summary>
        /// 前段タスク一覧を取得する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <returns>前段ノード一覧</returns>
        private static IEnumerable<NodeViewModel> GetPredecessors(NodeViewModel node, NodeEditorViewModel nodeEditor)
        {
            var inputPortIds = node.InputPorts.Select(port => port.PortGuid).ToHashSet();
            foreach (var connection in nodeEditor.Connections.Connections)
            {
                if (inputPortIds.Contains(connection.ToPort.PortGuid))
                {
                    var predecessor = nodeEditor.Nodes.Nodes.FirstOrDefault(item => item.OutputPorts.Contains(connection.FromPort));
                    if (predecessor != null)
                    {
                        yield return predecessor;
                    }
                }
            }
        }

        /// <summary>
        /// 作業可能時間を考慮して作業分を加算する
        /// </summary>
        /// <param name="start">開始日時</param>
        /// <param name="minutes">作業分</param>
        /// <param name="assignee">担当者</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>終了日時</returns>
        private static DateTime AddWorkMinutes(
            DateTime start,
            int minutes,
            TeamMemberViewModel? assignee,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            if (!HasAnyWorkingDay(assignee, specialHolidays))
            {
                return start.AddMinutes(minutes);
            }

            var cursor = start;
            var remaining = minutes;
            while (remaining > 0)
            {
                var dailyMinutes = GetDailyWorkMinutes(cursor, assignee, specialHolidays);
                if (dailyMinutes <= 0)
                {
                    cursor = cursor.Date.AddDays(1);
                    continue;
                }

                var todayEnd = cursor.Date.AddMinutes(dailyMinutes);
                if (cursor >= todayEnd)
                {
                    cursor = cursor.Date.AddDays(1);
                    continue;
                }

                var available = (int)Math.Ceiling((todayEnd - cursor).TotalMinutes);
                var used = Math.Min(remaining, available);
                cursor = cursor.AddMinutes(used);
                remaining -= used;
            }

            return cursor;
        }

        /// <summary>
        /// 作業可能時間を考慮して作業分を減算する
        /// </summary>
        /// <param name="end">終了日時</param>
        /// <param name="minutes">作業分</param>
        /// <param name="assignee">担当者</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>開始日時</returns>
        private static DateTime SubtractWorkMinutes(
            DateTime end,
            int minutes,
            TeamMemberViewModel? assignee,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            if (!HasAnyWorkingDay(assignee, specialHolidays))
            {
                return end.AddMinutes(-minutes);
            }

            var cursor = end;
            var remaining = minutes;
            while (remaining > 0)
            {
                var dailyMinutes = GetDailyWorkMinutes(cursor, assignee, specialHolidays);
                if (dailyMinutes <= 0)
                {
                    cursor = cursor.Date.AddTicks(-1);
                    continue;
                }

                var todayStart = cursor.Date;
                var todayEnd = cursor.Date.AddMinutes(dailyMinutes);
                if (cursor > todayEnd)
                {
                    cursor = todayEnd;
                }

                if (cursor <= todayStart)
                {
                    cursor = cursor.Date.AddTicks(-1);
                    continue;
                }

                var available = (int)Math.Ceiling((cursor - todayStart).TotalMinutes);
                var used = Math.Min(remaining, available);
                cursor = cursor.AddMinutes(-used);
                remaining -= used;
            }

            return cursor;
        }

        /// <summary>
        /// 指定日の作業可能時間を取得する
        /// </summary>
        /// <param name="dateTime">対象日時</param>
        /// <param name="assignee">担当者</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>作業可能時間。単位は分</returns>
        private static int GetDailyWorkMinutes(
            DateTime dateTime,
            TeamMemberViewModel? assignee,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            return assignee?.GetDefaultWorkTimeMinutes(DateOnly.FromDateTime(dateTime.Date), specialHolidays) ?? 480;
        }

        /// <summary>
        /// 担当者に作業可能日が存在するかどうかを判定する
        /// </summary>
        /// <param name="assignee">担当者</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>作業可能日が存在する場合はtrue</returns>
        private static bool HasAnyWorkingDay(TeamMemberViewModel? assignee, IReadOnlySet<DateOnly> specialHolidays)
        {
            if (assignee == null)
            {
                return true;
            }

            return Enum.GetValues<DayOfWeek>()
                       .Any(dayOfWeek => assignee.GetDefaultWorkTimeMinutes(dayOfWeek) > 0) ||
                   specialHolidays.Any(date => assignee.GetDefaultWorkTimeMinutes(date, specialHolidays) > 0);
        }

        /// <summary>
        /// 担当者を解決する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者</returns>
        private static TeamMemberViewModel? ResolveAssignee(NodeEditorViewModel nodeEditor, Guid? memberId)
        {
            if (memberId == null || nodeEditor.TeamMembers == null)
            {
                return null;
            }

            return nodeEditor.TeamMembers.FirstOrDefault(member => member.MemberId == memberId.Value);
        }

        /// <summary>
        /// 担当者名を解決する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者名</returns>
        private static string ResolveAssigneeName(NodeEditorViewModel nodeEditor, Guid? memberId)
        {
            return ResolveAssignee(nodeEditor, memberId)?.DisplayText ?? "(未担当)";
        }

        /// <summary>
        /// 担当者イニシャルを解決する
        /// </summary>
        /// <param name="nodeEditor">対象ノードエディタ</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者イニシャル</returns>
        private static string ResolveAssigneeInitials(NodeEditorViewModel nodeEditor, Guid? memberId)
        {
            return ResolveAssignee(nodeEditor, memberId)?.Initials ?? "";
        }

        /// <summary>
        /// 表示左位置を計算する
        /// </summary>
        /// <param name="timelineStartDate">表示開始日</param>
        /// <param name="dateTime">対象日時</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <returns>表示左位置</returns>
        private static double CalculateLeft(DateOnly timelineStartDate, DateTime dateTime, double dayWidth)
        {
            var days = (DateOnly.FromDateTime(dateTime.Date).DayNumber - timelineStartDate.DayNumber);
            return (days * dayWidth) + (dateTime.TimeOfDay.TotalMinutes / 1440.0 * dayWidth);
        }

        /// <summary>
        /// 表示幅を計算する
        /// </summary>
        /// <param name="start">開始日時</param>
        /// <param name="end">終了日時</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <returns>表示幅</returns>
        private static double CalculateWidth(DateTime start, DateTime end, double dayWidth)
        {
            return Math.Max(1.0, (end - start).TotalDays * dayWidth);
        }

        /// <summary>
        /// 内部算定済み予定
        /// </summary>
        private sealed record GanttTaskSchedule(
            NodeViewModel Node,
            DateTime StartDateTime,
            DateTime EndDateTime,
            bool HasSchedule,
            bool HasWarning,
            string ErrorText
        )
        {
            /// <summary>
            /// 並び替え用日時
            /// </summary>
            public DateTime SortDateTime => HasSchedule ? StartDateTime : DateTime.MaxValue;

            /// <summary>
            /// 予定ありの算定結果を作成する
            /// </summary>
            /// <param name="node">対象ノード</param>
            /// <param name="start">開始日時</param>
            /// <param name="end">終了日時</param>
            /// <param name="isEndOnly">終了日時のみ設定されているか</param>
            /// <returns>算定結果</returns>
            public static GanttTaskSchedule CreateSchedule(NodeViewModel node, DateTime start, DateTime end, bool isEndOnly)
            {
                return new GanttTaskSchedule(
                    node,
                    start,
                    end,
                    true,
                    isEndOnly,
                    isEndOnly ? "終了日時のみ設定されています" : ""
                );
            }

            /// <summary>
            /// 予定なしの算定結果を作成する
            /// </summary>
            /// <param name="node">対象ノード</param>
            /// <param name="baseDateTime">基準日時</param>
            /// <param name="errorText">エラー表示文字列</param>
            /// <returns>算定結果</returns>
            public static GanttTaskSchedule CreateError(NodeViewModel node, DateTime baseDateTime, string errorText)
            {
                return new GanttTaskSchedule(
                    node,
                    baseDateTime,
                    baseDateTime,
                    false,
                    true,
                    errorText
                );
            }
        }
    }
}

/* --- End of file --- */
