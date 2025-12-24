using MainApplication.ViewModels.Infrastructure;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class DeleteConnectionAction : IUndoableAction
    {
        public string ActionType => "DeleteConnection";
        public string Description => "接続線を削除";
        private readonly ObservableCollection<ConnectionViewModel> _connections;
        private readonly ConnectionViewModel _connection;

        public DeleteConnectionAction(ObservableCollection<ConnectionViewModel> connections,
                                      ConnectionViewModel connection)
        {
            _connections = connections;
            _connection = connection;
        }

        public void Undo()
        {
            _connections.Add(_connection);
            _connection.FromPort.ConnectedConnections.Add(_connection);
            _connection.ToPort.ConnectedConnections.Add(_connection);

        }

        public void Redo()
        {
            _connection.ToPort.ConnectedConnections.Remove(_connection);
            _connection.FromPort.ConnectedConnections.Remove(_connection);
            _connections.Remove(_connection);
        }
    }

}
