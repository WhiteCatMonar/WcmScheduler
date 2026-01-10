using System.Windows.Controls;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// 作成された複数の接続線の表示管理をするためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class ConnectionCollectionControl : UserControl
    {
        public ConnectionCollectionControl()
        {
            InitializeComponent();
        }
    }
}
