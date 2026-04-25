using MainApplication.ViewModels.Core;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            /* デフォルトテーマを展開 */
            ThemeManager.ExtractDefaultThemes();

            /* テーマ読み込み */
            ThemeManager.LoadThemes();
        }
    }
}

/* --- End of file --- */
