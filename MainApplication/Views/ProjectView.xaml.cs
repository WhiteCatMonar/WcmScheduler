using MainApplication.ViewModels.ProjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MainApplication.Views
{
    /// <summary>
    /// プロジェクト単体情報を表示するためのUserControl
    /// </summary>
    public partial class ProjectView : UserControl
    {
        /// <summary>
        /// ProjectViewを初期化し、対応するXAMLを読み込む
        /// </summary>
        public ProjectView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// キーボードショートカットによる編集操作を処理する
        /// </summary>
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not ProjectViewModel project)
            {
                return;
            }

            if (Keyboard.FocusedElement is TextBoxBase)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
            {
                if (project.NodeEditor.UndoCommand.CanExecute(null))
                {
                    project.NodeEditor.UndoCommand.Execute(null);
                    e.Handled = true;
                }

                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                if (project.NodeEditor.RedoCommand.CanExecute(null))
                {
                    project.NodeEditor.RedoCommand.Execute(null);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Delete && project.NodeEditor.Connections.DeleteSelectedConnectionCommand.CanExecute(null))
            {
                project.NodeEditor.Connections.DeleteSelectedConnectionCommand.Execute(null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// エディタ領域のタブ切り替えに応じて表示内容を更新する
        /// </summary>
        private void EditorTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != sender)
            {
                return;
            }

            if (sender is not TabControl tabControl || tabControl.SelectedIndex != 1)
            {
                return;
            }

            if (DataContext is ProjectViewModel project)
            {
                project.GanttChart.Refresh();
            }
        }
    }
}

/* --- End of file --- */
