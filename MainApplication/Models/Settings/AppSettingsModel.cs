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

        /// <summary>
        /// ガントチャート表示設定
        /// </summary>
        [JsonPropertyName("gantt-chart")]
        public GanttChartSettingsModel GanttChart { get; set; } = new();
    }

    /// <summary>
    /// ガントチャート表示状態のユーザー設定を表すモデル
    /// </summary>
    public class GanttChartSettingsModel
    {
        /// <summary>
        /// 担当者フィルタで選択中のメンバーID一覧
        /// </summary>
        [JsonPropertyName("assignee-member-ids")]
        public List<Guid> AssigneeMemberIds { get; set; } = [];

        /// <summary>
        /// 作業協力者フィルタで選択中のメンバーID一覧
        /// </summary>
        [JsonPropertyName("collaborator-member-ids")]
        public List<Guid> CollaboratorMemberIds { get; set; } = [];

        /// <summary>
        /// ステータスフィルタで選択中のステータス名一覧
        /// </summary>
        [JsonPropertyName("statuses")]
        public List<string> Statuses { get; set; } = [];

        /// <summary>
        /// 並び替えキー
        /// </summary>
        [JsonPropertyName("sort-key")]
        public string SortKey { get; set; } = "Dependency";
    }
}

/* --- End of file --- */
