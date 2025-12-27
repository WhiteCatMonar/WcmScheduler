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
        public NodeDetailControl()
        {
            InitializeComponent();
            this.DataContextChanged += NodeDetailControl_DataContextChanged;
        }

        private void NodeDetailControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as NodeViewModel)?.CommitEdits();
        }
    }
}
