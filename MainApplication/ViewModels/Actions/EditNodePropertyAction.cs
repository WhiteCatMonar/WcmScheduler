using MainApplication.ViewModels.Infrastructure;
using System.ComponentModel;
using System.Reflection;

namespace MainApplication.ViewModels.Actions
{
    public class EditNodePropertyAction : IUndoableAction
    {
        public string ActionType => "EditNodeProperty";
        public string Description => $"{_displayName} を {_oldValue} から {_newValue} に変更";
        private readonly NodeViewModel _node;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly string _displayName;

        public EditNodePropertyAction(NodeViewModel node, string propertyName, object oldValue, object newValue)
        {
            _node = node;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;

            var prop = _node.GetType().GetProperty(_propertyName);
            var displayAttr = prop?.GetCustomAttribute<DisplayNameAttribute>();
            _displayName = displayAttr?.DisplayName ?? _propertyName;
        }

        public void Undo()
        {
            SetValue(_oldValue);
        }

        public void Redo()
        {
            SetValue(_newValue);
        }

        private void SetValue(object value)
        {
            var prop = _node.GetType().GetProperty(_propertyName);
            prop?.SetValue(_node, value);
        }

    }
}
