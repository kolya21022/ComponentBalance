using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Выпуск].
    /// </summary>
    public class ReleaseMove : Move
    {
        /// <summary>
        /// Вторичный ключ на таблицу [Выпущенные изделия], 
        /// указывающий в какое выпущенное изделие была добавлена деталь.
        /// </summary>
        [Column("ReleaseProductId")]
        public long ReleaseProductId { get; set; }
        [ForeignKey("ReleaseProductId")]
        public virtual ReleaseProduct ReleaseProduct { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Расход - Выпуск {ReleaseProduct.FactoryNumber}";
        }

        /// <summary>
        /// Получить описание движения.
        /// </summary>
        public override string GetDisplayDiscription()
        {
            return $"{ReleaseProduct.Product.Code} {ReleaseProduct.Product.Name} {ReleaseProduct.Product.Designation}";
        }

        public ReleaseMove()
        {
            IsSupply = false;
            IsUserCanDelete = false;
        }

        public ReleaseMove(long balanceId, decimal count, decimal cost, long releaseProductId, MonthYear monthYear) : this()
        {
            BalanceId = balanceId;
            Count = count;
            Cost = cost;
            ReleaseProductId = releaseProductId;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
