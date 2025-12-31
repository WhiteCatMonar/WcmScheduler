using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static MainApplication.ViewModels.ProjectModel.PortViewModel;

namespace MainApplication.ViewModels.Converters
{
    /// <summary>
    /// PortType(Input/Output)に応じてポートの色を返すコンバータ。
    /// Input → DarkGreen、Output → DarkRed、それ以外はGrayを返す。
    /// </summary>
    public class PortColorConverter : IValueConverter
    {
        /* ---------------------------------------------------------
         * Convert(PortType → Brush)
         * --------------------------------------------------------- */

        /// <summary>
        /// ポート種別に応じた色(Brush)を返す。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PortType type)
            {
                switch (type)
                {
                    case PortType.Input:
                        return Brushes.DarkGreen;
                    case PortType.Output:
                        return Brushes.DarkRed;
                    default:
                        return Brushes.Gray;
                }
            }
            return Brushes.Gray;
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
