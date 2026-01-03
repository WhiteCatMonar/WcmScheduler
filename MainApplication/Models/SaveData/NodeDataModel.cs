using System;
using System.Collections.Generic;

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
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// ノードの種類(例：TaskNode)
        /// </summary>
        public string Type { get; set; } = "TaskNode";

        /// <summary>
        /// ノードの座標情報
        /// </summary>
        public PositionDataModel Position { get; set; } = new();

        /// <summary>
        /// ノードの詳細情報(タスク名・担当者・日時など)
        /// </summary>
        public NodeDetailsDataModel Details { get; set; } = new();

        /// <summary>
        /// ノードが持つポート一覧(入出力端子)
        /// </summary>
        public List<PortDataModel> Ports { get; set; } = [];
    }
}

/* --- End of file --- */
