using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
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

        private readonly ObservableCollection<TaskNodeViewModel> _nodes;
        private readonly TaskNodeViewModel _node;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード追加アクションを生成する
        /// </summary>
        public AddNodeAction(ObservableCollection<TaskNodeViewModel> nodes, TaskNodeViewModel node)
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
