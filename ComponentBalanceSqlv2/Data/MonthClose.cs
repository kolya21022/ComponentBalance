using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись таблицы [Закрытые месяца].
    /// </summary>
    public class MonthClose
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Цеха], 
        /// указывающий какой цеха закрыл/не закрыл месяц.
        /// </summary>
        public long WorkGuildId { get; set; }
        public virtual WorkGuild WorkGuild { get; set; }

        /// <summary>
        /// Флаг, указывающий закрыт ли месяц [Month] года [Year] цеха [WorkGuild] для изменнений.
        /// True - месяц закрыт (нельзя редактировать).
        /// False - месяц открыт (можно редактировать).
        /// </summary>
        public bool IsClose { get; set; }

        /// <summary>
        /// Месяц.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Год.
        /// </summary>
        public int Year { get; set; }
    }
}