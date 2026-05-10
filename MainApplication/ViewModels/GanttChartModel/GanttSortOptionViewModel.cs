namespace MainApplication.ViewModels.GanttChartModel
{
    /// <summary>
    /// ガントチャートの並び替えキー
    /// </summary>
    public enum GanttSortKey
    {
        /// <summary>
        /// 依存順
        /// </summary>
        Dependency,

        /// <summary>
        /// タスク名
        /// </summary>
        TaskName,

        /// <summary>
        /// 開始日時
        /// </summary>
        StartDateTime,

        /// <summary>
        /// 終了日時
        /// </summary>
        EndDateTime
    }

    /// <summary>
    /// ガントチャートの並び替え選択肢を表すViewModel
    /// </summary>
    public class GanttSortOptionViewModel
    {
        /// <summary>
        /// 並び替え選択肢を生成する
        /// </summary>
        /// <param name="sortKey">並び替えキー</param>
        /// <param name="displayText">表示文字列</param>
        public GanttSortOptionViewModel(GanttSortKey sortKey, string displayText)
        {
            SortKey = sortKey;
            DisplayText = displayText;
        }

        /// <summary>
        /// 並び替えキー
        /// </summary>
        public GanttSortKey SortKey { get; }

        /// <summary>
        /// 表示文字列
        /// </summary>
        public string DisplayText { get; }
    }
}

/* --- End of file --- */
