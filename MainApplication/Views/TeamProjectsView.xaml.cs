using System.Windows.Controls;

namespace MainApplication.Views
{
    /// <summary>
    /// 複数のチーム内プロジェクト情報を表示するためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class TeamProjectsView : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// TeamProjectsViewを初期化し、対応するXAMLを読み込む。
        /// </summary>
        public TeamProjectsView()
        {
            InitializeComponent();
        }
    }
}
