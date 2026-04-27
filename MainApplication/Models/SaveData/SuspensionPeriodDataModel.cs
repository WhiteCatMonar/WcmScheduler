using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// タスクの中断期間を表す保存データ。
    /// </summary>
    public class SuspensionPeriodDataModel
    {
        /// <summary>
        /// 中断開始日時。
        /// </summary>
        [JsonPropertyName("start-date-time")]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 中断終了日時。
        /// </summary>
        [JsonPropertyName("end-date-time")]
        public DateTime? EndDateTime { get; set; }
    }
}

/* --- End of file --- */
