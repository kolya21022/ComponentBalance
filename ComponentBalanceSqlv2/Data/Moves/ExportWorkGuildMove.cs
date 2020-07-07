using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [В другой цех].
    /// </summary>
    public class ExportWorkGuildMove : Move
    {
        /// <summary>
        /// Вторичный ключ на таблицу [Балансы], 
        /// указывающий на какой баланс(цех) была передана(ушла) деталь.
        /// </summary>
        [Column("TransferBalanceId")]
        public long TransferBalanceId { get; set; }
        [ForeignKey("TransferBalanceId")]
        public virtual Balance TransferBalance { get; set; }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return $"Расход - В другой цех: {TransferBalance.WorkGuild.WorkGuildNumber}";
        }

        public ExportWorkGuildMove()
        {
            IsSupply = false;
            IsUserCanDelete = true;
        }

        public ExportWorkGuildMove(long balanceId, decimal count, decimal cost, long transferBalanceId, MonthYear monthYear) : this()
        {
            BalanceId = balanceId;
            Count = count;
            Cost = cost;
            TransferBalanceId = transferBalanceId;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
