using System.IO;

namespace MainApplication.Infrastructure
{
    public class FileService : IFileService
    {
        public void SaveText(string path, string content)
        {
            /* NOTE: 例外はViewModel側でキャッチしてUIに通知する */
            File.WriteAllText(path, content);
        }

        public string LoadText(string path)
        {
            /* NOTE: 例外はViewModel側に投げる */
            return File.ReadAllText(path);
        }
    }
}
