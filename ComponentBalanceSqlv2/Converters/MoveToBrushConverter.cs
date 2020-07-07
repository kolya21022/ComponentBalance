using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Converters
{
    /// <summary>
    /// Конвертер из Move значения в Brushes.
    /// </summary>
    public class MoveToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                return null;
            }

            // RowType - тип элемента строки
            var move = value as Move;
            if (move == null)
            {
                return Brushes.Black;
            }

            if (move.IsSupply)
            {
                return Brushes.Green;
            }

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}