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
        /// プロジェクト一覧
        /// </summary>
        [JsonPropertyName("projects")]
        public List<ProjectDataModel> Projects { get; set; } = [];

        /// <summary>
        /// チームメンバー一覧
        /// </summary>
        [JsonPropertyName("members")]
        public List<MemberDataModel> Members { get; set; } = [];

        /// <summary>
        /// 特別休日一覧
        /// </summary>
        [JsonPropertyName("special-holidays")]
        public List<DateOnly> SpecialHolidays { get; set; } = [];
    }
}

/* --- End of file --- */
