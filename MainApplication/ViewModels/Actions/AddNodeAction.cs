using MainApplication.ViewModels.Core;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class AddNodeAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "AddNode";

        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => "タスクを追加";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly ObservableCollection<NodeViewModel> _nodes;
        private readonly NodeViewModel _node;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード追加アクションを生成する
        /// </summary>
        public AddNodeAction(ObservableCollection<NodeViewModel> nodes, NodeViewModel node)
        {
            _nodes = nodes;
            _node = node;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードを削除して追加操作を取り消す
        /// </summary>
        public void Undo()
        {
            _nodes.Remove(_node);
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードを再度追加する
        /// </summary>
        public void Redo()
        {
            _nodes.Add(_node);
        }
    }
}

/* --- End of file --- */
