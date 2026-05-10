using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MainApplication.ViewModels.Converters
{
    /// <summary>
    /// bool値をVisibilityに変換するコンバータ。
    /// true → Visible、false → Collapsed を返す。
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /* ---------------------------------------------------------
         * Convert(bool → Visibility)
         * --------------------------------------------------------- */

        /// <summary>
        /// bool値をVisibilityに変換する。
        /// trueの場合はVisible、falseの場合はCollapsedを返す。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        /* ---------------------------------------------------------
         * ConvertBack(未実装)
         * --------------------------------------------------------- */

        /// <summary>
        /// Visibility → boolの逆変換は未対応。
        /// 使用された場合は例外を投げる。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

/* --- End of file --- */
