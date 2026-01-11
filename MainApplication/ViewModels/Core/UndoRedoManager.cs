using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// Undo/Redo操作を管理するクラス。
    /// アクションの実行・取り消し・やり直し、履歴管理を行う。
    /// </summary>
    public interface IUndoableAction
    {
        /// <summary>アクションを取り消す</summary>
        void Undo();

        /// <summary>アクションを再実行する</summary>
        void Redo();

        /// <summary>アクション種別(識別用)</summary>
        string ActionType { get; }

        /// <summary>アクションの説明(履歴表示用)</summary>
        string Description { get; }
    }

    public class UndoRedoManager
    {
        /* ---------------------------------------------------------
         * Undo/Redoスタック
         * --------------------------------------------------------- */

        private readonly Stack<IUndoableAction> _undoStack = new();
        private readonly Stack<IUndoableAction> _redoStack = new();

        /// <summary>Undoが可能かどうか</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Redoが可能かどうか</summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// 履歴リストに表示されるアイテム。
        /// アクションの説明・種別・実行時刻・現在位置フラグを保持する。
        /// </summary>
        public class HistoryItem : ViewModelBase
        {
            /// <summary>アクションの説明</summary>
            public required string Description { get; set; }

            /// <summary>アクション種別</summary>
            public required string ActionType { get; set; }

            /// <summary>アクション実行時刻</summary>
            public DateTime Timestamp { get; set; } = DateTime.Now;

            private bool _isCurrent;

            /// <summary>この履歴が現在位置かどうか</summary>
            public bool IsCurrent {
                get => _isCurrent;
                set => SetProperty(ref _isCurrent, value);
            }
        }

        /* ---------------------------------------------------------
         * 履歴管理
         * --------------------------------------------------------- */

        /// <summary>履歴アイテム一覧(UI表示用)</summary>
        public ObservableCollection<HistoryItem> History { get; } = [];
        
        /// <summary>
        /// 履歴適用中かどうか(Undo/Redo実行中に副作用を防ぐため)
        /// </summary>
        public bool IsApplyingHistory { get; private set; }
        
        /* ---------------------------------------------------------
         * アクション実行
         * --------------------------------------------------------- */

        /// <summary>
        /// 新しいアクションを実行し、Undoスタックに積む。
        /// 履歴リストも更新する。
        /// </summary>
        public void Execute(IUndoableAction action)
        {
            IsApplyingHistory = true;
            action.Redo();
            IsApplyingHistory = false;
            _undoStack.Push(action);
            _redoStack.Clear();

            HistoryItem? firstItem = History.FirstOrDefault(h => h.IsCurrent);
            int currentIndex = (firstItem is null) ? -1
                                                   : History.IndexOf(firstItem);

            var item = new HistoryItem
            {
                Description = action.Description,
                ActionType = action.ActionType,
                Timestamp = DateTime.Now
            };

            if (currentIndex < 0)
            {
                /* 初回実行 */
                History.Clear();
                History.Insert(0, item);
            }
            else
            {
                /* 現在位置に挿入 */
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

        /* ---------------------------------------------------------
         * Undo/Redo
         * --------------------------------------------------------- */

        /// <summary>Undoを実行する</summary>
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

        /// <summary>Redoを実行する</summary>
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

        /* ---------------------------------------------------------
         * 履歴位置管理
         * --------------------------------------------------------- */

        private readonly Dictionary<IUndoableAction, HistoryItem> _actionToHistory = [];

        /// <summary>現在位置が変更されたときに通知されるイベント</summary>
        public event EventHandler<HistoryItem?>? CurrentHistoryChanged;

        /// <summary>
        /// Undo/Redoスタックの状態に基づいて現在位置を更新する
        /// </summary>
        private void UpdateCurrentHistory()
        {
            foreach (var item in History)
            {
                item.IsCurrent = false;
            }

            HistoryItem? currentItem = null;
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

        /* ---------------------------------------------------------
         * 任意の履歴位置へ移動
         * --------------------------------------------------------- */

        /// <summary>
        /// 指定した履歴アイテムの位置まで Undo/Redoを繰り返して移動する
        /// </summary>
        public void MoveToHistory(HistoryItem? target)
        {
            if (target == null)
            {
                return;
            }

            int targetIndex = History.IndexOf(target);
            if (targetIndex < 0)
            {
                return;
            }

            HistoryItem? firstItem = History.FirstOrDefault(h => h.IsCurrent);
            int currentIndex = (firstItem is null) ? -1
                                                   : History.IndexOf(firstItem);

            if (currentIndex < 0)
            {
                /* 現在位置が未設定なら最新に移動 */
                currentIndex = 0;
            }

            /* 現在位置より過去に戻す場合 → Undoを繰り返す */
            while (currentIndex < targetIndex && CanUndo)
            {
                Undo();
                firstItem = History.FirstOrDefault(h => h.IsCurrent);
                currentIndex = (firstItem is null) ? -1
                                                   : History.IndexOf(firstItem);
            }

            /* 現在位置より未来に進める場合 → Redoを繰り返す */
            while (currentIndex > targetIndex && CanRedo)
            {
                Redo();
                firstItem = History.FirstOrDefault(h => h.IsCurrent);
                currentIndex = (firstItem is null) ? -1
                                                   : History.IndexOf(firstItem);
            }
        }

        /* ---------------------------------------------------------
         * 全履歴クリア
         * --------------------------------------------------------- */

        /// <summary>Undo/Redoスタックと履歴をすべてクリアする</summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            History.Clear();
        }
    }
}

/* --- End of file --- */
