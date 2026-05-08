using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MainApplication.Views.ProjectEditor.Sidebar
{
    /// <summary>
    /// ノードの詳細情報(タスク名・担当者・日時など)を表示するUserControl。
    /// このコードビハインドは、DataContextの切り替え時に
    /// 旧ノードの未確定編集を確定するための最小限のロジックのみを持つ。
    /// </summary>
    public partial class TaskDetailControl : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// TaskDetailControlを初期化し、
        /// DataContext変更時の処理を登録する。
        /// </summary>
        public TaskDetailControl()
        {
            InitializeComponent();
            DataContextChanged += TaskDetailControl_DataContextChanged;
        }

        /* ---------------------------------------------------------
         * DataContext変更時の処理
         * --------------------------------------------------------- */

        /// <summary>
        /// DataContextが別のTaskNodeViewModelに切り替わった際、
        /// 旧ノードの未確定編集(遅延コミット対象)を確定する。
        /// 
        /// これにより、ユーザーが別のノードを選択したときに
        /// 編集内容が失われることを防ぐ。
        /// </summary>
        private void TaskDetailControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (e.OldValue as TaskNodeViewModel)?.Detail.CommitEdits();
        }
    }
}

/* --- End of file --- */
