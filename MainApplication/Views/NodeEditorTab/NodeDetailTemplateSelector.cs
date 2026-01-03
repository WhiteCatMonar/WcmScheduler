using System.Windows;
using System.Windows.Controls;
using MainApplication.ViewModels.ProjectModel;

namespace MainApplication.Views.NodeEditorTab
{
    /// <summary>
    /// NodeDetailControlの表示テンプレートを切り替えるためのDataTemplateSelector。
    /// 
    /// ・NodeViewModelが選択されている場合 → ノード詳細テンプレート
    /// ・未選択の場合 → プレースホルダーテンプレート
    /// 
    /// XAML側で柔軟にテンプレートを差し替えるために使用する。
    /// </summary>
    public class NodeDetailTemplateSelector : DataTemplateSelector
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
        /// NodeViewModelが選択されている場合に使用するテンプレート。
        /// </summary>
        public required DataTemplate NodeDetailTemplate { get; set; }

        /* ---------------------------------------------------------
         * テンプレート選択ロジック
         * --------------------------------------------------------- */

        /// <summary>
        /// DataContextの型に応じてテンプレートを切り替える。
        /// NodeViewModel → NodeDetailTemplate
        /// nullまたはその他 → PlaceholderTemplate
        /// 
        /// テンプレートが未設定の場合はResourceからフォールバック検索する。
        /// </summary>
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (item is NodeViewModel)
            {
                /* NodeDetailTemplateが設定されていればそれを使い、無ければResourceからNodeViewModel用テンプレートを探す */
                return NodeDetailTemplate ?? element?.FindResource(typeof(NodeViewModel)) as DataTemplate;
            }

            /* プレースホルダー用テンプレート */
            return PlaceholderTemplate ?? element?.FindResource("PlaceholderTemplate") as DataTemplate;
        }
    }
}

/* --- End of file --- */
