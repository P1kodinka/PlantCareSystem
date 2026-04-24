using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PlantCareSystem.Converters
{
    public class DateToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                if (date.Date == DateTime.Today)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3CD"));
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"));
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}