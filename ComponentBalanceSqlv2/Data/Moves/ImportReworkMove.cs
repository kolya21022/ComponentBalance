using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Приход доработка].
    /// </summary>
    public class ImportReworkMove : Move
    {
        /// <summary>
        /// Вторичный ключ на таблицу [Выпущенные изделия], 
        /// указывающий в какое выпущенное изделие было поставлено(доработано).
        /// </summary>
        [Column("ReleaseProductId")]
        public long ReleaseProductId { get; set; }
        [ForeignKey("ReleaseProductId")]
        public virtual ReleaseProduct ReleaseProduct { get; set; }

        /// <summary>
        /// Номер акта доработки.
        /// </summary>
        [Column("Number")]
        public string Number { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Приход - Доработка {ReleaseProduct.FactoryNumber}";
        }

        /// <summary>
        /// Получить описание движения.
        /// </summary>
        public override string GetDisplayDiscription()
        {
            return $"{ReleaseProduct.Product.Code} {ReleaseProduct.Product.Name} {ReleaseProduct.Product.Designation}; Акт - {Number}";
        }

        public ImportReworkMove()
        {
            IsSupply = true;
            IsUserCanDelete = false;
        }

        public ImportReworkMove(long balanceId, decimal count, decimal cost, long releaseProductId, string number,
            MonthYear monthYear) : this()
        {
            BalanceId = balanceId;
            Count = count;
            Cost = cost;
            ReleaseProductId = releaseProductId;
            Number = number;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
