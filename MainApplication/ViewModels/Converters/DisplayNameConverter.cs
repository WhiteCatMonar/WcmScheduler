using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace MainApplication.ViewModels.Converters
{
    /// <summary>
    /// プロパティ名からDisplayNameAttributeの表示名を取得するコンバータ。
    /// UI表示用のラベル変換に使用する。
    /// </summary>
    public class DisplayNameConverter : IValueConverter
    {
        /* ---------------------------------------------------------
         * Convert(プロパティ名 → 表示名)
         * --------------------------------------------------------- */

        /// <summary>
        /// 指定されたオブジェクトのプロパティに付与された
        /// DisplayNameAttributeを取得し、その表示名を返す。
        /// DisplayNameAttributeが無い場合はプロパティ名を返す。
        /// </summary>
        /// <param name="value">対象オブジェクト</param>
        /// <param name="parameter">プロパティ名</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return string.Empty;
            }

            var type = value.GetType();
            var prop = type.GetProperty(parameter.ToString());
            var attr = prop?.GetCustomAttribute<DisplayNameAttribute>();
            return attr?.DisplayName ?? parameter.ToString();
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
