using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Типы ТМЦ].
    /// </summary>
    public class TmcType
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Название ТМЦ.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Соращенное название ТМЦ.
        /// </summary>
        public string ShortName { get; set; }

        public TmcType() { }

        public TmcType(string shortName)
        {
            ShortName = shortName;
        }

        public virtual ICollection<Detail> Details { get; set; }
    }
}
