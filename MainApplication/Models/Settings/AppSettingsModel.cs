using System.Text.Json.Serialization;

namespace MainApplication.Models.Settings
{
    /// <summary>
    /// アプリケーション全体のユーザー設定を表すモデル。
    /// </summary>
    public class AppSettingsModel
    {
        /// <summary>
        /// 最後に適用したテーマ名。
        /// </summary>
        [JsonPropertyName("last-theme-name")]
        public string? LastThemeName { get; set; }
    }
}

/* --- End of file --- */
