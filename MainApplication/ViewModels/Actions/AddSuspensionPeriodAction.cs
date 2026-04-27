using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.ViewModels.Actions
{
    /// <summary>
    /// 中断期間追加操作を表すUndo/Redoアクション。
    /// </summary>
    public class AddSuspensionPeriodAction : IUndoableAction
    {
        private readonly NodeDetailViewModel _nodeDetail;
        private readonly SuspensionPeriodViewModel _period;
        private readonly int _index;

        /// <summary>
        /// アクション種別。
        /// </summary>
        public string ActionType => "AddSuspensionPeriod";

        /// <summary>
        /// アクションの説明。
        /// </summary>
        public string Description => "中断期間を追加";

        /// <summary>
        /// 中断期間追加アクションを生成する。
        /// </summary>
        /// <param name="nodeDetail">対象のノード詳細。</param>
        /// <param name="period">追加する中断期間。</param>
        /// <param name="index">追加位置。</param>
        public AddSuspensionPeriodAction(NodeDetailViewModel nodeDetail, SuspensionPeriodViewModel period, int index)
        {
            _nodeDetail = nodeDetail;
            _period = period;
            _index = index;
        }

        /// <summary>
        /// 中断期間追加を取り消す。
        /// </summary>
        public void Undo()
        {
            _nodeDetail.RemoveSuspensionPeriodDirect(_period);
        }

        /// <summary>
        /// 中断期間を追加する。
        /// </summary>
        public void Redo()
        {
            _nodeDetail.InsertSuspensionPeriodDirect(_period, _index);
        }
    }
}

/* --- End of file --- */
