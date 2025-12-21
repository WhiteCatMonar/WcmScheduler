using MainApplication.ViewModels.Infrastructure;
using System.Collections.ObjectModel;

namespace MainApplication.ViewModels.Actions
{
    public class DeleteNodeAction : IUndoableAction
    {
        public string ActionType => "DeleteNode";
        public string Description => "タスクを削除";
        private readonly ObservableCollection<NodeViewModel> _nodes;
        private readonly NodeViewModel _node;

        public DeleteNodeAction(ObservableCollection<NodeViewModel> nodes, NodeViewModel node)
        {
            _nodes = nodes;
            _node = node;
        }

        public void Undo()
        {
            _nodes.Add(_node);
        }

        public void Redo()
        {
            _nodes.Remove(_node);
        }
    }
}
