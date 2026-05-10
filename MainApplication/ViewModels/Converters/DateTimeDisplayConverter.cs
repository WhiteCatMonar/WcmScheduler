using System;
using System.Globalization;
using System.Windows.Data;

namespace MainApplication.ViewModels.Converters
{
    /// <summary>
    /// DateTimeをUI表示用の文字列に変換するコンバータ。
    /// 値がnullまたはDateTimeでない場合はプレースホルダーを返す。
    /// </summary>
    public class DateTimeDisplayConverter : IValueConverter
    {
        /* ---------------------------------------------------------
         * Convert(DateTime → 表示文字列)
         * --------------------------------------------------------- */

        /// <summary>
        /// DateTimeを"yyyy/MM/dd HH:mm"形式の文字列に変換する。
        /// 値がDateTimeでない場合は"----/--/-- --:--"を返す。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("yyyy/MM/dd HH:mm");
            }
            return "----/--/-- --:--";
        }

        /* ---------------------------------------------------------
         * ConvertBack(未使用)
         * --------------------------------------------------------- */

        /// <summary>
        /// 逆変換は不要のため、Binding.DoNothingを返す。
        /// 編集はモーダルダイアログで行う前提。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /* ConvertBackは不要。編集はモーダルで行うのでnullを返す */
            return Binding.DoNothing;
        }
    } 
}

/* --- End of file --- */
