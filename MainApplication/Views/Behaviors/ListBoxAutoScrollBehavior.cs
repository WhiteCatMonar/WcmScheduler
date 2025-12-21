using MainApplication.ViewModels.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MainApplication.Views.Behaviors
{
    public static class ListBoxAutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll", typeof(bool), typeof(ListBoxAutoScrollBehavior),
                new PropertyMetadata(false, OnAutoScrollChanged));

        public static void SetAutoScroll(DependencyObject obj, bool value) =>
            obj.SetValue(AutoScrollProperty, value);
        public static bool GetAutoScroll(DependencyObject obj) =>
            (bool)obj.GetValue(AutoScrollProperty);

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

        private static readonly Dictionary<ListBox, NotifyCollectionChangedEventHandler> _collectionHandlers
            = new Dictionary<ListBox, NotifyCollectionChangedEventHandler>();
        private static void TryAttach(ListBox listBox)
        {
            try
            {
                if (listBox.ItemsSource is INotifyCollectionChanged collection)
                {
                    if (_collectionHandlers.TryGetValue(listBox, out var existing))
                    {
                        collection.CollectionChanged -= existing;
                    }

                    NotifyCollectionChangedEventHandler handler = (s, args) => OnCollectionChanged(s, args, listBox);
                    _collectionHandlers[listBox] = handler;
                    collection.CollectionChanged += handler;

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

        private static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args, ListBox listBox)
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

        private static readonly Dictionary<(ListBox listBox, UndoRedoManager.HistoryItem item), PropertyChangedEventHandler> _handlers
            = new Dictionary<(ListBox, UndoRedoManager.HistoryItem), PropertyChangedEventHandler>();

        private static void Subscribe(ListBox listBox, UndoRedoManager.HistoryItem item)
        {
            try
            {
                var key = (listBox, item);

                if (_handlers.TryGetValue(key, out var existing))
                {
                    item.PropertyChanged -= existing;
                }

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
