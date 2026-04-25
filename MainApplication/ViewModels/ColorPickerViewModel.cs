using MainApplication.ViewModels.Core;
using System.Globalization;
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

            ConfirmCommand = new RelayCommand(Confirm, () => IsHexValid);
            ResetCommand = new RelayCommand(Reset);
        }

        /// <summary>
        /// アルファ値。
        /// </summary>
        public int Alpha
        {
            get => _alpha;
            set => SetComponent(ref _alpha, value);
        }

        /// <summary>
        /// 赤成分。
        /// </summary>
        public int Red
        {
            get => _red;
            set => SetComponent(ref _red, value);
        }

        /// <summary>
        /// 緑成分。
        /// </summary>
        public int Green
        {
            get => _green;
            set => SetComponent(ref _green, value);
        }

        /// <summary>
        /// 青成分。
        /// </summary>
        public int Blue
        {
            get => _blue;
            set => SetComponent(ref _blue, value);
        }

        /// <summary>
        /// #AARRGGBB形式の色文字列。
        /// </summary>
        public string HexText
        {
            get => _hexText;
            set
            {
                if (!SetProperty(ref _hexText, value, [nameof(IsHexValid), nameof(SelectedBrush)]))
                {
                    return;
                }

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
        private void SetComponent(ref int field, int value)
        {
            int clampedValue = Math.Clamp(value, 0, 255);
            if (!SetProperty(ref field, clampedValue, [nameof(SelectedBrush)]))
            {
                return;
            }

            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            HexText = FormatHexFromComponents();
            _isSynchronizing = false;
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
            if (!IsHexValid)
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
