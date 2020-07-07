using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Замены выпусков]
    /// </summary>
    public class ReleaseReplacementDetail
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Id выпущенного изделия.
        /// </summary>
        public long ReleaseProductId { get; set; }
        public virtual ReleaseProduct ReleaseProduct { get; set; }

        /// <summary>
        /// Id детали основной (по документации).
        /// </summary>
        public long DetailId { get; set; }
        public virtual Detail Detail { get; set; }

        /// <summary>
        /// Id баланса детали на которую заменили. (Если была замена то есть баланс на котором хранилась эта деталь)
        /// </summary>
        public long BalanceId { get; set; }
        public virtual Balance Balance { get; set; }

        /// <summary>
        /// Кол-во замененных деталей.
        /// </summary>
        public decimal Count { get; set; }

        public ReleaseReplacementDetail()
        {

        }

        public ReleaseReplacementDetail(long releaseProductId, 
            long detailId, long balanceId, decimal count) : this()
        {
            ReleaseProductId = releaseProductId;
            DetailId = detailId;
            BalanceId = balanceId;
            Count = count;
        }
    }
}
