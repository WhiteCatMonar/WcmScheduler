using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// プロジェクト内メンバー情報の保存データ
    /// </summary>
    public class ProjectMemberInfoDataModel
    {
        /// <summary>
        /// 対象メンバーID
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

        /// <summary>
        /// 日付別作業可能時間上書き値一覧
        /// </summary>
        [JsonPropertyName("work-times")]
        public List<MemberWorkTimeDataModel> WorkTimes { get; set; } = [];
    }
}

/* --- End of file --- */
