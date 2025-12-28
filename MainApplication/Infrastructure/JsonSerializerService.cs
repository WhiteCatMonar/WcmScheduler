using System.Text.Json;

namespace MainApplication.Infrastructure
{
    public class JsonSerializerService : IJsonSerializerService
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializerService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}
