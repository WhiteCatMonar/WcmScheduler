using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// JSONシリアライズ設定を初期化する。
        /// </summary>
        public JsonSerializerService()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };
        }

        /* ---------------------------------------------------------
         * シリアライズ(オブジェクト → JSON)
         * --------------------------------------------------------- */

        /// <summary>
        /// 指定オブジェクトをJSON文字列へ変換する。
        /// </summary>
        /// <typeparam name="T">変換対象の型。</typeparam>
        /// <param name="obj">変換対象オブジェクト。</param>
        /// <returns>JSON文字列。</returns>
        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        /* ---------------------------------------------------------
         * デシリアライズ(JSON → オブジェクト)
         * --------------------------------------------------------- */

        /// <summary>
        /// JSON文字列を指定型のオブジェクトへ変換する。
        /// </summary>
        /// <typeparam name="T">変換先の型。</typeparam>
        /// <param name="json">JSON文字列。</param>
        /// <returns>変換後オブジェクト。</returns>
        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }
}

/* --- End of file --- */
