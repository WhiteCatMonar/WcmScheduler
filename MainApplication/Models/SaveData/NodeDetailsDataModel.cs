using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// ノード詳細情報の保存データ。
    /// </summary>
    public class NodeDetailsDataModel
    {
        /// <summary>
        /// タスク名。
        /// </summary>
        [JsonPropertyName("task-name")]
        public string? TaskName { get; set; }

        /// <summary>
        /// 旧形式の担当者名。読み込み時は未担当扱いにする。
        /// </summary>
        [JsonPropertyName("person")]
        public string? Person { get; set; }

        /// <summary>
        /// 担当者メンバーID。
        /// </summary>
        [JsonPropertyName("assignee-member-id")]
        public Guid? AssigneeMemberId { get; set; }

        /// <summary>
        /// 作業協力者メンバーID一覧。
        /// </summary>
        [JsonPropertyName("collaborator-member-ids")]
        public List<Guid> CollaboratorMemberIds { get; set; } = [];

        /// <summary>
        /// 開始日時。
        /// </summary>
        [JsonPropertyName("start-date-time")]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 終了日時。
        /// </summary>
        [JsonPropertyName("end-date-time")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 作業見積時間。単位は分。
        /// </summary>
        [JsonPropertyName("work-estimate-minutes")]
        public int? WorkEstimateMinutes { get; set; }

        /// <summary>
        /// 中断期間一覧。
        /// </summary>
        [JsonPropertyName("suspension-periods")]
        public List<SuspensionPeriodDataModel> SuspensionPeriods { get; set; } = [];

        /// <summary>
        /// 備考コメント。
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}

/* --- End of file --- */
