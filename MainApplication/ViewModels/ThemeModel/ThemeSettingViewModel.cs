using MainApplication.Models.Settings;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace MainApplication.ViewModels.ThemeModel
{
    /// <summary>
    /// テーマ設定ウィンドウの状態と保存処理を管理するViewModel。
    /// </summary>
    public class ThemeSettingViewModel : ViewModelBase
    {
        private static readonly Regex ThemeColorRegex = new("^#[0-9A-Fa-f]{8}$", RegexOptions.Compiled);
        private readonly IColorPickerService _colorPicker;
        private string _themeName = "";

        /// <summary>
        /// テーマ名。
        /// </summary>
        public string ThemeName
        {
            get => _themeName;
            set => SetProperty(ref _themeName, value);
        }

        /// <summary>
        /// 編集可能なテーマ色一覧。
        /// </summary>
        public ObservableCollection<ColorItemViewModel> EditableColors { get; } = [];

        /// <summary>
        /// テーマを保存するコマンド。
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// 色編集ダイアログを開くコマンド。
        /// </summary>
        public ICommand EditColorCommand { get; }

        /// <summary>
        /// テーマ設定ViewModelを生成する。
        /// </summary>
        /// <param name="source">編集元のテーマ設定。</param>
        /// <param name="colorPicker">色編集サービス。</param>
        public ThemeSettingViewModel(ThemeSettingModel source, IColorPickerService? colorPicker = null)
        {
            _colorPicker = colorPicker ?? new ColorPickerService();
            ThemeName = source.Name;

            var defaultColors = new ThemeSettingModel().Colors;
            var supportedKeys = defaultColors.Keys;

            foreach (var key in supportedKeys)
            {
                source.Colors.TryGetValue(key, out var value);
                defaultColors.TryGetValue(key, out var defaultValue);
                EditableColors.Add(new ColorItemViewModel(key, value ?? defaultValue ?? ""));
            }

            SaveCommand = new RelayCommand(Save);
            EditColorCommand = new RelayCommand<ColorItemViewModel>(EditColor, item => item != null);
        }

        /// <summary>
        /// テーマ保存時に通知するイベント。
        /// </summary>
        public event Action? ThemeSaved;

        /// <summary>
        /// 現在の編集内容をテーマファイルとして保存し、即時適用する。
        /// </summary>
        private void Save()
        {
            var model = new ThemeSettingModel
            {
                Name = ThemeName,
                Colors = EditableColors.Where(x => IsValidThemeColor(x.Value))
                                       .ToDictionary(x => x.Key, x => ColorPickerViewModel.Normalize(x.Value))
            };

            ThemeManager.SaveTheme(model);
            ThemeManager.LoadThemes();
            ThemeManager.ApplyTheme(model);
            ThemeSaved?.Invoke();
        }

        /// <summary>
        /// 色編集ダイアログを開き、確定した色を対象項目へ反映する。
        /// </summary>
        /// <param name="item">編集対象の色項目。</param>
        private void EditColor(ColorItemViewModel? item)
        {
            if (item == null)
            {
                return;
            }

            var picked = _colorPicker.EditColor(item.Value, IsValidThemeColor);
            if (picked != item.Value)
            {
                item.Value = picked;
            }
        }

        /// <summary>
        /// 色項目を追加する。
        /// </summary>
        private void AddColor()
        {
            EditableColors.Add(new ColorItemViewModel("new-key", "#FFFFFFFF"));
        }

        /// <summary>
        /// 色項目を削除する。
        /// </summary>
        /// <param name="item">削除対象の色項目。</param>
        private void RemoveColor(ColorItemViewModel? item)
        {
            if (item != null)
            {
                EditableColors.Remove(item);
            }
        }

        /// <summary>
        /// テーマ色文字列が有効かどうかを判定する。
        /// </summary>
        /// <param name="value">判定対象の色文字列。</param>
        /// <returns>有効な色文字列であればtrue。</returns>
        private static bool IsValidThemeColor(string value)
        {
            return ThemeColorRegex.IsMatch(value);
        }
    }

    /// <summary>
    /// 色1項目を表すViewModel。
    /// </summary>
    public class ColorItemViewModel : ViewModelBase
    {
        private string _key;
        private string _value;

        /// <summary>
        /// 色設定キー。
        /// </summary>
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        /// <summary>
        /// #AARRGGBB形式の色文字列。
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value, [nameof(PreviewBrush)]);
        }

        /// <summary>
        /// 色プレビュー用ブラシ。
        /// </summary>
        public Brush PreviewBrush
        {
            get
            {
                if (!ColorPickerViewModel.IsValidHexColor(Value))
                {
                    return Brushes.Transparent;
                }

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Value));
            }
        }

        /// <summary>
        /// ColorItemViewModelを生成する。
        /// </summary>
        /// <param name="key">色設定キー。</param>
        /// <param name="value">色文字列。</param>
        public ColorItemViewModel(string key, string value)
        {
            _key = key;
            _value = value;
        }
    }
}

/* --- End of file --- */
