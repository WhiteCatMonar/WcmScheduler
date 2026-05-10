using MainApplication.ViewModels.DependencyEditorModel;
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
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="projectId">対象プロジェクトID</param>
        /// <param name="memberAvailabilityProvider">プロジェクト内メンバー稼働条件提供元</param>
        /// <param name="timelineStartDate">表示開始日</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <param name="rowHeight">1行分の表示高さ</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>ガントタスク一覧</returns>
        public List<GanttTaskItemViewModel> CreateProjectTasks(
            DependencyEditorViewModel dependencyEditor,
            Guid projectId,
            IProjectMemberAvailabilityProvider? memberAvailabilityProvider,
            DateOnly timelineStartDate,
            double dayWidth,
            double rowHeight,
            IEnumerable<DateOnly> specialHolidays,
            GanttChartDisplaySettingsViewModel displaySettings
        )
        {
            var holidays = specialHolidays.ToHashSet();
            var schedules = CalculateSchedules(dependencyEditor, holidays);
            var dependencyOrder = CreateDependencyOrderMap(dependencyEditor);
            var result = new List<GanttTaskItemViewModel>();
            var index = 0;

            foreach (var schedule in SortSchedules(schedules.Where(item => displaySettings.IsTaskVisible(item.Node)), displaySettings.SelectedSortKey, dependencyOrder))
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
                    AssigneeName = ResolveAssigneeName(dependencyEditor, schedule.Node.Detail.AssigneeMemberId),
                    AssigneeInitials = ResolveAssigneeInitials(dependencyEditor, schedule.Node.Detail.AssigneeMemberId),
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
                    AddUnavailablePeriods(
                        task,
                        schedule,
                        projectId,
                        ResolveAssignee(dependencyEditor, schedule.Node.Detail.AssigneeMemberId),
                        memberAvailabilityProvider,
                        timelineStartDate,
                        dayWidth,
                        left,
                        holidays
                    );

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
        /// 参加期間外と作業不可日の表示区間を追加する
        /// </summary>
        /// <param name="task">追加対象ガントタスク</param>
        /// <param name="schedule">予定期間</param>
        /// <param name="projectId">対象プロジェクトID</param>
        /// <param name="assignee">担当者</param>
        /// <param name="memberAvailabilityProvider">プロジェクト内メンバー稼働条件提供元</param>
        /// <param name="timelineStartDate">表示開始日</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <param name="barLeft">タスクバー左位置</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        private static void AddUnavailablePeriods(
            GanttTaskItemViewModel task,
            GanttTaskSchedule schedule,
            Guid projectId,
            TeamMemberViewModel? assignee,
            IProjectMemberAvailabilityProvider? memberAvailabilityProvider,
            DateOnly timelineStartDate,
            double dayWidth,
            double barLeft,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            if (assignee == null)
            {
                return;
            }

            var startDate = DateOnly.FromDateTime(schedule.StartDateTime.Date);
            var endDate = DateOnly.FromDateTime(schedule.EndDateTime.Date);
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var segmentStart = Max(schedule.StartDateTime, date.ToDateTime(TimeOnly.MinValue));
                var segmentEnd = Min(schedule.EndDateTime, date.AddDays(1).ToDateTime(TimeOnly.MinValue));
                if (segmentStart >= segmentEnd)
                {
                    continue;
                }

                if (memberAvailabilityProvider?.IsParticipating(projectId, assignee.MemberId, date) == false)
                {
                    task.OutOfParticipationPeriods.Add(CreateUnavailablePeriod(timelineStartDate, segmentStart, segmentEnd, dayWidth, barLeft));
                    continue;
                }

                var workMinutes = memberAvailabilityProvider?.GetEffectiveWorkTimeMinutes(projectId, assignee, date)
                                  ?? assignee.GetDefaultWorkTimeMinutes(date, specialHolidays);
                if (workMinutes <= 0)
                {
                    task.NonWorkingPeriods.Add(CreateUnavailablePeriod(timelineStartDate, segmentStart, segmentEnd, dayWidth, barLeft));
                }
            }
        }

        /// <summary>
        /// 稼働不可区間表示を生成する
        /// </summary>
        /// <param name="timelineStartDate">表示開始日</param>
        /// <param name="start">区間開始日時</param>
        /// <param name="end">区間終了日時</param>
        /// <param name="dayWidth">1日分の表示幅</param>
        /// <param name="barLeft">タスクバー左位置</param>
        /// <returns>稼働不可区間表示</returns>
        private static GanttUnavailablePeriodItemViewModel CreateUnavailablePeriod(
            DateOnly timelineStartDate,
            DateTime start,
            DateTime end,
            double dayWidth,
            double barLeft
        )
        {
            return new GanttUnavailablePeriodItemViewModel(
                CalculateLeft(timelineStartDate, start, dayWidth) - barLeft,
                Math.Max(2.0, CalculateWidth(start, end, dayWidth))
            );
        }

        /// <summary>
        /// ノード一覧から算定済み予定を生成する
        /// </summary>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>算定済み予定一覧</returns>
        private static List<GanttTaskSchedule> CalculateSchedules(
            DependencyEditorViewModel dependencyEditor,
            IReadOnlySet<DateOnly> specialHolidays
        )
        {
            var schedules = new Dictionary<TaskNodeViewModel, GanttTaskSchedule>();
            var nodes = dependencyEditor.Nodes.Nodes.ToList();

            foreach (var node in nodes)
            {
                CalculateSchedule(node, dependencyEditor, schedules, [], specialHolidays);
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
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="schedules">算定済み予定</param>
        /// <param name="visiting">循環検出用ノード集合</param>
        /// <param name="specialHolidays">特別休日一覧</param>
        /// <returns>算定済み予定</returns>
        private static GanttTaskSchedule? CalculateSchedule(
            TaskNodeViewModel node,
            DependencyEditorViewModel dependencyEditor,
            Dictionary<TaskNodeViewModel, GanttTaskSchedule> schedules,
            HashSet<TaskNodeViewModel> visiting,
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
                end = AddWorkMinutes(start.Value, estimateMinutes, ResolveAssignee(dependencyEditor, detail.AssigneeMemberId), specialHolidays);
                return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
            }

            if (end != null)
            {
                start = SubtractWorkMinutes(end.Value, estimateMinutes, ResolveAssignee(dependencyEditor, detail.AssigneeMemberId), specialHolidays);
                return AddSchedule(node, schedules, visiting, start.Value, end.Value, isEndOnly);
            }

            var predecessorEnd = GetPredecessors(node, dependencyEditor)
                .Select(predecessor => CalculateSchedule(predecessor, dependencyEditor, schedules, visiting, specialHolidays)?.EndDateTime)
                .Where(value => value != null)
                .DefaultIfEmpty(DateTime.Today)
                .Max();

            start = predecessorEnd ?? DateTime.Today;
            if (node.Status == TaskNodeViewModel.TaskStatus.Ready && start < DateTime.Now)
            {
                start = DateTime.Now;
            }

            end = AddWorkMinutes(start.Value, estimateMinutes, ResolveAssignee(dependencyEditor, detail.AssigneeMemberId), specialHolidays);
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
            TaskNodeViewModel node,
            Dictionary<TaskNodeViewModel, GanttTaskSchedule> schedules,
            HashSet<TaskNodeViewModel> visiting,
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
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <returns>前段ノード一覧</returns>
        private static IEnumerable<TaskNodeViewModel> GetPredecessors(TaskNodeViewModel node, DependencyEditorViewModel dependencyEditor)
        {
            var inputPortIds = node.InputPorts.Select(port => port.PortGuid).ToHashSet();
            foreach (var connection in dependencyEditor.Connections.Connections)
            {
                if (inputPortIds.Contains(connection.ToPort.PortGuid))
                {
                    var predecessor = dependencyEditor.Nodes.Nodes.FirstOrDefault(item => item.OutputPorts.Contains(connection.FromPort));
                    if (predecessor != null)
                    {
                        yield return predecessor;
                    }
                }
            }
        }

        /// <summary>
        /// ガントチャート表示用予定を指定キーで並び替える
        /// </summary>
        /// <param name="schedules">予定一覧</param>
        /// <param name="sortKey">並び替えキー</param>
        /// <param name="dependencyOrder">依存順インデックス</param>
        /// <returns>並び替え済み予定一覧</returns>
        private static IEnumerable<GanttTaskSchedule> SortSchedules(
            IEnumerable<GanttTaskSchedule> schedules,
            GanttSortKey sortKey,
            IReadOnlyDictionary<TaskNodeViewModel, int> dependencyOrder
        )
        {
            return sortKey switch
            {
                GanttSortKey.TaskName => schedules
                    .OrderBy(item => item.Node.Detail.TaskName)
                    .ThenBy(item => dependencyOrder.GetValueOrDefault(item.Node, int.MaxValue)),
                GanttSortKey.StartDateTime => schedules
                    .OrderBy(item => item.HasSchedule ? item.StartDateTime : DateTime.MaxValue)
                    .ThenBy(item => item.Node.Detail.TaskName),
                GanttSortKey.EndDateTime => schedules
                    .OrderBy(item => item.HasSchedule ? item.EndDateTime : DateTime.MaxValue)
                    .ThenBy(item => item.Node.Detail.TaskName),
                _ => schedules
                    .OrderBy(item => dependencyOrder.GetValueOrDefault(item.Node, int.MaxValue))
                    .ThenBy(item => item.Node.Detail.TaskName)
            };
        }

        /// <summary>
        /// 依存関係に基づく表示順インデックスを作成する
        /// </summary>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <returns>タスクノード別依存順インデックス</returns>
        private static Dictionary<TaskNodeViewModel, int> CreateDependencyOrderMap(DependencyEditorViewModel dependencyEditor)
        {
            var orderedNodes = new List<TaskNodeViewModel>();
            var visited = new HashSet<TaskNodeViewModel>();
            foreach (var node in dependencyEditor.Nodes.Nodes)
            {
                VisitDependencyOrder(node, dependencyEditor, visited, orderedNodes);
            }

            return orderedNodes
                .Select((node, index) => new { node, index })
                .ToDictionary(item => item.node, item => item.index);
        }

        /// <summary>
        /// 前段タスクを優先して依存順一覧へノードを追加する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="visited">訪問済みノード集合</param>
        /// <param name="orderedNodes">依存順ノード一覧</param>
        private static void VisitDependencyOrder(
            TaskNodeViewModel node,
            DependencyEditorViewModel dependencyEditor,
            HashSet<TaskNodeViewModel> visited,
            List<TaskNodeViewModel> orderedNodes
        )
        {
            if (!visited.Add(node))
            {
                return;
            }

            foreach (var predecessor in GetPredecessors(node, dependencyEditor))
            {
                VisitDependencyOrder(predecessor, dependencyEditor, visited, orderedNodes);
            }

            orderedNodes.Add(node);
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
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者</returns>
        private static TeamMemberViewModel? ResolveAssignee(DependencyEditorViewModel dependencyEditor, Guid? memberId)
        {
            if (memberId == null || dependencyEditor.TeamMembers == null)
            {
                return null;
            }

            return dependencyEditor.TeamMembers.FirstOrDefault(member => member.MemberId == memberId.Value);
        }

        /// <summary>
        /// 担当者名を解決する
        /// </summary>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者名</returns>
        private static string ResolveAssigneeName(DependencyEditorViewModel dependencyEditor, Guid? memberId)
        {
            return ResolveAssignee(dependencyEditor, memberId)?.DisplayText ?? "(未担当)";
        }

        /// <summary>
        /// 担当者イニシャルを解決する
        /// </summary>
        /// <param name="dependencyEditor">対象依存関係編集</param>
        /// <param name="memberId">担当者ID</param>
        /// <returns>担当者イニシャル</returns>
        private static string ResolveAssigneeInitials(DependencyEditorViewModel dependencyEditor, Guid? memberId)
        {
            return ResolveAssignee(dependencyEditor, memberId)?.Initials ?? "";
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
        /// 遅い日時を取得する
        /// </summary>
        /// <param name="left">比較対象日時1</param>
        /// <param name="right">比較対象日時2</param>
        /// <returns>遅い日時</returns>
        private static DateTime Max(DateTime left, DateTime right)
        {
            return left >= right ? left : right;
        }

        /// <summary>
        /// 早い日時を取得する
        /// </summary>
        /// <param name="left">比較対象日時1</param>
        /// <param name="right">比較対象日時2</param>
        /// <returns>早い日時</returns>
        private static DateTime Min(DateTime left, DateTime right)
        {
            return left <= right ? left : right;
        }

        /// <summary>
        /// 内部算定済み予定
        /// </summary>
        private sealed record GanttTaskSchedule(
            TaskNodeViewModel Node,
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
            public static GanttTaskSchedule CreateSchedule(TaskNodeViewModel node, DateTime start, DateTime end, bool isEndOnly)
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
            public static GanttTaskSchedule CreateError(TaskNodeViewModel node, DateTime baseDateTime, string errorText)
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
