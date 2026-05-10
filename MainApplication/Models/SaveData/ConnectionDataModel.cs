using System;
using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// ノード間接続線の保存データ。
    /// </summary>
    public class ConnectionDataModel
    {
        /// <summary>
        /// 接続線自体のID。
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 接続元ポートのID。
        /// </summary>
        [JsonPropertyName("from-port-id")]
        public required string FromPortId { get; set; }

        /// <summary>
        /// 接続先ポートのID。
        /// </summary>
        [JsonPropertyName("to-port-id")]
        public required string ToPortId { get; set; }
    }
}

/* --- End of file --- */
