using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Брак].
    /// </summary>
    public class DefectMove : Move
    {
        /// <summary>
        /// Номер акта брака.
        /// </summary>
        [Column("Number")]
        public string Number { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Расход - Брак {Number}";
        }

        public DefectMove()
        {
            IsSupply = false;
            IsUserCanDelete = true;
        }

        public DefectMove(long balanceId, decimal count, decimal cost, string number, MonthYear monthYear) : this()
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
