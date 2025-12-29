using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MainApplication.ViewModels.Converters
{
    /// <summary>
    /// 選択状態(bool)に応じて背景色を返すコンバータ。
    /// true → LightGreen、false → LightBlueを返す。
    /// </summary>
    public class SelectionBrushConverter : IValueConverter
    {
        /* ---------------------------------------------------------
         * Convert(bool → Brush)
         * --------------------------------------------------------- */

        /// <summary>
        /// 選択状態に応じてBrushを返す。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value is bool b && b;
            return isSelected ? Brushes.LightGreen : Brushes.LightBlue;
        }

        /* ---------------------------------------------------------
         * ConvertBack(未使用)
         * --------------------------------------------------------- */

        /// <summary>
        /// 逆変換は不要のため未実装。
        /// 使用された場合は例外を投げる。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

/* --- End of file --- */
