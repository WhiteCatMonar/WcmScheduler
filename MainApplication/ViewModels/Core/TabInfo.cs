namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// タブの表示名(Header)と、そのタブが表示する内容(Content)を保持するクラス。
    /// タブ管理部分を有するViewModelに使用される。
    /// </summary>
    public class TabInfo
    {
        /* ---------------------------------------------------------
         * タブの表示名
         * --------------------------------------------------------- */

        /// <summary>
        /// タブヘッダーに表示される文字列。
        /// 例：「タスク編集」「プロジェクト設定」「進捗」など。
        /// </summary>
        public string Header { get; }

        /* ---------------------------------------------------------
         * タブの内容(表示するViewModel)
         * --------------------------------------------------------- */

        /// <summary>
        /// ContentPresenterに渡され、対応するDataTemplateによって
        /// 適切なUserControlが描画される。
        /// </summary>
        public object Content { get; }

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// HeaderとContentを受け取り、タブ情報を初期化する。
        /// </summary>
        public TabInfo(string header, object content)
        {
            Header = header;
            Content = content;
        }
    }
}

/* --- End of file --- */
