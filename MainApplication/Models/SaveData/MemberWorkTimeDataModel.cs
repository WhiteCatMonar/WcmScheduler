using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// 日付別作業可能時間上書き値の保存データ
    /// </summary>
    public class MemberWorkTimeDataModel
    {
        /// <summary>
        /// 対象日
        /// </summary>
        [JsonPropertyName("work-date")]
        public DateOnly WorkDate { get; set; }

        /// <summary>
        /// 作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("work-time-minutes")]
        public int WorkTimeMinutes { get; set; }
    }
}

/* --- End of file --- */
