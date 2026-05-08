using System.Windows.Controls;

namespace MainApplication.Views.ProjectEditor.Sidebar
{
    /// <summary>
    /// Undo/Redo履歴を表示するためのUserControl。
    /// このクラスはXAML側のUIと結びつくコードビハインドであり、
    /// 特別なロジックは持たずInitializeComponentの呼び出しのみを担当する。
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// HistoryControlを初期化し、対応するXAMLを読み込む。
        /// </summary>
        public HistoryControl()
        {
            InitializeComponent();
        }
    }
}

/* --- End of file --- */
