using MainApplication.Models.Settings;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels.Core
{
    /// <summary>
    /// テーマファイルの読み込み、保存、適用を管理するクラス。
    /// </summary>
    public static class ThemeManager
    {
        private static readonly Regex ThemeColorRegex = new("^#[0-9A-Fa-f]{8}$", RegexOptions.Compiled);
        private static readonly Dictionary<string, string> BuiltInFallbackColors = new ThemeSettingModel().Colors;

        private static readonly Dictionary<string, string> ThemeBrushResourceKeys = new()
        {
            { "window-background", "WindowBackgroundBrush" },
            { "window-text", "WindowTextBrush" },
            { "tab-window-background", "TabWindowBackgroundBrush" },
            { "tab-item-background", "TabItemBackgroundBrush" },
            { "tab-item-selected-background", "TabItemSelectedBackgroundBrush" },
            { "tab-item-text", "TabItemTextBrush" },
            { "tab-item-selected-text", "TabItemSelectedTextBrush" },
            { "node-editor-border", "NodeEditorBorderBrush" },
            { "node-editor-canvas-background", "NodeEditorCanvasBackgroundBrush" },
            { "temporary-connection-stroke", "TemporaryConnectionStrokeBrush" },
            { "grid-line-minor", "GridLineMinorBrush" },
            { "grid-line-major", "GridLineMajorBrush" },
            { "grid-origin-axis-x", "GridOriginAxisXBrush" },
            { "grid-origin-axis-y", "GridOriginAxisYBrush" },
            { "node-background", "NodeBackgroundBrush" },
            { "node-selected-background", "NodeSelectedBackgroundBrush" },
            { "node-border", "NodeBorderBrush" },
            { "node-status-ready", "NodeStatusReadyBrush" },
            { "node-status-pending", "NodeStatusPendingBrush" },
            { "node-status-in-progress", "NodeStatusInProgressBrush" },
            { "node-status-done", "NodeStatusDoneBrush" },
            { "node-text", "NodeTextBrush" },
            { "port-input-fill", "PortInputFillBrush" },
            { "port-output-fill", "PortOutputFillBrush" },
            { "port-unknown-fill", "PortUnknownFillBrush" },
            { "connection-stroke", "ConnectionStrokeBrush" },
            { "connection-selected-stroke", "ConnectionSelectedStrokeBrush" },
            { "side-panel-background", "SidePanelBackgroundBrush" },
            { "splitter-background", "SplitterBackgroundBrush" },
            { "placeholder-text", "PlaceholderTextBrush" },
            { "task-editor-heading-text", "TaskEditorHeadingTextBrush" },
            { "history-panel-background", "HistoryPanelBackgroundBrush" },
            { "history-list-background", "HistoryListBackgroundBrush" },
            { "history-text", "HistoryTextBrush" },
            { "history-timestamp-text", "HistoryTimestampTextBrush" },
            { "history-current-text", "HistoryCurrentTextBrush" },
            { "history-selected-background", "HistorySelectedBackgroundBrush" },
            { "date-time-editor-background", "DateTimeEditorBackgroundBrush" },
            { "date-time-editor-text", "DateTimeEditorTextBrush" },
            { "theme-setting-background", "ThemeSettingBackgroundBrush" },
            { "theme-setting-text", "ThemeSettingTextBrush" },
            { "color-preview-border", "ColorPreviewBorderBrush" },
            { "gantt-chart-background", "GanttChartBackgroundBrush" },
            { "gantt-chart-border", "GanttChartBorderBrush" },
            { "gantt-header-background", "GanttHeaderBackgroundBrush" },
            { "gantt-header-text", "GanttHeaderTextBrush" },
            { "gantt-row-header-background", "GanttRowHeaderBackgroundBrush" },
            { "gantt-grid-line", "GanttGridLineBrush" },
            { "gantt-warning-text", "GanttWarningTextBrush" },
            { "gantt-suspension-background", "GanttSuspensionBackgroundBrush" },
            { "gantt-dependency-line", "GanttDependencyLineBrush" },
            { "gantt-saturday-background", "GanttSaturdayBackgroundBrush" },
            { "gantt-saturday-text", "GanttSaturdayTextBrush" },
            { "gantt-sunday-background", "GanttSundayBackgroundBrush" },
            { "gantt-sunday-text", "GanttSundayTextBrush" },
            { "gantt-special-holiday-background", "GanttSpecialHolidayBackgroundBrush" },
            { "gantt-special-holiday-text", "GanttSpecialHolidayTextBrush" }
        };

        /// <summary>
        /// 読み込み済みテーマ一覧。
        /// </summary>
        public static List<ThemeSettingModel> LoadedThemes { get; } = new();

        /// <summary>
        /// 現在適用中のテーマ。
        /// </summary>
        public static ThemeSettingModel CurrentTheme { get; private set; } = new ThemeSettingModel();

        /// <summary>
        /// 実行ディレクトリのテーマファイルを読み込む。
        /// </summary>
        public static void LoadThemes()
        {
            LoadedThemes.Clear();

            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            if (!Directory.Exists(baseDir))
            {
                return;
            }

            var files = Directory.GetFiles(baseDir, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                LoadThemeFromJson(file);
            }

            AppSettingsManager.Load();

            var defaultThemeName = IsSystemLightTheme() ? "Light" : "Dark";
            var initialTheme = LoadedThemes.FirstOrDefault(t => t.Name == AppSettingsManager.Current.LastThemeName)
                               ?? LoadedThemes.FirstOrDefault(t => t.Name == defaultThemeName)
                               ?? LoadedThemes.FirstOrDefault();

            if (initialTheme != null)
            {
                ApplyTheme(initialTheme, false);
            }
        }

        /// <summary>
        /// 指定パスのJSONファイルからテーマを読み込む。
        /// </summary>
        /// <param name="path">テーマJSONファイルのパス。</param>
        /// <returns>読み込まれたテーマ。読み込みに失敗した場合はnull。</returns>
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
            }
            return model;
        }

        /// <summary>
        /// テーマを実行ディレクトリのThemesフォルダへ保存する。
        /// </summary>
        /// <param name="theme">保存対象のテーマ。</param>
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
            }
        }

        /// <summary>
        /// Windowsのアプリテーマ設定がライトテーマかどうかを取得する。
        /// </summary>
        /// <returns>ライトテーマの場合はtrue。</returns>
        public static bool IsSystemLightTheme()
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);

            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                return value == 1;
            }

            return true;
        }

        /// <summary>
        /// テーマ色を取得する。
        /// </summary>
        /// <param name="key">テーマ色キー。</param>
        /// <param name="targetColors">適用対象テーマの色辞書。</param>
        /// <param name="fallbackColors">フォールバックテーマの色辞書。</param>
        /// <returns>解決された色。</returns>
        private static Color GetThemeColor(
            string key,
            Dictionary<string, string> targetColors,
            Dictionary<string, string> fallbackColors)
        {
            if (targetColors.TryGetValue(key, out var targetColor) && IsValidColor(targetColor))
            {
                if (ColorConverter.ConvertFromString(targetColor) is Color targetSettingColor)
                {
                    return targetSettingColor;
                }
            }

            if (fallbackColors.TryGetValue(key, out var fallbackColor) && IsValidColor(fallbackColor))
            {
                if (ColorConverter.ConvertFromString(fallbackColor) is Color defaultSettingColor)
                {
                    return defaultSettingColor;
                }
            }

            if (BuiltInFallbackColors.TryGetValue(key, out var builtInColor) && IsValidColor(builtInColor))
            {
                if (ColorConverter.ConvertFromString(builtInColor) is Color builtInSettingColor)
                {
                    return builtInSettingColor;
                }
            }

            return Colors.Transparent;
        }

        /// <summary>
        /// 色文字列が#AARRGGBB形式かどうかを判定する。
        /// </summary>
        /// <param name="value">判定対象の色文字列。</param>
        /// <returns>有効な色文字列であればtrue。</returns>
        private static bool IsValidColor(string value)
        {
            return ThemeColorRegex.IsMatch(value);
        }

        /// <summary>
        /// 指定テーマをアプリケーションリソースへ適用する。
        /// </summary>
        /// <param name="theme">適用対象のテーマ。</param>
        /// <param name="saveAsLastTheme">最後に適用したテーマとして保存するかどうか。</param>
        public static void ApplyTheme(ThemeSettingModel theme, bool saveAsLastTheme = true)
        {
            var dict = new ResourceDictionary();

            var defaultThemeName = IsSystemLightTheme() ? "Light" : "Dark";
            var defaultTheme = LoadedThemes.FirstOrDefault(t => t.Name == defaultThemeName)
                               ?? new ThemeSettingModel();

            foreach (var pair in ThemeBrushResourceKeys)
            {
                dict[pair.Value] = new SolidColorBrush(GetThemeColor(pair.Key, theme.Colors, defaultTheme.Colors));
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
            CurrentTheme = theme;

            if (saveAsLastTheme)
            {
                AppSettingsManager.SaveLastThemeName(theme.Name);
            }
        }

        /// <summary>
        /// 埋め込みリソースからデフォルトテーマを実行ディレクトリへ展開する。
        /// </summary>
        public static void ExtractDefaultThemes()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            Directory.CreateDirectory(baseDir);

            ExtractResource("MainApplication.Themes.Light.json", Path.Combine(baseDir, "Light.json"));
            ExtractResource("MainApplication.Themes.Dark.json", Path.Combine(baseDir, "Dark.json"));

            string schemaDir = Path.Combine(baseDir, "schema");
            Directory.CreateDirectory(schemaDir);

            ExtractResource(
                "MainApplication.Themes.schema.ThemeSchema.json",
                Path.Combine(schemaDir, "ThemeSchema.json")
            );
        }

        /// <summary>
        /// 埋め込みリソースを指定パスへ展開する。
        /// </summary>
        /// <param name="resourceName">埋め込みリソース名。</param>
        /// <param name="outputPath">出力先パス。</param>
        private static void ExtractResource(string resourceName, string outputPath)
        {
            if (File.Exists(outputPath))
            {
                EnsureThemeHasDefaultKeys(resourceName, outputPath);
                return;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return;
            }

            using var file = File.Create(outputPath);
            stream.CopyTo(file);
        }

        /// <summary>
        /// 既存テーマファイルに不足している既定色キーを補完する。
        /// </summary>
        /// <param name="resourceName">既定テーマの埋め込みリソース名。</param>
        /// <param name="outputPath">補完対象のテーマファイルパス。</param>
        private static void EnsureThemeHasDefaultKeys(string resourceName, string outputPath)
        {
            if (!resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    return;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var defaultJson = reader.ReadToEnd();
                var defaultRoot = JsonNode.Parse(defaultJson) as JsonObject;
                var targetRoot = JsonNode.Parse(File.ReadAllText(outputPath, Encoding.UTF8)) as JsonObject;
                var defaultColors = defaultRoot?["colors"] as JsonObject;

                if (targetRoot == null || defaultColors == null)
                {
                    return;
                }

                if (targetRoot["colors"] is not JsonObject targetColors)
                {
                    targetColors = new JsonObject();
                    targetRoot["colors"] = targetColors;
                }

                bool updated = false;
                foreach (var color in defaultColors)
                {
                    if (!targetColors.ContainsKey(color.Key))
                    {
                        targetColors[color.Key] = color.Value?.DeepClone();
                        updated = true;
                    }
                }

                if (updated)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    File.WriteAllText(outputPath, targetRoot.ToJsonString(options), Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}

/* --- End of file --- */
