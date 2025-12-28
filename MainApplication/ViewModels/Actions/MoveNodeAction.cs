using MainApplication.ViewModels.Core;

namespace MainApplication.ViewModels.Actions
{
    public class MoveNodeAction : IUndoableAction
    {
        public string ActionType => "MoveNode";
        public string Description => $"タスクを移動([{_oldX}, {_oldY}]→[{_newX}, {_newY}])";
        private readonly NodeViewModel _node;
        private readonly double _oldX;
        private readonly double _oldY;
        private readonly double _newX;
        private readonly double _newY;

        public MoveNodeAction(NodeViewModel node, double oldX, double oldY, double newX, double newY)
        {
            _node = node;
            _oldX = oldX;
            _oldY = oldY;
            _newX = newX;
            _newY = newY;
        }

        public void Undo()
        {
            _node.X = _oldX;
            _node.Y = _oldY;
        }

        public void Redo()
        {
            _node.X = _newX;
            _node.Y = _newY;
        }
    }
}