using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.ViewModels.Actions
{
    /// <summary>
    /// 中断期間削除操作を表すUndo/Redoアクション。
    /// </summary>
    public class DeleteSuspensionPeriodAction : IUndoableAction
    {
        private readonly NodeDetailViewModel _nodeDetail;
        private readonly SuspensionPeriodViewModel _period;
        private readonly int _index;

        /// <summary>
        /// アクション種別。
        /// </summary>
        public string ActionType => "DeleteSuspensionPeriod";

        /// <summary>
        /// アクションの説明。
        /// </summary>
        public string Description => "中断期間を削除";

        /// <summary>
        /// 中断期間削除アクションを生成する。
        /// </summary>
        /// <param name="nodeDetail">対象のノード詳細。</param>
        /// <param name="period">削除する中断期間。</param>
        /// <param name="index">削除前の位置。</param>
        public DeleteSuspensionPeriodAction(NodeDetailViewModel nodeDetail, SuspensionPeriodViewModel period, int index)
        {
            _nodeDetail = nodeDetail;
            _period = period;
            _index = index;
        }

        /// <summary>
        /// 中断期間を復元する。
        /// </summary>
        public void Undo()
        {
            _nodeDetail.InsertSuspensionPeriodDirect(_period, _index);
        }

        /// <summary>
        /// 中断期間を削除する。
        /// </summary>
        public void Redo()
        {
            _nodeDetail.RemoveSuspensionPeriodDirect(_period);
        }
    }
}

/* --- End of file --- */
