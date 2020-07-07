using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Enums;

namespace ComponentBalanceSqlv2.Converters
{
    /// <summary>
    /// Конвертер из Enough значения в Brushes.
    /// </summary>
    public class EnoughToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                return null;
            }

            // RowType - тип элемента строки
            var item = value as IEnoughDetail;
            if (item == null)
            {
                return null;
            }
            switch (item.Enough)
            {
                case Enough.Yes:
                    return Brushes.Green;
                case Enough.Optional:
                    return Brushes.DarkOrange;
                case Enough.No:
                    return Brushes.Red;
                case Enough.Unknown:
                    return Brushes.Black;
                default:
                    return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}