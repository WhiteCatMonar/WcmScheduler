using MainApplication.ViewModels.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MainApplication.ViewModels.Infrastructure
{
    public interface IUndoableAction
    {
        void Undo();
        void Redo();
        string ActionType { get; }
        string Description { get; }
    }

    public class UndoRedoManager
    {
        private readonly Stack<IUndoableAction> _undoStack = new Stack<IUndoableAction>();
        private readonly Stack<IUndoableAction> _redoStack = new Stack<IUndoableAction>();

        public bool CanUndo => _undoStack.Any();
        public bool CanRedo => _redoStack.Any();

        public class HistoryItem : INotifyPropertyChanged
        {
            public string Description { get; set; }
            public string ActionType { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;

            private bool _isCurrent;
            public bool IsCurrent { get => _isCurrent; set { _isCurrent = value; OnPropertyChanged(nameof(IsCurrent)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<HistoryItem> History { get; } = new ObservableCollection<HistoryItem>();
        public bool IsApplyingHistory { get; private set; }
        public void Execute(IUndoableAction action)
        {
            IsApplyingHistory = true;
            action.Redo();
            IsApplyingHistory = false;
            _undoStack.Push(action);
            _redoStack.Clear();

            int currentIndex = History.IndexOf(History.FirstOrDefault(h => h.IsCurrent));

            var item = new HistoryItem
            {
                Description = action.Description,
                ActionType = action.ActionType,
                Timestamp = DateTime.Now
            };

            if (currentIndex < 0)
            {
                History.Clear();
                History.Insert(0, item);
            }
            else
            {
                History.Insert(currentIndex, item);

                /* Redo不可能になった履歴を削除 */
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    History.RemoveAt(i);
                }
            }
            _actionToHistory[action] = item;

            UpdateCurrentHistory();
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }
            var action = _undoStack.Pop();

            IsApplyingHistory = true;
            action.Undo();
            IsApplyingHistory = false;
            _redoStack.Push(action);

            UpdateCurrentHistory();
        }

        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }
            var action = _redoStack.Pop();

            IsApplyingHistory = true;
            action.Redo();
            IsApplyingHistory = false;
            _undoStack.Push(action);

            UpdateCurrentHistory();
        }

        private Dictionary<IUndoableAction, HistoryItem> _actionToHistory = new Dictionary<IUndoableAction, HistoryItem>();
        public event EventHandler<HistoryItem> CurrentHistoryChanged;
        private void UpdateCurrentHistory()
        {
            foreach (var item in History)
            {
                item.IsCurrent = false;
            }

            HistoryItem currentItem = null;
            if (_undoStack.Count > 0)
            {
                var currentAction = _undoStack.Peek();
                if (_actionToHistory.TryGetValue(currentAction, out var found))
                {
                    found.IsCurrent = true;
                    currentItem = found;
                }
            }

            CurrentHistoryChanged?.Invoke(this, currentItem);
        }

        public void MoveToHistory(HistoryItem target)
        {
            if (target == null) return;

            int targetIndex = History.IndexOf(target);
            if (targetIndex < 0) return;

            int currentIndex = History.IndexOf(History.FirstOrDefault(h => h.IsCurrent));

            if (currentIndex < 0)
            {
                /* 現在位置が未設定なら最新に移動 */
                currentIndex = 0;
            }

            /* 現在位置より過去に戻す場合 → Undoを繰り返す */
            while (currentIndex < targetIndex && CanUndo)
            {
                Undo();
                currentIndex = History.IndexOf(History.FirstOrDefault(h => h.IsCurrent));
            }

            /* 現在位置より未来に進める場合 → Redoを繰り返す */
            while (currentIndex > targetIndex && CanRedo)
            {
                Redo();
                currentIndex = History.IndexOf(History.FirstOrDefault(h => h.IsCurrent));
            }
        }

    }
}
