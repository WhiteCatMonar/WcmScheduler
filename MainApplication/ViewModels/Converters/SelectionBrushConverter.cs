using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MainApplication.ViewModels.Converters
{    public class SelectionBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value is bool b && b;
            return isSelected ? Brushes.LightGreen : Brushes.LightBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
