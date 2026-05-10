using MainApplication.ViewModels.Core;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// ARGB形式の色を編集するためのViewModel。
    /// </summary>
    public class ColorPickerViewModel : ViewModelBase
    {
        private static readonly Regex HexColorRegex = new("^#[0-9A-Fa-f]{8}$", RegexOptions.Compiled);
        private readonly string _initialDisplayHex;
        private bool _isSynchronizing;
        private int _alpha;
        private int _red;
        private int _green;
        private int _blue;
        private string _alphaText = "255";
        private string _redText = "255";
        private string _greenText = "255";
        private string _blueText = "255";
        private string _hexText;
        private string? _result;

        /// <summary>
        /// ColorPickerViewModelを生成する。
        /// </summary>
        /// <param name="initial">初期色文字列。</param>
        public ColorPickerViewModel(string? initial)
        {
            _initialDisplayHex = NormalizeOrDefault(initial);
            _hexText = _initialDisplayHex;
            ApplyHexToComponents(_initialDisplayHex);

            ConfirmCommand = new RelayCommand(Confirm, () => IsInputValid);
            ResetCommand = new RelayCommand(Reset);
        }

        /// <summary>
        /// アルファ値。
        /// </summary>
        public int Alpha
        {
            get => _alpha;
            set => SetComponent(ref _alpha, value, nameof(AlphaText));
        }

        /// <summary>
        /// アルファ値入力文字列。
        /// </summary>
        public string AlphaText
        {
            get => _alphaText;
            set => SetComponentText(ref _alphaText, value, ref _alpha, nameof(Alpha));
        }

        /// <summary>
        /// 赤成分。
        /// </summary>
        public int Red
        {
            get => _red;
            set => SetComponent(ref _red, value, nameof(RedText));
        }

        /// <summary>
        /// 赤成分入力文字列。
        /// </summary>
        public string RedText
        {
            get => _redText;
            set => SetComponentText(ref _redText, value, ref _red, nameof(Red));
        }

        /// <summary>
        /// 緑成分。
        /// </summary>
        public int Green
        {
            get => _green;
            set => SetComponent(ref _green, value, nameof(GreenText));
        }

        /// <summary>
        /// 緑成分入力文字列。
        /// </summary>
        public string GreenText
        {
            get => _greenText;
            set => SetComponentText(ref _greenText, value, ref _green, nameof(Green));
        }

        /// <summary>
        /// 青成分。
        /// </summary>
        public int Blue
        {
            get => _blue;
            set => SetComponent(ref _blue, value, nameof(BlueText));
        }

        /// <summary>
        /// 青成分入力文字列。
        /// </summary>
        public string BlueText
        {
            get => _blueText;
            set => SetComponentText(ref _blueText, value, ref _blue, nameof(Blue));
        }

        /// <summary>
        /// #AARRGGBB形式の色文字列。
        /// </summary>
        public string HexText
        {
            get => _hexText;
            set
            {
                if (!SetProperty(ref _hexText, value, [nameof(IsHexValid), nameof(IsInputValid), nameof(SelectedBrush)]))
                {
                    return;
                }

                CommandManager.InvalidateRequerySuggested();

                if (_isSynchronizing || !IsHexValid)
                {
                    return;
                }

                _isSynchronizing = true;
                ApplyHexToComponents(_hexText);
                _isSynchronizing = false;
            }
        }

        /// <summary>
        /// 入力中の色文字列が有効かどうか。
        /// </summary>
        public bool IsHexValid => IsValidHexColor(HexText);

        /// <summary>
        /// 確定可能な入力状態かどうか。
        /// </summary>
        public bool IsInputValid => IsHexValid &&
                                    IsValidComponentText(AlphaText) &&
                                    IsValidComponentText(RedText) &&
                                    IsValidComponentText(GreenText) &&
                                    IsValidComponentText(BlueText);

        /// <summary>
        /// 現在選択中の色を表示するブラシ。
        /// </summary>
        public Brush SelectedBrush => CreateBrush(HexText);

        /// <summary>
        /// 初期色を表示するブラシ。
        /// </summary>
        public Brush InitialBrush => CreateBrush(_initialDisplayHex);

        /// <summary>
        /// 色を確定するコマンド。
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// 初期色に戻すコマンド。
        /// </summary>
        public ICommand ResetCommand { get; }

        /// <summary>
        /// ダイアログの結果として返す色文字列。
        /// </summary>
        public string? Result
        {
            get => _result;
            private set => SetProperty(ref _result, value);
        }

        /// <summary>
        /// 色文字列が#AARRGGBB形式かどうかを判定する。
        /// </summary>
        /// <param name="value">判定対象の文字列。</param>
        /// <returns>有効な色文字列であればtrue。</returns>
        public static bool IsValidHexColor(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && HexColorRegex.IsMatch(value);
        }

        /// <summary>
        /// 色文字列を#AARRGGBB形式の大文字表記に正規化する。
        /// </summary>
        /// <param name="value">正規化対象の文字列。</param>
        /// <returns>正規化された色文字列。</returns>
        public static string Normalize(string value)
        {
            return value.ToUpperInvariant();
        }

        /// <summary>
        /// 成分値を設定し、必要に応じて色文字列を更新する。
        /// </summary>
        /// <param name="field">更新対象フィールド。</param>
        /// <param name="value">設定する値。</param>
        /// <param name="textPropertyName">連動する入力文字列プロパティ名。</param>
        /// <param name="propertyName">変更通知対象プロパティ名。</param>
        private void SetComponent(
            ref int field,
            int value,
            string textPropertyName,
            [CallerMemberName] string? propertyName = null
        )
        {
            int clampedValue = Math.Clamp(value, 0, 255);
            if (!SetProperty(ref field, clampedValue, [nameof(IsInputValid), nameof(SelectedBrush)], propertyName))
            {
                return;
            }

            SetComponentTextFromValue(textPropertyName, clampedValue);
            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            HexText = FormatHexFromComponents();
            _isSynchronizing = false;
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// 成分値入力文字列を設定し、有効な場合は数値と色文字列を更新する。
        /// </summary>
        /// <param name="textField">更新対象入力文字列フィールド。</param>
        /// <param name="value">設定する入力文字列。</param>
        /// <param name="componentField">更新対象成分値フィールド。</param>
        /// <param name="componentPropertyName">連動する成分値プロパティ名。</param>
        /// <param name="propertyName">変更通知対象プロパティ名。</param>
        private void SetComponentText(
            ref string textField,
            string value,
            ref int componentField,
            string componentPropertyName,
            [CallerMemberName] string? propertyName = null
        )
        {
            if (!SetProperty(ref textField, value, [nameof(IsInputValid)], propertyName))
            {
                return;
            }

            CommandManager.InvalidateRequerySuggested();
            if (_isSynchronizing || !TryParseComponentText(value, out var componentValue))
            {
                return;
            }

            _isSynchronizing = true;
            if (SetProperty(ref componentField, componentValue, [nameof(IsInputValid), nameof(SelectedBrush)], componentPropertyName))
            {
                HexText = FormatHexFromComponents();
            }

            _isSynchronizing = false;
        }

        /// <summary>
        /// 成分値を対応する入力文字列へ反映する。
        /// </summary>
        /// <param name="textPropertyName">入力文字列プロパティ名。</param>
        /// <param name="value">反映する成分値。</param>
        private void SetComponentTextFromValue(string textPropertyName, int value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);
            switch (textPropertyName)
            {
                case nameof(AlphaText):
                    SetProperty(ref _alphaText, text, [nameof(IsInputValid)], nameof(AlphaText));
                    break;
                case nameof(RedText):
                    SetProperty(ref _redText, text, [nameof(IsInputValid)], nameof(RedText));
                    break;
                case nameof(GreenText):
                    SetProperty(ref _greenText, text, [nameof(IsInputValid)], nameof(GreenText));
                    break;
                case nameof(BlueText):
                    SetProperty(ref _blueText, text, [nameof(IsInputValid)], nameof(BlueText));
                    break;
            }
        }

        /// <summary>
        /// 現在のARGB成分から色文字列を生成する。
        /// </summary>
        /// <returns>#AARRGGBB形式の色文字列。</returns>
        private string FormatHexFromComponents()
        {
            return $"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
        }

        /// <summary>
        /// 色文字列をARGB成分へ反映する。
        /// </summary>
        /// <param name="hex">#AARRGGBB形式の色文字列。</param>
        private void ApplyHexToComponents(string hex)
        {
            Alpha = ParseHexComponent(hex, 1);
            Red = ParseHexComponent(hex, 3);
            Green = ParseHexComponent(hex, 5);
            Blue = ParseHexComponent(hex, 7);
        }

        /// <summary>
        /// 成分値入力文字列が0から255までの整数かどうかを判定する。
        /// </summary>
        /// <param name="value">判定対象文字列。</param>
        /// <returns>有効な成分値入力であればtrue。</returns>
        private static bool IsValidComponentText(string? value)
        {
            return TryParseComponentText(value, out _);
        }

        /// <summary>
        /// 成分値入力文字列を整数値へ変換する。
        /// </summary>
        /// <param name="value">変換対象文字列。</param>
        /// <param name="component">変換後成分値。</param>
        /// <returns>変換できた場合はtrue。</returns>
        private static bool TryParseComponentText(string? value, out int component)
        {
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out component) &&
                   component >= 0 &&
                   component <= 255;
        }

        /// <summary>
        /// 色文字列から指定位置の成分を取得する。
        /// </summary>
        /// <param name="hex">#AARRGGBB形式の色文字列。</param>
        /// <param name="startIndex">成分の開始位置。</param>
        /// <returns>成分値。</returns>
        private static int ParseHexComponent(string hex, int startIndex)
        {
            return int.Parse(hex.Substring(startIndex, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 色文字列を正規化し、不正な場合は白を返す。
        /// </summary>
        /// <param name="value">正規化対象の文字列。</param>
        /// <returns>表示に使用する色文字列。</returns>
        private static string NormalizeOrDefault(string? value)
        {
            return IsValidHexColor(value) ? Normalize(value!) : "#FFFFFFFF";
        }

        /// <summary>
        /// 色文字列から表示用ブラシを生成する。
        /// </summary>
        /// <param name="hex">色文字列。</param>
        /// <returns>表示用ブラシ。</returns>
        private static Brush CreateBrush(string hex)
        {
            if (!IsValidHexColor(hex))
            {
                return Brushes.Transparent;
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        /// <summary>
        /// 現在の有効な色を確定する。
        /// </summary>
        private void Confirm()
        {
            if (!IsInputValid)
            {
                return;
            }

            Result = Normalize(HexText);
        }

        /// <summary>
        /// 編集内容を初期色へ戻す。
        /// </summary>
        private void Reset()
        {
            HexText = _initialDisplayHex;
        }
    }
}

/* --- End of file --- */
