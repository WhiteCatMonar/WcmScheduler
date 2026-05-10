using System.IO;
using System.Text;

namespace MainApplication.Infrastructure
{
    public class FileService : IFileService
    {
        /* ---------------------------------------------------------
         * 公開メソッド(ファイル保存)
         * --------------------------------------------------------- */

        /// <summary>
        /// テキストファイルをUTF-8で保存する
        /// </summary>
        /// <param name="path">保存先パス</param>
        /// <param name="content">保存内容</param>
        public void SaveText(string path, string content)
        {
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        /* ---------------------------------------------------------
         * 公開メソッド(ファイル読み込み)
         * --------------------------------------------------------- */

        /// <summary>
        /// テキストファイルをUTF-8で読み込む
        /// </summary>
        /// <param name="path">読み込み元パス</param>
        /// <returns>読み込んだ文字列</returns>
        public string LoadText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
    }
}

/* --- End of file --- */
