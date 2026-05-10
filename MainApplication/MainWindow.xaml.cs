using MainApplication.ViewModels;
using System.ComponentModel;
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

            SchedulerVM = new SchedulerViewModel();
            DataContext = SchedulerVM;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        /* ---------------------------------------------------------
         * Loaded(イベント購読の登録)
         * --------------------------------------------------------- */

        /// <summary>
        /// ウィンドウがロードされたタイミングで、
        /// ViewModel → View への依頼イベントを購読する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">イベント情報。</param>
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
                var restoreEditFile = false;
                if (SchedulerVM.HasEditFile(dialog.FileName))
                {
                    var editFilePath = SchedulerVM.GetEditFilePathFor(dialog.FileName);
                    var fileTime = System.IO.File.GetLastWriteTime(dialog.FileName);
                    var editTime = System.IO.File.GetLastWriteTime(editFilePath);
                    var result = MessageBox.Show(
                        this,
                        $"編集作業ファイルが残っています。復元しますか？\n\n正式ファイル: {fileTime:yyyy/MM/dd HH:mm:ss}\n編集作業ファイル: {editTime:yyyy/MM/dd HH:mm:ss}",
                        "編集作業ファイルの確認",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    restoreEditFile = result == MessageBoxResult.Yes;
                }

                if (!SchedulerVM.LoadFromFile(dialog.FileName, restoreEditFile))
                {
                    MessageBox.Show(
                        this,
                        "ファイルの読み込みに失敗しました。",
                        "読み込みエラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        /* ---------------------------------------------------------
         * 名前を付けて保存(ViewModelからの依頼)
         * --------------------------------------------------------- */

        /// <summary>
        /// ViewModelから「名前を付けて保存してほしい」と依頼されたときに呼ばれる。
        /// </summary>
        private void OnRequestSaveAs()
        {
            ShowSaveAsDialog();
        }

        /// <summary>
        /// SaveFileDialogを表示し、選択されたパスに保存する。
        /// </summary>
        /// <returns>保存に成功した場合はtrue。</returns>
        private bool ShowSaveAsDialog()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON ファイル (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                return SchedulerVM.SaveAs(dialog.FileName);
            }

            return false;
        }

        /// <summary>
        /// ウィンドウ終了時に未保存データがある場合は確認する。
        /// </summary>
        /// <param name="sender">イベント発行元。</param>
        /// <param name="e">終了イベント情報。</param>
        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            SchedulerVM.RefreshDirtyState();
            if (!SchedulerVM.IsDirty)
            {
                SchedulerVM.DeleteEditFileIfClean();
                return;
            }

            var result = MessageBox.Show(
                this,
                "未保存の変更があります。保存して終了しますか？",
                "未保存データの確認",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.No)
            {
                SchedulerVM.DiscardEditFile();
                return;
            }

            var saved = !string.IsNullOrEmpty(SchedulerVM.CurrentFilePath)
                ? SchedulerVM.Save()
                : ShowSaveAsDialog();

            if (saved)
            {
                SchedulerVM.DeleteEditFileIfClean();
                return;
            }

            MessageBox.Show(
                this,
                "保存に失敗したため、終了を中止しました。",
                "保存エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            e.Cancel = true;
        }
    }
}

/* --- End of file --- */
