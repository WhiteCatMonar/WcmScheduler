using System.IO;

namespace MainApplication.Infrastructure
{
    public class FileService : IFileService
    {
        /* ---------------------------------------------------------
         * 公開メソッド(ファイル保存)
         * --------------------------------------------------------- */

        public void SaveText(string path, string content)
        {
            /* NOTE: 例外はViewModel側でキャッチしてUIに通知する */
            File.WriteAllText(path, content);
        }

        /* ---------------------------------------------------------
         * 公開メソッド(ファイル読み込み)
         * --------------------------------------------------------- */

        public string LoadText(string path)
        {
            /* NOTE: 例外はViewModel側に投げる */
            return File.ReadAllText(path);
        }
    }
}

/* --- End of file --- */
