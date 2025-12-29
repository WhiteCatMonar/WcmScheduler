using System.Windows;

namespace MainApplication
{
    /// <summary>
    /// アプリケーション全体のエントリポイントとなるApplicationクラス。
    /// 
    /// 現状では特別な初期化処理は行っていないが、
    /// 以下のようなアプリケーションレベルのイベントを追加する際に使用する：
    /// ・Startup(起動時の初期化)
    /// ・Exit(終了処理)
    /// ・DispatcherUnhandledException(未処理例外の捕捉)
    /// ・リソースの初期化やDIコンテナの構築
    /// </summary>
    public partial class App : Application
    {
        /* ---------------------------------------------------------
         * 必要に応じてアプリケーションイベントを追加可能
         * ---------------------------------------------------------
         * 例：アプリ起動時の初期化処理
         * protected override void OnStartup(StartupEventArgs e)
         * {
         *     base.OnStartup(e);
         * }
         * 
         * 例：アプリ終了処理
         * protected override void OnExit(ExitEventArgs e)
         * {
         *     base.OnExit(e);
         * }
         * --------------------------------------------------------- */
    }
}

/* --- End of file --- */
