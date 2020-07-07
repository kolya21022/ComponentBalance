using System.Windows.Media;

namespace ComponentBalanceSqlv2.Utils.Styles
{
    public static class Visual
    {
        public static readonly double FontSize = Properties.Settings.Default.FontSize;

        /// <summary>
        /// Получения объекта класса цвета визуальных элементов, по hex-коду цвета
        /// </summary>
        public static SolidColorBrush BrushHex(string hexColor)
        {
            var solidColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            if (solidColorBrush != null && !solidColorBrush.IsFrozen)
            {
                // NOTE: Делаем цвет нередактируемым, так как без этого сильно просаживается производительность.
                // Подробнее (Freezable Objects): https://msdn.microsoft.com/en-us/library/aa970683(v=vs.85).aspx
                solidColorBrush.Freeze();
            }
            return solidColorBrush;
        }

        // ReSharper disable InconsistentNaming  /* Названия цветов: http://chir.ag/projects/name-that-color/ */

        // Цвета текста
        public static readonly SolidColorBrush ForeColor1_BigStone = BrushHex("#1b293e"); // тёмно-синий
        public static readonly SolidColorBrush ForeColor2_PapayaWhip = BrushHex("#ffefd5"); // бежевый
        public static readonly SolidColorBrush ForeColor3_Yellow = BrushHex("#ffff00"); // жёлтый
        public static readonly SolidColorBrush ForeColor4_Red = BrushHex("#ff0000"); // красный
        public static readonly SolidColorBrush ForeColor5_Lochmara = BrushHex("#007acc"); // голубой
        public static readonly SolidColorBrush ForeColor6_Silver = BrushHex("#cccccc"); // серый
        public static readonly SolidColorBrush ForeColor7_White = BrushHex("#ffffff"); // белый
        public static readonly SolidColorBrush ForeColor8_GuardsmanRed = BrushHex("#ca1000");
        public static readonly SolidColorBrush ForeColor9_SeaGreen = BrushHex("#317a2e");

        // Цвета фонов
        public static readonly SolidColorBrush BackColor1_AthensGray = BrushHex("#eeeef2"); // светло-серый
        public static readonly SolidColorBrush BackColor2_Botticelli = BrushHex("#d6dbe9"); // серый
        public static readonly SolidColorBrush BackColor3_SanJuan = BrushHex("#364e6f"); // синий
        public static readonly SolidColorBrush BackColor4_BlueBayoux = BrushHex("#4d6082"); // синий
        public static readonly SolidColorBrush BackColor5_WaikawaGray = BrushHex("#566c92"); // синий
        public static readonly SolidColorBrush BackColor6_Lochmara = BrushHex("#007acc"); // голубой
        public static readonly SolidColorBrush BackColor7_BahamaBlue = BrushHex("#005c99"); // голубой
        public static readonly SolidColorBrush BackColor8_DiSerria = BrushHex("#d3a35b"); // бежевый

        // Цвета границ и линий
        public static readonly SolidColorBrush LineBorderColor1_BigStone = BrushHex("#1b293e"); // тёмно-синий
        public static readonly SolidColorBrush LineBorderColor2_Nepal = BrushHex("#8e9bbc"); // серый
        public static readonly SolidColorBrush LineBorderColor3_SanJuan = BrushHex("#364e6f"); // синий
        public static readonly SolidColorBrush LineBorderColor4_BlueBayoux = BrushHex("#4d6082"); // синий
        public static readonly SolidColorBrush LineBorderColor5_Sail = BrushHex("#b8d8f9"); // голубой
    }
}
