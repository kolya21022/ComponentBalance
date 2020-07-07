using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Цеха].
    /// </summary>
    public class WorkGuild
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Номер цеха.
        /// </summary>
        public int WorkGuildNumber { get; set; }

        /// <summary>
        /// Пароль для входа.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Уровень доступа.
        /// </summary>
        public byte Lvl { get; set; }

        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<MonthClose> MonthCloses { get; set; }
        public virtual ICollection<ReleaseProduct> ReleaseProducts { get; set; }
        public virtual ICollection<CompositionProduct> CompositionProducts { get; set; }

        /// <summary>
        /// Строка [Цех: 'Номер'].
        /// </summary>
        public string Display => "Цех: " + WorkGuildNumber;

        /// <summary>
        /// Строковый номер цеха с 0 спереди если номер меньше 10.
        /// </summary>
        public string DisplayWorkguildString => $"{WorkGuildNumber:d2}";

        public WorkGuild()
        {
            Balances = new HashSet<Balance>();
            MonthCloses = new HashSet<MonthClose>();
            ReleaseProducts = new HashSet<ReleaseProduct>();
            CompositionProducts = new HashSet<CompositionProduct>();
        }

        public WorkGuild(int workGuildNumber) : this()
        {
            WorkGuildNumber = workGuildNumber;
        }
    }
}
