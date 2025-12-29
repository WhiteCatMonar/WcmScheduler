using System.Collections.Generic;

namespace MainApplication.Models.SaveData
{
    public class TaskEditorDataModel
    {
        /* ---------------------------------------------------------
         * データプロパティ(タスクエディタ全体の保存データ)
         * --------------------------------------------------------- */

        /// <summary>
        /// エディタ内に存在するすべてのノード
        /// </summary>
        public List<NodeDataModel> Nodes { get; set; } = new List<NodeDataModel>();

        /// <summary>
        /// エディタ内に存在するすべてのノード間接続情報
        /// </summary>
        public List<ConnectionDataModel> Connections { get; set; } = new List<ConnectionDataModel>();
    }
}

/* --- End of file --- */
