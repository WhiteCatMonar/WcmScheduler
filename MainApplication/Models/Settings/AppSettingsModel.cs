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

        /// <summary>
        /// 自動バックアップ保持世代数。
        /// </summary>
        [JsonPropertyName("auto-backup-generation-count")]
        public int AutoBackupGenerationCount { get; set; } = 3;
    }
}

/* --- End of file --- */
