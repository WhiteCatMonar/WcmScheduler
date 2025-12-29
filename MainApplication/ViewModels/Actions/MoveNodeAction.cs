using MainApplication.ViewModels.Core;

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
        public string Description => $"タスクを移動([{_oldX}, {_oldY}]→[{_newX}, {_newY}])";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly NodeViewModel _node;
        private readonly double _oldX;
        private readonly double _oldY;
        private readonly double _newX;
        private readonly double _newY;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード移動アクションを生成する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="oldX">移動前のX座標</param>
        /// <param name="oldY">移動前のY座標</param>
        /// <param name="newX">移動後のX座標</param>
        /// <param name="newY">移動後のY座標</param>
        public MoveNodeAction(NodeViewModel node, double oldX, double oldY, double newX, double newY)
        {
            _node = node;
            _oldX = oldX;
            _oldY = oldY;
            _newX = newX;
            _newY = newY;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの位置を移動前の座標に戻す
        /// </summary>
        public void Undo()
        {
            _node.X = _oldX;
            _node.Y = _oldY;
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの位置を移動後の座標に再設定する
        /// </summary>
        public void Redo()
        {
            _node.X = _newX;
            _node.Y = _newY;
        }
    }
}

/* --- End of file --- */
