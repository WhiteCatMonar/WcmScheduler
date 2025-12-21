using MainApplication.ViewModels.Infrastructure;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class AddConnectionAction : IUndoableAction
    {
        public string ActionType => "AddConnection";
        public string Description => "接続線を追加";
        private readonly ObservableCollection<ConnectionViewModel> _connections;
        private readonly ConnectionViewModel _connection;

        public AddConnectionAction(ObservableCollection<ConnectionViewModel> connections,
                                   ConnectionViewModel connection)
        {
            _connections = connections;
            _connection = connection;
        }

        public void Undo()
        {
            _connections.Remove(_connection);
        }

        public void Redo()
        {
            _connections.Add(_connection);
        }
    }
}
