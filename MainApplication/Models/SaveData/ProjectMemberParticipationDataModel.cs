using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// プロジェクト単位のメンバー参加期間保存データ
    /// </summary>
    public class ProjectMemberParticipationDataModel
    {
        /// <summary>
        /// 対象プロジェクトのID
        /// </summary>
        [JsonPropertyName("project-id")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// 対象メンバーのID
        /// </summary>
        [JsonPropertyName("member-id")]
        public Guid MemberId { get; set; }

        /// <summary>
        /// プロジェクト参加開始日
        /// </summary>
        [JsonPropertyName("participation-start-date")]
        public DateOnly? ParticipationStartDate { get; set; }

        /// <summary>
        /// プロジェクト参加終了日
        /// </summary>
        [JsonPropertyName("participation-end-date")]
        public DateOnly? ParticipationEndDate { get; set; }
    }
}

/* --- End of file --- */
