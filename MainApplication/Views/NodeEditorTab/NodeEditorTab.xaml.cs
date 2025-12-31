using MainApplication.ViewModels.ProjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MainApplication.Views.NodeEditorTab
{
    /// <summary>
    /// ノードエディタタブのUIロジック。
    /// キーボードショートカット(Undo/Redo/Delete)を処理し、
    /// ViewModelのコマンドへ橋渡しする役割を持つ。
    /// </summary>
    public partial class NodeEditorTab : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// NodeEditorTabを初期化する。
        /// </summary>
        public NodeEditorTab()
        {
            InitializeComponent();
        }

        /* ---------------------------------------------------------
         * キーボードショートカット処理
         * --------------------------------------------------------- */

        /// <summary>
        /// Ctrl+Z / Ctrl+Y / Deleteなどのショートカットを処理する。
        /// TextBoxにフォーカスがある場合は通常の文字入力を優先し、
        /// それ以外の場合のみショートカットを有効にする。
        /// </summary>
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            /* テキスト入力中はショートカットを無効化 */
            if (Keyboard.FocusedElement is TextBoxBase)
            {
                return;
            }

            if (DataContext is NodeEditorViewModel nevm)
            {
                /* Undo(Ctrl+Z) */
                if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (nevm.UndoCommand.CanExecute(null))
                    {
                        nevm.UndoCommand.Execute(null);
                    }
                    e.Handled = true;
                }
                /* Redo(Ctrl+Y) */
                else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (nevm.RedoCommand.CanExecute(null))
                    {
                        nevm.RedoCommand.Execute(null);
                    }
                    e.Handled = true;
                }
                /* 接続線削除(Delete) */
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

/* --- End of file --- */
