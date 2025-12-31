using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class DeleteConnectionAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "DeleteConnection";
        
        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => "接続線を削除";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly ObservableCollection<ConnectionViewModel> _connections;
        private readonly ConnectionViewModel _connection;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線削除アクションを生成する
        /// </summary>
        public DeleteConnectionAction(ObservableCollection<ConnectionViewModel> connections,
                                      ConnectionViewModel connection)
        {
            _connections = connections;
            _connection = connection;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線を復元し、削除操作を取り消す
        /// </summary>
        public void Undo()
        {
            _connections.Add(_connection);
            _connection.FromPort.ConnectedConnections.Add(_connection);
            _connection.ToPort.ConnectedConnections.Add(_connection);

        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// 接続線を再度削除する
        /// </summary>
        public void Redo()
        {
            _connection.ToPort.ConnectedConnections.Remove(_connection);
            _connection.FromPort.ConnectedConnections.Remove(_connection);
            _connections.Remove(_connection);
        }
    }
}

/* --- End of file --- */
