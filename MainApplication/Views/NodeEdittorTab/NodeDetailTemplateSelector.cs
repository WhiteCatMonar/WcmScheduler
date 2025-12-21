using System.Windows;
using System.Windows.Controls;
using MainApplication.ViewModels;

namespace MainApplication.Views.NodeEditorTab
{
    public class NodeDetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlaceholderTemplate { get; set; }
        public DataTemplate NodeDetailTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (item is NodeViewModel)
            {
                return NodeDetailTemplate ?? element.FindResource(typeof(NodeViewModel)) as DataTemplate;
            }
            return PlaceholderTemplate ?? element.FindResource("PlaceholderTemplate") as DataTemplate;
        }
    }
}
