using MainApplication.ViewModels.DependencyEditorModel;
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
                if (project.DependencyEditor.UndoCommand.CanExecute(null))
                {
                    project.DependencyEditor.UndoCommand.Execute(null);
                    e.Handled = true;
                }

                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
            {
                if (project.DependencyEditor.RedoCommand.CanExecute(null))
                {
                    project.DependencyEditor.RedoCommand.Execute(null);
                    e.Handled = true;
                }

                return;
            }

            if (e.Key == Key.Delete && project.DependencyEditor.Connections.DeleteSelectedConnectionCommand.CanExecute(null))
            {
                project.DependencyEditor.Connections.DeleteSelectedConnectionCommand.Execute(null);
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
                project.GanttChart.RequestRefresh();
            }
        }
    }
}

/* --- End of file --- */
