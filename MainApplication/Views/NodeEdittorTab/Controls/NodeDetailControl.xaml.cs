using MainApplication.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MainApplication.Views.NodeEditorTab
{
    /// <summary>
    /// NodeDetailView.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeDetailControl : UserControl
    {
        private DispatcherTimer _historyTimer;
        public NodeDetailControl()
        {
            InitializeComponent();
            _historyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _historyTimer.Tick += HistoryTimer_Tick;
            this.DataContextChanged += NodeDetailControl_DataContextChanged;
        }

        private void NodeDetailControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as NodeViewModel)?.CommitEdits();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _historyTimer.Stop();
            _historyTimer.Start();
        }

        private void HistoryTimer_Tick(object sender, EventArgs e)
        {
            _historyTimer.Stop();

            if (DataContext is NodeViewModel node)
            {
                node.CommitEdits();
            }
        }
    }
}
