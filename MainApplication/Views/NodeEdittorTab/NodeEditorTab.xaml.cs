using MainApplication.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MainApplication.Views.NodeEditorTab
{
    /// <summary>
    /// NodeEditorTab.xaml の相互作用ロジック
    /// </summary>
    public partial class NodeEditorTab : UserControl
    {
        public NodeEditorTab()
        {
            InitializeComponent();

            /* ViewModelを生成してDataContextに設定 */
            this.DataContext = new NodeEditorViewModel();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(Keyboard.FocusedElement is TextBoxBase))
            {
                if (DataContext is NodeEditorViewModel nevm)
                {
                    if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        if (nevm.UndoCommand.CanExecute(null))
                        {
                            nevm.UndoCommand.Execute(null);
                        }
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        if (nevm.RedoCommand.CanExecute(null))
                        {
                            nevm.RedoCommand.Execute(null);
                        }
                        e.Handled = true;
                    }
                    if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
                    {
                        if (nevm.Connections.DeleteSelectedConnectionCommand.CanExecute(null))
                        {
                            nevm.Connections.DeleteSelectedConnectionCommand.Execute(null);
                        }
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
