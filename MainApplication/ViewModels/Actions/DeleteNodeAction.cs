using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class DeleteNodeAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "DeleteNode";

        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => "タスクを削除";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly ObservableCollection<NodeViewModel> _nodes;
        private readonly NodeViewModel _node;
        
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード削除アクションを生成する
        /// </summary>
        public DeleteNodeAction(ObservableCollection<NodeViewModel> nodes, NodeViewModel node)
        {
            _nodes = nodes;
            _node = node;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードを復元し、削除操作を取り消す
        /// </summary>
        public void Undo()
        {
            _nodes.Add(_node);
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードを再度削除する
        /// </summary>
        public void Redo()
        {
            _nodes.Remove(_node);
        }
    }
}

/* --- End of file --- */
