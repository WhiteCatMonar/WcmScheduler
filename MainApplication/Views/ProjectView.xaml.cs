using System.Windows.Controls;

namespace MainApplication.Views
{
    /// <summary>
    /// プロジェクト単体情報を表示するためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class ProjectView : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ProjectViewを初期化し、対応するXAMLを読み込む。
        /// </summary>
        public ProjectView()
        {
            InitializeComponent();
        }
    }
}
