using System.Windows;
using System.Windows.Controls;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.Views.ProjectEditor.Sidebar
{
    /// <summary>
    /// TaskDetailControlの表示テンプレートを切り替えるためのDataTemplateSelector。
    /// 
    /// ・TaskNodeViewModelが選択されている場合 → ノード詳細テンプレート
    /// ・未選択の場合 → プレースホルダーテンプレート
    /// 
    /// XAML側で柔軟にテンプレートを差し替えるために使用する。
    /// </summary>
    public class TaskDetailTemplateSelector : DataTemplateSelector
    {
        /* ---------------------------------------------------------
         * プロパティ(XAMLから設定される)
         * --------------------------------------------------------- */

        /// <summary>
        /// ノード未選択時に表示するテンプレート。
        /// 例：説明文や「ノードを選択してください」などのUI。
        /// </summary>
        public required DataTemplate PlaceholderTemplate { get; set; }

        /// <summary>
        /// TaskNodeViewModelが選択されている場合に使用するテンプレート。
        /// </summary>
        public required DataTemplate TaskDetailTemplate { get; set; }

        /* ---------------------------------------------------------
         * テンプレート選択ロジック
         * --------------------------------------------------------- */

        /// <summary>
        /// DataContextの型に応じてテンプレートを切り替える。
        /// TaskNodeViewModel → TaskDetailTemplate
        /// nullまたはその他 → PlaceholderTemplate
        /// 
        /// テンプレートが未設定の場合はResourceからフォールバック検索する。
        /// </summary>
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (item is TaskNodeViewModel)
            {
                /* NodeDetailTemplateが設定されていればそれを使い、無ければResourceからTaskNodeViewModel用テンプレートを探す */
                return TaskDetailTemplate ?? element?.FindResource(typeof(TaskNodeViewModel)) as DataTemplate;
            }

            /* プレースホルダー用テンプレート */
            return PlaceholderTemplate ?? element?.FindResource("PlaceholderTemplate") as DataTemplate;
        }
    }
}

/* --- End of file --- */
