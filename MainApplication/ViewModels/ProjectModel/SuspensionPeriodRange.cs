using System;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// 正規化済み中断期間を表す読み取り専用データ。
    /// </summary>
    public class SuspensionPeriodRange
    {
        /// <summary>
        /// 中断開始日時。
        /// </summary>
        public DateTime StartDateTime { get; }

        /// <summary>
        /// 中断終了日時。
        /// </summary>
        public DateTime EndDateTime { get; }

        /// <summary>
        /// 正規化済み中断期間を生成する。
        /// </summary>
        /// <param name="startDateTime">中断開始日時。</param>
        /// <param name="endDateTime">中断終了日時。</param>
        public SuspensionPeriodRange(DateTime startDateTime, DateTime endDateTime)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }
    }
}

/* --- End of file --- */
