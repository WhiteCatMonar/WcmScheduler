using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// タスク編集機能の保存データ。
    /// </summary>
    public class TaskEditorDataModel
    {
        /// <summary>
        /// 対象プロジェクトのID。
        /// </summary>
        [JsonPropertyName("project-id")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// エディタ内に存在するすべてのノード。
        /// </summary>
        [JsonPropertyName("nodes")]
        public List<NodeDataModel> Nodes { get; set; } = [];

        /// <summary>
        /// エディタ内に存在するすべてのノード間接続情報。
        /// </summary>
        [JsonPropertyName("connections")]
        public List<ConnectionDataModel> Connections { get; set; } = [];
    }
}

/* --- End of file --- */
