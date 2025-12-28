namespace MainApplication.Infrastructure
{
    public interface IFileService
    {
        void SaveText(string path, string content);
        string LoadText(string path);
    }
}
