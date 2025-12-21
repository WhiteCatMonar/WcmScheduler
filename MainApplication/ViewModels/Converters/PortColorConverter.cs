using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static MainApplication.ViewModels.PortViewModel;

namespace MainApplication.ViewModels.Converters
{
    public class PortColorConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
