using System.ComponentModel.DataAnnotations.Schema;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Движение [Докомплектация].
    /// </summary>
    public class EquipmentMove : Move
    {
        /// <summary>
        /// Номер акта докомплектации.
        /// </summary>
        [Column("Number")]
        public string Number { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Выпущенные изделия], 
        /// указывающий какое выпущенное изделие было докомплектовано.
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
            return $"Расход - Комплектация {ReleaseProduct.FactoryNumber}";
        }

        /// <summary>
        /// Получить описание движения.
        /// </summary>
        public override string GetDisplayDiscription()
        {
            return $"{ReleaseProduct.Product.Code} {ReleaseProduct.Product.Name} {ReleaseProduct.Product.Designation}; Акт: {Number}";
        }

        public EquipmentMove()
        {
            IsSupply = false;
            IsUserCanDelete = true;
        }

        public EquipmentMove(long balanceId, decimal count, decimal cost, long releaseProductId, string number, MonthYear monthYear) : this()
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
