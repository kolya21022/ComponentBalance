using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Из другого цеха].
    /// </summary>
    public class ImportWorkGuildMove : Move
    {
        /// <summary>
        /// Вторичный ключ на таблицу [Балансы], 
        /// указывающий c какого баланса(цеха) была передана(пришла) деталь.
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
            return $"Приход - Из другого цеха: {TransferBalance.WorkGuild.WorkGuildNumber}";
        }

        public ImportWorkGuildMove()
        {
            IsSupply = true;
            IsUserCanDelete = false;
        }

        public ImportWorkGuildMove(long balanceId, decimal count, decimal cost, long transferBalanceId, MonthYear monthYear) : this()
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
