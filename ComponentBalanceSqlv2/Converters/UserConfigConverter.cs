using System;
using System.Windows.Data;

namespace ComponentBalanceSqlv2.Converters
{
    public class UserConfigConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var foxProDbFolderComposition = (string)values[0];
                var foxProDbFolderFox60ArmLimit = (string)values[1];
                var foxProDbFolderTemp = (string)values[2];
                var foxProDbFolderTempWork = (string)values[3];
                var foxProDbFolderBase = (string)values[4];
                var foxProDbFolderSkl = (string)values[5];
                var foxProDbFolderSkl58 = (string)values[6];
                var isRunningFullscreen = (bool)values[7];
                var fontSize = (string)values[8];

                return $"{foxProDbFolderComposition}|{foxProDbFolderFox60ArmLimit}|" +
                       $"{foxProDbFolderTemp}|{foxProDbFolderTempWork}|" +
                       $"{foxProDbFolderBase}|{foxProDbFolderSkl}|" +
                       $"{foxProDbFolderSkl58}|{isRunningFullscreen}|{fontSize}";
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
