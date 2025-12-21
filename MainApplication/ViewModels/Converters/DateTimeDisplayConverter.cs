using System;
using System.Globalization;
using System.Windows.Data;

namespace MainApplication.ViewModels.Converters
{
    public class DateTimeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("yyyy/MM/dd HH:mm");
            }
            return "----/--/-- --:--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBackは不要。編集はモーダルで行うのでnullを返す
            return Binding.DoNothing;
        }
    }

}
