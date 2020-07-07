using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [На склад].
    /// </summary>
    public class ExportWarehouseMove : Move
    {
        /// <summary>
        /// Номер склада.
        /// </summary>
        [Column("Number")]
        public string Number { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Расход - На склад {Number}";
        }

        public ExportWarehouseMove()
        {
            IsSupply = false;
            IsUserCanDelete = true;
        }

        public ExportWarehouseMove(long balanceId, decimal count, decimal cost, string number, MonthYear monthYear) : this()
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
