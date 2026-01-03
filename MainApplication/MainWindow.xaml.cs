using MainApplication.ViewModels;
using System.Windows;

namespace MainApplication
{
    /// <summary>
    /// アプリケーションのメインウィンドウ。
    /// SchedulerViewModelを生成し、DataContextとして設定する。
    /// また、保存・読み込みダイアログの表示など、
    /// ViewModelからの依頼(RequestLoad / RequestSaveAs)を受け取って処理する。
    /// </summary>
    public partial class MainWindow : Window
    {
        /* ---------------------------------------------------------
         * プロパティ
         * --------------------------------------------------------- */

        /// <summary>
        /// アプリ全体を統括するSchedulerViewModel。
        /// </summary>
        public SchedulerViewModel SchedulerVM { get; }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// MainWindowを初期化し、SchedulerViewModelを生成してDataContextに設定する。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            /* 子ViewModel名の辞書を渡してSchedulerViewModelを生成 */
            SchedulerVM = new SchedulerViewModel();
            DataContext = SchedulerVM;

            Loaded += MainWindow_Loaded;
        }

        /* ---------------------------------------------------------
         * Loaded(イベント購読の登録)
         * --------------------------------------------------------- */

        /// <summary>
        /// ウィンドウがロードされたタイミングで、
        /// ViewModel → View への依頼イベントを購読する。
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SchedulerVM.RequestLoad += OnRequestLoad;
            SchedulerVM.RequestSaveAs += OnRequestSaveAs;
        }

        /* ---------------------------------------------------------
         * ファイル読み込み処理(ViewModelからの依頼)
         * --------------------------------------------------------- */

        /// <summary>
        /// ViewModelから「ファイルを開いてほしい」と依頼されたときに呼ばれる。
        /// OpenFileDialogを表示し、選択されたファイルを読み込む。
        /// </summary>
        private void OnRequestLoad()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON ファイル (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                SchedulerVM.LoadFromFile(dialog.FileName);
            }
        }

        /* ---------------------------------------------------------
         * 名前を付けて保存(ViewModelからの依頼)
         * --------------------------------------------------------- */

        /// <summary>
        /// ViewModelから「名前を付けて保存してほしい」と依頼されたときに呼ばれる。
        /// SaveFileDialogを表示し、選択されたパスに保存する。
        /// </summary>
        private void OnRequestSaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON ファイル (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                SchedulerVM.SaveAs(dialog.FileName);
            }
        }
    }
}

/* --- End of file --- */
