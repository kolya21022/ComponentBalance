using System;
using System.Collections.Generic;

namespace ComponentBalanceSqlv2.Utils
{
    /// <summary>
    /// Служебные константы приложения (шрифт, цвета, относительные пути отчёта)
    /// </summary>
    public static class Constants
    {
        public const string SerializeError = "Все вложенные классы сущности и этот класс должны быть [Serializable]";
        public const string LogicErrorPattern = "Ошибка логики приложения: {0}";
        public const string BuildDateTimePattern = "{0:yyyy.MM.dd \'/\' HH:mm}";

        // Названия относительных каталогов с отчётами/название файла отчёта
        public const string ReportFolder = "Reports";
        public const string AppDataFolder = "App_Data";

        // Шаблоны ФИО: полный и сокращённый (фамилия и инициалы с точками)
        public const string EmployeeFullNamePattern = "{0} {1} {2}";
        public const string EmployeeShortNamePattern = "{0} {1}.{2}.";

        public const string FilterBarCoverLabel = "Нажмите ПКМ по столбцу для фильтрации";

        // Шаблоны надписей над таблицами
        public const string PatternCountItemsTable = "[Записей: {0}]";
        public const string PatternEmployeesTableWorkMonth = "[Раб.месяц: {0}]";

        // Описания хоткеев страниц
        public const string HotkeyLabelsSeparator = ", ";
        public const string HotkeyLabelEdit = "[Двойной клик мышкой по полю] - редактировать";
        public const string HotkeyLabelFilter = "[ПКМ по столбцу] - фильтрация";
        public const string HotkeyLabelJumpNext = "[Enter/↓] - следующее поле";
        public const string HotkeyLabelJumpPrevious = "[↑] - Предыдущее поле";
        public const string HotkeyLabelJump = "[F4] - начать/отменить корректировку";

        /// <summary>
        /// Названия всех месяцов. 
        /// </summary>
        public static Dictionary<int, string> MonthsFullNames = Common.MonthsFullNames();
    }
}
