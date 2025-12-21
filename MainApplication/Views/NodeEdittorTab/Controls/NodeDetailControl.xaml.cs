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
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is NodeViewModel node)
            {
                // 履歴に追加する処理
                if (node.OldTaskName != node.TaskName)
                {
                    node.CommitHistory("TaskName", node.OldTaskName, node.TaskName);
                    node.OldTaskName = node.TaskName;
                }
                if (node.OldPerson != node.Person)
                {
                    node.CommitHistory("Person", node.OldPerson, node.Person);
                    node.OldPerson = node.Person;
                }
                if (node.OldComment != node.Comment)
                {
                    node.CommitHistory("Comment", node.OldComment, node.Comment);
                    node.OldComment = node.Comment;
                }
            }
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
                // 履歴に追加する処理
                if (node.OldTaskName != node.TaskName)
                {
                    node.CommitHistory("TaskName", node.OldTaskName, node.TaskName);
                    node.OldTaskName = node.TaskName;
                }
                if (node.OldPerson != node.Person)
                {
                    node.CommitHistory("Person", node.Person, node.Person);
                    node.OldPerson = node.Person;
                }
                if (node.OldComment != node.Comment)
                {
                    node.CommitHistory("Comment", node.Comment, node.Comment);
                    node.OldComment = node.Comment;
                }
            }
        }

    }
}
