using System;

namespace MainApplication.Models.SaveData
{
    public class NodeDetailsDataModel
    {
        /* ---------------------------------------------------------
         * データプロパティ(ノード詳細情報)
         * --------------------------------------------------------- */

        /// <summary>
        /// タスク名
        /// </summary>
        public string? TaskName { get; set; }

        /// <summary>
        /// 担当者名
        /// </summary>
        public string? Person { get; set; }

        /// <summary>
        /// 開始日時(着手日時)(未設定の場合はnull)
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 終了日時(完了日時)(未設定の場合はnull)
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 作業見積時間(分単位)(未設定の場合はnull)
        /// </summary>
        public int? WorkEstimateMinutes { get; set; }

        /// <summary>
        /// 備考・コメント
        /// </summary>
        public string? Comment { get; set; }
    }
}

/* --- End of file --- */
