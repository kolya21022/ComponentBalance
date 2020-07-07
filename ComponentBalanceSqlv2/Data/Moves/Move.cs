using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Запись таблицы [Движения].
    /// </summary>
    public class Move
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Балансы], 
        /// указывающий какая деталь движется.
        /// </summary>
        public long BalanceId { get; set; }
        [ForeignKey("BalanceId")]
        public virtual Balance Balance { get; set; }

        /// <summary>
        /// Кол-во движущихся деталей.
        /// </summary>
        public decimal Count { get; set; }

        /// <summary>
        /// Цена по которой одна деталь движется.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Конечная стоймость всех движущихся деталей (до 2ух знаков).
        /// </summary>
        public decimal Sum => decimal.Round(Count * Cost, 2);

        /// <summary>
        /// Месяц когда было движение.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Год когда было движение.
        /// </summary>
        public int Year { get; set; }

        //public string Discriminator { get; set; }

        /// <summary>
        /// Флаг, указывающий движение является приходом или расходом.
        /// True - приход.
        /// False - расход.
        /// </summary>
        public bool IsSupply { get; set; }

        /// <summary>
        /// Флаг, указывающий может ли пользователь удалить проведенное движение.
        /// True - может удалить.
        /// False - не может удалить.
        /// </summary>
        public bool IsUserCanDelete { get; set; }

        /// <summary>
        /// Информация о типе движения.
        /// </summary>
        [NotMapped]
        public string DisplayMoveTypeInfo => GetDisplayMoveTypeInfo();

        /// <summary>
        /// Описание движения.
        /// </summary>
        [NotMapped]
        public string DisplayDiscription => GetDisplayDiscription();

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public virtual string GetDisplayMoveTypeInfo()
        {
            return string.Empty;
        }

        /// <summary>
        /// Получить описание движения.
        /// </summary>
        public virtual string GetDisplayDiscription()
        {
            return string.Empty;
        }
    }
}
