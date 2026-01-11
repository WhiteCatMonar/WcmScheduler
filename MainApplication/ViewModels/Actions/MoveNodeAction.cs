using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Windows;

namespace MainApplication.ViewModels.Actions
{
    public class MoveNodeAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "MoveNode";

        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => $"タスクを移動([{_oldPosition}]→[{_newPosition}])";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly NodeViewModel _node;
        private readonly Point _oldPosition;
        private readonly Point _newPosition;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード移動アクションを生成する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="oldPosition">移動前の座標</param>
        /// <param name="newPosition">移動後の座標</param>
        public MoveNodeAction(NodeViewModel node, Point oldPosition, Point newPosition)
        {
            _node = node;
            _oldPosition = oldPosition;
            _newPosition = newPosition;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの位置を移動前の座標に戻す
        /// </summary>
        public void Undo()
        {
            _node.Position = _oldPosition;
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの位置を移動後の座標に再設定する
        /// </summary>
        public void Redo()
        {
            _node.Position = _newPosition;
        }
    }
}

/* --- End of file --- */
