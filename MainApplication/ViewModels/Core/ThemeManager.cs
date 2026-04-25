using MainApplication.Models.Settings;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels.Core
{
    public static class ThemeManager
    {
        public static List<ThemeSettingModel> LoadedThemes { get; } = new();
        public static ThemeSettingModel CurrentTheme { get; private set; } = new ThemeSettingModel();

        public static void LoadThemes()
        {
            LoadedThemes.Clear();

            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            if (!Directory.Exists(baseDir))
                return;

            /* NOTE: schemaフォルダは除外 */
            var files = Directory.GetFiles(baseDir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var theme = LoadThemeFromJson(file);
            }

            /* システムテーマに合わせて初期テーマを決定 */
            var defaultThemeName = IsSystemLightTheme() ? "Light" : "Dark";
            var initialTheme = LoadedThemes.FirstOrDefault(t => t.Name == defaultThemeName)
                               ?? LoadedThemes.FirstOrDefault();

            if (initialTheme != null)
            {
                ApplyTheme(initialTheme);
            }
        }

        private static ThemeSettingModel? LoadThemeFromJson(string path)
        {
            ThemeSettingModel? model = null;
            try
            {
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                model = JsonSerializer.Deserialize<ThemeSettingModel>(json, options);

                if (model != null && !LoadedThemes.Any(t => t.Name == model.Name))
                {
                    LoadedThemes.Add(model);
                }
            }
            catch
            {
                /* TODO: ログ出力 */
            }
            return model;
        }

        public static void SaveTheme(ThemeSettingModel theme)
        {
            try
            {
                var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
                Directory.CreateDirectory(baseDir);

                var path = Path.Combine(baseDir, $"{theme.Name}.json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(theme, options);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch
            {
                /* TODO: ログ出力 */
            }
        }

        public static bool IsSystemLightTheme()
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);

            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                return value == 1;
            }

            /* 取得できない場合はLightとみなす */
            return true;
        }

        private static Color GetThemeColor(
            string key,
            Dictionary<string, string> targetColors,
            Dictionary<string, string> fallbackColors)
        {

            if (targetColors.TryGetValue(key, out var targetColor))
            {
                if (Regex.IsMatch(targetColor, @"^#[0-9A-Fa-f]{8}$")) {
                    if (ColorConverter.ConvertFromString(targetColor) is Color targetSettingColor)
                    {
                        return targetSettingColor;
                    }
                }
            }
            
            if (fallbackColors.TryGetValue(key, out var fallbackColor))
            {
                if (Regex.IsMatch(fallbackColor, @"^#[0-9A-Fa-f]{8}$"))
                {
                    if (ColorConverter.ConvertFromString(fallbackColor) is Color defaultSettingColor)
                    {
                        return defaultSettingColor;
                    }
                }
            }

            /* デフォルトテーマが取得できないのは異常ケース */
            /* TODO: ログ出力 */
            return Colors.Transparent;
        }

        public static void ApplyTheme(ThemeSettingModel theme)
        {
            var dict = new ResourceDictionary();

            /* デフォルトテーマを取得(フォールバック用) */
            var defaultThemeName = IsSystemLightTheme() ? "Light" : "Dark";
            var defaultTheme = LoadedThemes.FirstOrDefault(t => t.Name == defaultThemeName);
            if (defaultTheme is null)
            {
                /* TODO: ログ出力 */
                defaultTheme = new ThemeSettingModel();
            }

            dict["NodeBackgroundBrush"] = new SolidColorBrush(GetThemeColor("node-background", theme.Colors, defaultTheme.Colors));

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            CurrentTheme = theme;
        }

        public static void ExtractDefaultThemes()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            Directory.CreateDirectory(baseDir);

            ExtractResource("MainApplication.Themes.Light.json", Path.Combine(baseDir, "Light.json"));
            ExtractResource("MainApplication.Themes.Dark.json", Path.Combine(baseDir, "Dark.json"));

            string schemaDir = Path.Combine(baseDir, "schema");
            Directory.CreateDirectory(schemaDir);

            ExtractResource("MainApplication.Themes.schema.ThemeSchema.json",
                Path.Combine(schemaDir, "ThemeSchema.json"));
        }

        private static void ExtractResource(string resourceName, string outputPath)
        {
            if (File.Exists(outputPath))
                return; // 上書きしない（ユーザーの編集を守る）

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
                return;

            using var file = File.Create(outputPath);
            stream.CopyTo(file);
        }
    }
}

/* --- End of file --- */
