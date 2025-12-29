using System.Text.Json;

namespace MainApplication.Infrastructure
{
    public class JsonSerializerService : IJsonSerializerService
    {
        /* ---------------------------------------------------------
         * JSONシリアライズ設定
         * --------------------------------------------------------- */

        private readonly JsonSerializerOptions _options;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        public JsonSerializerService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
        }

        /* ---------------------------------------------------------
         * シリアライズ(オブジェクト → JSON)
         * --------------------------------------------------------- */

        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        /* ---------------------------------------------------------
         * デシリアライズ(JSON → オブジェクト)
         * --------------------------------------------------------- */

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}

/* --- End of file --- */
