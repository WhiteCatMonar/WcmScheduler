using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// チームメンバーの保存データ
    /// </summary>
    public class MemberDataModel
    {
        /// <summary>
        /// メンバーを一意に識別するID
        /// </summary>
        [JsonPropertyName("member-id")]
        public Guid MemberId { get; set; }

        /// <summary>
        /// UIに表示するメンバー名
        /// </summary>
        [JsonPropertyName("display-name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 日曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("sunday-work-time-minutes")]
        public int SundayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 月曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("monday-work-time-minutes")]
        public int MondayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 火曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("tuesday-work-time-minutes")]
        public int TuesdayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 水曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("wednesday-work-time-minutes")]
        public int WednesdayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 木曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("thursday-work-time-minutes")]
        public int ThursdayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 金曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("friday-work-time-minutes")]
        public int FridayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 土曜日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("saturday-work-time-minutes")]
        public int SaturdayWorkTimeMinutes { get; set; } = 480;

        /// <summary>
        /// 特別休日のデフォルト作業可能時間。単位は分
        /// </summary>
        [JsonPropertyName("special-holiday-work-time-minutes")]
        public int SpecialHolidayWorkTimeMinutes { get; set; }
    }
}

/* --- End of file --- */
