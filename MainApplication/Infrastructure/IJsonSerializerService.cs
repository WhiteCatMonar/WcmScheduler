namespace MainApplication.Infrastructure
{
    public interface IJsonSerializerService
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}
