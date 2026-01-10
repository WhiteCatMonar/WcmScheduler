using System.Windows.Controls;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// 作成された複数のノードの表示管理をするためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class NodeCollectionControl : UserControl
    {
        public NodeCollectionControl()
        {
            InitializeComponent();
        }
    }
}
