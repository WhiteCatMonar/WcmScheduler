using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// 保存データのルート情報
    /// </summary>
    public class RootSaveDataModel
    {
        /// <summary>
        /// タスクエディタ全体の保存データ
        /// </summary>
        [JsonPropertyName("task-editor")]
        public required TaskEditorDataModel TaskEditor { get; set; }

        /// <summary>
        /// チームメンバー一覧
        /// </summary>
        [JsonPropertyName("members")]
        public List<MemberDataModel> Members { get; set; } = [];

        /// <summary>
        /// プロジェクト単位のメンバー別作業可能時間一覧
        /// </summary>
        [JsonPropertyName("project-member-work-times")]
        public List<ProjectMemberWorkTimeDataModel> ProjectMemberWorkTimes { get; set; } = [];

        /// <summary>
        /// プロジェクト単位のメンバー参加期間一覧
        /// </summary>
        [JsonPropertyName("project-member-participations")]
        public List<ProjectMemberParticipationDataModel> ProjectMemberParticipations { get; set; } = [];
    }
}

/* --- End of file --- */
