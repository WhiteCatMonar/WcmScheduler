using System;
using System.Windows;

namespace MainApplication.ViewModels.Core
{
    public class EditableField<T>
    {
        public string Name { get; }
        public Func<T> Getter { get; }
        public Action<T> Setter { get; }

        private T _oldValue;

        public EditableField(string name, Func<T> getter, Action<T> setter)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            _oldValue = getter();
        }

        public bool TryCommit(Action<string, T, T> commitHistory)
        {
            var newValue = Getter();
            if (!Equals(_oldValue, newValue))
            {
                commitHistory(Name, _oldValue, newValue);
                _oldValue = newValue;
                return true;
            }
            return false;
        }

    }
}
