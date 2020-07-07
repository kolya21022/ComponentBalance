using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Запчасть].
    /// </summary>
    public class SparePartMove : Move
    {
        /// <summary>
        /// Номер акта продажи запчасти.
        /// </summary>
        [Column("Number")]
        public string Number { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Расход - Запчасть Акт: {Number}";
        }

        public SparePartMove()
        {
            IsSupply = false;
            IsUserCanDelete = true;
        }

        public SparePartMove(long balanceId, decimal count, decimal cost, string number, MonthYear monthYear) : this()
        {
            BalanceId = balanceId;
            Count = count;
            Cost = cost;
            Number = number;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
