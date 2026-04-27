using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    public class NodeDataModel
    {
        /* ---------------------------------------------------------
         * データプロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの一意識別子
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// ノードの種類(例：TaskNode)
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "TaskNode";

        /// <summary>
        /// ノードの座標情報
        /// </summary>
        [JsonPropertyName("position")]
        public PositionDataModel Position { get; set; } = new();

        /// <summary>
        /// ノードの詳細情報(タスク名・担当者・日時など)
        /// </summary>
        [JsonPropertyName("details")]
        public NodeDetailsDataModel Details { get; set; } = new();

        /// <summary>
        /// ノードが持つポート一覧(入出力端子)
        /// </summary>
        [JsonPropertyName("ports")]
        public List<PortDataModel> Ports { get; set; } = [];
    }
}

/* --- End of file --- */
