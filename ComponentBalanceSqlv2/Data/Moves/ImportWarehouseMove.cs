namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Со склада].
    /// </summary>
    public class ImportWarehouseMove : Move
    {
        /// <summary>
        /// Вторичный ключ на таблицу [Изделий], 
        /// указывающий для какого изделия пришла деталь в цех.
        /// </summary>
        public long ProductId { get; set; }
        public virtual Product Product { get; set; }

        /// <summary>
        /// Описание движения.
        /// </summary>
        public override string GetDisplayMoveTypeInfo()
        {
            return "Приход - Со склада";
        }

        /// <summary>
        /// Получить информацию о типе движения.
        /// </summary>
        public override string GetDisplayDiscription()
        {
            return $"{Product.Code} {Product.Name} {Product.Designation}";
        }

        public ImportWarehouseMove()
        {
            IsSupply = true;
            IsUserCanDelete = false;
        }

        public ImportWarehouseMove(long balanceId, decimal count, decimal cost, long productId, MonthYear monthYear) : this()
        {
            BalanceId = balanceId;
            Count = count;
            Cost = cost;
            ProductId = productId;
            Month = monthYear.Month;
            Year = monthYear.Year;
        }
    }
}
