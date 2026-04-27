using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// ポート情報の保存データ。
    /// </summary>
    public class PortDataModel
    {
        /// <summary>
        /// ポートの一意識別子。
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ポート名。
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// ポート種別。
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }
    }
}

/* --- End of file --- */
