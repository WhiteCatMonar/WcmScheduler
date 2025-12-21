using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace MainApplication.ViewModels.Converters
{
    public class DisplayNameConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
