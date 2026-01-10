using System.Windows.Controls;

namespace MainApplication.Views.NodeEditorTab.Controls
{
    /// <summary>
    /// グリッド線を表示・操作するためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class GridControl : UserControl
    {
        public GridControl()
        {
            InitializeComponent();
        }
    }
}
