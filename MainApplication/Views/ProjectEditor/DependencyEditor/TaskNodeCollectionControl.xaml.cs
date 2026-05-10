using System.Windows.Controls;

namespace MainApplication.Views.ProjectEditor.DependencyEditor
{
    /// <summary>
    /// 作成された複数のノードの表示管理をするためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class TaskNodeCollectionControl : UserControl
    {
        public TaskNodeCollectionControl()
        {
            InitializeComponent();
        }
    }
}
