using MainApplication.ViewModels.Infrastructure;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class AddNodeAction : IUndoableAction
    {
        public string ActionType => "AddNode";
        public string Description => "タスクを追加";
        private readonly ObservableCollection<NodeViewModel> _nodes;
        private readonly NodeViewModel _node;

        public AddNodeAction(ObservableCollection<NodeViewModel> nodes, NodeViewModel node)
        {
            _nodes = nodes;
            _node = node;
        }

        public void Undo()
        {
            _nodes.Remove(_node);
        }

        public void Redo()
        {
            _nodes.Add(_node);
        }
    }
}