using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// プロジェクト単位のメンバー別日付別作業可能時間の保存データ。
    /// </summary>
    public class ProjectMemberWorkTimeDataModel
    {
        /// <summary>
        /// 対象プロジェクトのID。
        /// </summary>
        [JsonPropertyName("project-id")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// 対象メンバーのID。
        /// </summary>
        [JsonPropertyName("member-id")]
        public Guid MemberId { get; set; }

        /// <summary>
        /// 対象日付。
        /// </summary>
        [JsonPropertyName("work-date")]
        public DateOnly WorkDate { get; set; }

        /// <summary>
        /// 作業可能時間。単位は分。
        /// </summary>
        [JsonPropertyName("work-time-minutes")]
        public int WorkTimeMinutes { get; set; }
    }
}

/* --- End of file --- */
