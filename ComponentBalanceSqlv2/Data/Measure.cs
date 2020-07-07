using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Еденицы измерения].
    /// </summary>
    public class Measure
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Заводской код еденицы измерения.
        /// </summary>
        public int OldDbCode { get; set; }

        /// <summary>
        /// Сокращенное название еденицы измерения.
        /// </summary>
        public string ShortName { get; set; }

        public virtual ICollection<Detail> Details { get; set; }

        public Measure()
        {
            Details = new HashSet<Detail>();
        }

        public Measure(int oldDbCode, string shortName) : this()
        {
            OldDbCode = oldDbCode;
            ShortName = shortName;
        }
    }
}
