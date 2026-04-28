using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// プロジェクト1件分の保存データ
    /// </summary>
    public class ProjectDataModel
    {
        /// <summary>
        /// プロジェクトID
        /// </summary>
        [JsonPropertyName("project-id")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// プロジェクト名
        /// </summary>
        [JsonPropertyName("project-name")]
        public string? ProjectName { get; set; }

        /// <summary>
        /// タスク編集データ
        /// </summary>
        [JsonPropertyName("task-editor")]
        public TaskEditorDataModel TaskEditor { get; set; } = new();

        /// <summary>
        /// プロジェクト内メンバー情報一覧
        /// </summary>
        [JsonPropertyName("member-info")]
        public List<ProjectMemberInfoDataModel> MemberInfo { get; set; } = [];
    }
}

/* --- End of file --- */
