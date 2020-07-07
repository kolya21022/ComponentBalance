using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись таблицы [МесяцГод].
    /// </summary>
    public class MonthYear
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        
        /// <summary>
        /// Месяц.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Год.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Описание что пара МесяцГод обозначает.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Месяц строковый с 0 в начале.
        /// </summary>
        public string MonthToString => $"{Month:d2}";

        /// <summary>
        /// Полное название месяца.
        /// </summary>
        public string DisplayMonth => Constants.MonthsFullNames[Month];

        public MonthYear() { }

        public MonthYear(int month, int year)
        {
            Month = month;
            Year = year;
        }

        /// <summary>
        /// Получить следующий месяц.
        /// </summary>
        public MonthYear GetNextMonth()
        {
            var date = FirstDate.AddMonths(1);
            return new MonthYear(date.Month, date.Year);
        }
        /// <summary>
        /// Получить предыдущий месяц.
        /// </summary>
        public MonthYear GetPreviousMonth()
        {
            var date = FirstDate.AddMonths(-1);
            return new MonthYear(date.Month, date.Year);
        }
        /// <summary>
        /// Получить дату на первое число месяца.
        /// </summary>
        public DateTime FirstDate => new DateTime(Year, Month, 1);

        /// <summary>
        /// Получить дату на последнее число месяца.
        /// </summary>
        public DateTime EndDate => new DateTime(Year, Month + 1, 1).AddDays(-1);
    }
}