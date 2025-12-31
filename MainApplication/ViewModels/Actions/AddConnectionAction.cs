using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class AddConnectionAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "AddConnection";

        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => "接続線を追加";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly ObservableCollection<ConnectionViewModel> _connections;
        private readonly ConnectionViewModel _connection;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線追加アクションを生成する
        /// </summary>
        public AddConnectionAction(ObservableCollection<ConnectionViewModel> connections,
                                   ConnectionViewModel connection)
        {
            _connections = connections;
            _connection = connection;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線を削除して追加操作を取り消す
        /// </summary>
        public void Undo()
        {
            _connections.Remove(_connection);
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線を再度追加する
        /// </summary>
        public void Redo()
        {
            _connections.Add(_connection);
        }
    }
}

/* --- End of file --- */
