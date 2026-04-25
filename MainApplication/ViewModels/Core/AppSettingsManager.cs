using MainApplication.Models.Settings;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// アプリケーション全体のユーザー設定を読み書きするクラス。
    /// </summary>
    public static class AppSettingsManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// 現在読み込まれているユーザー設定。
        /// </summary>
        public static AppSettingsModel Current { get; private set; } = new();

        /// <summary>
        /// ユーザー設定ファイルを読み込む。
        /// </summary>
        public static void Load()
        {
            try
            {
                var path = GetSettingsPath();
                if (!File.Exists(path))
                {
                    Current = new AppSettingsModel();
                    return;
                }

                var json = File.ReadAllText(path, Encoding.UTF8);
                Current = JsonSerializer.Deserialize<AppSettingsModel>(json, JsonOptions)
                          ?? new AppSettingsModel();
            }
            catch
            {
                Current = new AppSettingsModel();
            }
        }

        /// <summary>
        /// ユーザー設定ファイルを保存する。
        /// </summary>
        public static void Save()
        {
            try
            {
                var path = GetSettingsPath();
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(Current, JsonOptions);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 最後に適用したテーマ名を保存する。
        /// </summary>
        /// <param name="themeName">テーマ名。</param>
        public static void SaveLastThemeName(string themeName)
        {
            Current.LastThemeName = themeName;
            Save();
        }

        /// <summary>
        /// ユーザー設定ファイルのパスを取得する。
        /// </summary>
        /// <returns>ユーザー設定ファイルのパス。</returns>
        private static string GetSettingsPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "app-settings.json");
        }
    }
}

/* --- End of file --- */
