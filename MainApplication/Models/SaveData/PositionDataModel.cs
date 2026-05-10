using System.Text.Json.Serialization;

namespace MainApplication.Models.SaveData
{
    /// <summary>
    /// ノード座標の保存データ。
    /// </summary>
    public class PositionDataModel
    {
        /// <summary>
        /// X座標。
        /// </summary>
        [JsonPropertyName("x")]
        public double X { get; set; }

        /// <summary>
        /// Y座標。
        /// </summary>
        [JsonPropertyName("y")]
        public double Y { get; set; }
    }
}

/* --- End of file --- */
