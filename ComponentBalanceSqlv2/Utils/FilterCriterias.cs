using System;
using System.Collections.Generic;

namespace ComponentBalanceSqlv2.Utils
{
    /// <summary>
    /// Критерии фильтрации данных в страницах таблиц сущностей.
    /// Также содержит сервисные методы поиска значений строковых представлений типов
    /// </summary>
    public class FilterCriterias
    {
        /// <summary>
        /// Критерии фильтров таблиц следующей структуры: ключ словаря - название слобца, 
        /// значение словаря - класс-обёрка для значения и отображаемого пользователю названия столбца.
        /// </summary>
        private readonly Dictionary<string, FilterValue> _filterCriterias = new Dictionary<string, FilterValue>();

        /// <summary>
        /// Проверка пустоты критериев
        /// </summary>
        public bool IsEmpty
        {
            get { return _filterCriterias == null || _filterCriterias.Count == 0; }
        }

        /// <summary>
        /// Получение отображаемого пользователю набора данных в таблицу фильтрации страницы
        /// </summary>
        public Dictionary<string, FilterValue> DisplayedDictionary
        {
            get { return _filterCriterias; }
        }

        /// <summary>
        /// Попытка получения значения фильтра в out-параметре (если параметр задан ранее)
        /// </summary>
        public bool GetValue(string key, out string value)
        {
            FilterValue buffer;
            var isValueExist = _filterCriterias.TryGetValue(key, out buffer);
            value = isValueExist ? buffer.Value : string.Empty;
            return isValueExist;
        }

        /// <summary>
        /// Очистка всех значениев фильтров страницы
        /// </summary>
        public void ClearAll()
        {
            _filterCriterias.Clear();
        }

        /// <summary>
        /// Удаление указанного критерия фильтрации
        /// </summary>
        public void RemoveCriteria(string column)
        {
            _filterCriterias.Remove(column);
        }

        /// <summary>
        /// Добавление/обновление значения фильтра
        /// </summary>
        public void UpdateCriteria(string column, string value, string displayedColumn)
        {
            _filterCriterias[column] = new FilterValue { Value = value, DisplayedColumn = displayedColumn };
        }

        /// <summary>
        /// Проверка наличия в строковом представлении указанного Long искомых значений, разделённых пробелами
        /// </summary>
        public static bool ContainsLong(long? source, string finded)
        {
            var valueInLine = source == null ? string.Empty : source.ToString();
            return ContainsLine(valueInLine, finded);
        }

        /// <summary>
        /// Проверка наличия в строковом представлении указанного Decimal искомых значений, разделённых пробелами
        /// </summary>
        public static bool ContainsDecimal(decimal? source, string finded)
        {
            var valueInLine = source == null ? string.Empty : source.ToString();
            return ContainsLine(valueInLine, finded);
        }

        /// <summary>
        /// Проверка наличия в строковом представлении указанной даты искомых значений, разделённых пробелами
        /// </summary>
        public static bool ContainsDate(DateTime? source, string finded)
        {
            var valueInLine = source == null ? string.Empty : ((DateTime)source).ToShortDateString();
            return ContainsLine(valueInLine, finded);
        }

        /// <summary>
        /// Проверка наличия в исходной строке искомых значений.
        /// Строка искомых значений разделяется на несколько по пробелами и проверяется наличие каждого из значений
        /// </summary>
        public static bool ContainsLine(string source, string finded)
        {
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(finded))
            {
                return false;
            }
            var isCoincided = true;
            foreach (var searchValue in finded.Trim().Split(null)) // разделение искомой строки в массив по пробелам
            {
                isCoincided &= source.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
            }
            return isCoincided;
        }
    }

    /// <summary>
    /// Значение фильтра (класс-обёртка для значения фильтрации и отображаемого название столбца)
    /// </summary>
    public class FilterValue
    {
        /// <summary>
        /// Отображаемое пользователю название столбца
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global /* NOTE: Геттер вызывается в xaml, не удалять */
        public string DisplayedColumn { get; set; }

        /// <summary>
        /// Значение фильтрации
        /// </summary>
        public string Value { get; set; }
    }
}


