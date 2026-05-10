using MainApplication.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MainApplication.Views.Behaviors
{
    /// <summary>
    /// ListBoxに対して「現在の項目(IsCurrent = true)へ自動スクロールする」
    /// という動作を付与する添付ビヘイビア。
    /// 
    /// Undo/Redoの履歴ビューなど、
    /// 「現在位置が変わったら自動でスクロールしてほしい」ケースで使用する。
    /// </summary>
    public static class ListBoxAutoScrollBehavior
    {
        /* ---------------------------------------------------------
         * AutoScroll添付プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// AutoScrollを有効にするかどうかを示す添付プロパティ。
        /// trueにすると、ListBoxのItemsSourceと各項目を監視し、
        /// IsCurrent = trueの項目へ自動スクロールする。
        /// </summary>
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll", typeof(bool), typeof(ListBoxAutoScrollBehavior),
                new PropertyMetadata(false, OnAutoScrollChanged));

        public static void SetAutoScroll(DependencyObject obj, bool value) =>
            obj.SetValue(AutoScrollProperty, value);

        public static bool GetAutoScroll(DependencyObject obj) =>
            (bool)obj.GetValue(AutoScrollProperty);

        /* ---------------------------------------------------------
         * AutoScroll有効化／無効化時の処理
         * --------------------------------------------------------- */

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                {
                    /* 購読開始 */
                    listBox.Loaded += ListBox_Loaded;
                    listBox.DataContextChanged += ListBox_DataContextChanged;
                    listBox.Unloaded += ListBox_Unloaded;
                }
                else
                {
                    /* 購読解除 */
                    Detach(listBox);
                    listBox.Unloaded -= ListBox_Unloaded;
                }
            }
        }

        /* ---------------------------------------------------------
         * 購読解除処理
         * --------------------------------------------------------- */

        /// <summary>
        /// ListBoxからすべてのイベント購読を解除し、
        /// 内部で保持しているハンドラも破棄する。
        /// </summary>
        private static void Detach(ListBox listBox)
        {
            try
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.DataContextChanged -= ListBox_DataContextChanged;

                if (_collectionHandlers.TryGetValue(listBox, out var handler))
                {
                    if (listBox.ItemsSource is INotifyCollectionChanged collection)
                    {
                        collection.CollectionChanged -= handler;
                    }
                    _collectionHandlers.Remove(listBox);
                }

                var keysToRemove = _handlers.Keys.Where(k => k.listBox == listBox).ToList();
                foreach (var key in keysToRemove)
                {
                    key.item.PropertyChanged -= _handlers[key];
                    _handlers.Remove(key);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ListBoxAutoScrollBehavior] Detach error: {ex}");
            }
        }

        /* ---------------------------------------------------------
         * ListBoxイベント
         * --------------------------------------------------------- */

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                TryAttach(listBox);
                listBox.Loaded -= ListBox_Loaded;
            }
        }

        private static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                Detach(listBox);
            }
        }

        private static void ListBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                TryAttach(listBox);
            }
        }

        /* ---------------------------------------------------------
         * ItemsSourceのCollectionChangedを管理
         * --------------------------------------------------------- */

        private static readonly Dictionary<ListBox, NotifyCollectionChangedEventHandler> _collectionHandlers = [];

        /// <summary>
        /// ListBox の ItemsSource を監視し、HistoryItem の追加時に購読を行う。
        /// </summary>
        private static void TryAttach(ListBox listBox)
        {
            try
            {
                if (listBox.ItemsSource is INotifyCollectionChanged collection)
                {
                    /* 既存ハンドラを解除 */
                    if (_collectionHandlers.TryGetValue(listBox, out var existing))
                    {
                        collection.CollectionChanged -= existing;
                    }

                    /* 新しいハンドラを登録 */
                    NotifyCollectionChangedEventHandler handler = (s, args) => OnCollectionChanged(s, args, listBox);
                    _collectionHandlers[listBox] = handler;
                    collection.CollectionChanged += handler;

                    /* 既存の項目にも購読を付与 */
                    foreach (var obj in listBox.Items.OfType<UndoRedoManager.HistoryItem>())
                    {
                        Subscribe(listBox, obj);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ListBoxAutoScrollBehavior] TryAttach error: {ex}");
            }
        }

        /* ---------------------------------------------------------
         * CollectionChanged(項目追加時)
         * --------------------------------------------------------- */

        private static void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args, ListBox listBox)
        {
            if (sender is INotifyCollectionChanged)
            {

                try
                {
                    if (args.NewItems != null)
                    {
                        foreach (var obj in args.NewItems.OfType<UndoRedoManager.HistoryItem>())
                        {
                            Subscribe(listBox, obj);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ListBoxAutoScrollBehavior] OnCollectionChanged error: {ex}");
                }
            }
        }

        /* ---------------------------------------------------------
         * HistoryItemのPropertyChangedを監視し、
         * IsCurrent = trueになったら自動スクロールする
         * --------------------------------------------------------- */

        private static readonly Dictionary<(ListBox listBox, UndoRedoManager.HistoryItem item), PropertyChangedEventHandler> _handlers
            = new Dictionary<(ListBox, UndoRedoManager.HistoryItem), PropertyChangedEventHandler>();

        /// <summary>
        /// HistoryItemのIsCurrentプロパティを監視し、
        /// trueになったらScrollIntoViewで自動スクロールする。
        /// </summary>
        private static void Subscribe(ListBox listBox, UndoRedoManager.HistoryItem item)
        {
            try
            {
                var key = (listBox, item);

                /* 既存ハンドラを解除 */
                if (_handlers.TryGetValue(key, out var existing))
                {
                    item.PropertyChanged -= existing;
                }

                /* 新しいハンドラ */
                PropertyChangedEventHandler handler = (sender, ev) =>
                {
                    if (ev.PropertyName == nameof(UndoRedoManager.HistoryItem.IsCurrent))
                    {
                        if (!listBox.Dispatcher.HasShutdownStarted)
                        {
                            if (sender is UndoRedoManager.HistoryItem historyItem && historyItem.IsCurrent)
                            {

                                listBox.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    listBox.UpdateLayout();
                                    listBox.ScrollIntoView(historyItem);
                                }), System.Windows.Threading.DispatcherPriority.Render);
                            }
                        }
                    }
                };

                _handlers[key] = handler;
                item.PropertyChanged += handler;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ListBoxAutoScrollBehavior] Subscribe error: {ex}");
            }
        }
    }
}

/* --- End of file --- */
