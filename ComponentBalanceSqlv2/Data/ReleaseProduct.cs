using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Выпущенные изделия].
    /// </summary>
    public class ReleaseProduct
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Цеха], 
        /// указывающий какой цех выпустил изделие.
        /// </summary>
        public long WorkGuildId { get; set; }
        public virtual WorkGuild WorkGuild { get; set; }

        /// <summary>
        /// Заводской номер выпущенного изделия.
        /// </summary>
        public string FactoryNumber { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Изделия], 
        /// указывающий какое изделие было выпущено.
        /// </summary>
        public long ProductId { get; set; }
        public virtual Product Product { get; set; }

        /// <summary>
        /// Флаг, указывающий имеются ли в выпущенном изделии [Комплектующие] детали.
        /// True - имеются.
        /// False - отсутствуют.
        /// </summary>
        public bool IsHaveReleaseKom => ReleaseMoves.FirstOrDefault(i => i.Balance.Detail.TmcType.ShortName != "VSP" && i.Balance.Detail.TmcType.ShortName != "KOM") != null;

        /// <summary>
        /// Флаг, указывающий имеются ли в выпущенном изделии [Вспомогательные] детали.
        /// True - имеются.
        /// False - отсутствуют.
        /// </summary>
        public bool IsHaveReleaseVsp => ReleaseMoves.FirstOrDefault(i => i.Balance.Detail.TmcType.ShortName == "VSP" || i.Balance.Detail.TmcType.ShortName == "KOM") != null;

        /// <summary>
        /// Месяц выпуска изделия.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Год выпуска изделия.
        /// </summary>
        public int Year { get; set; }

        public virtual ICollection<ReleaseMove> ReleaseMoves { get; set; }
        public virtual ICollection<EquipmentMove> EquipmentMoves { get; set; }
        public virtual ICollection<ImportReworkMove> ImportReworkMoves { get; set; }
        public virtual ICollection<ExportReworkMove> ExportReworkMoves { get; set; }

        public virtual ICollection<ReleaseReplacementDetail> ReleaseReplacementDetails { get; set; }

        /// <summary>
        /// Все движения выпущенного изделия, без докомплектации.
        /// </summary>
        public IEnumerable<Move> Moves
        {
            get
            {
                var moves = new List<Move>();
                moves.AddRange(ReleaseMoves);
                moves.AddRange(ImportReworkMoves);
                moves.AddRange(ExportReworkMoves);
                return moves;
            }
        }

        /// <summary>
        /// Стоймость покупных деталей выпущенного изделия.
        /// </summary>
        public decimal TotalBuySum => Moves.Where(i => i.Balance.Detail.TmcType.ShortName == "KOM"
                                                       || i.Balance.Detail.TmcType.ShortName == "FAB"
                                                       || i.Balance.Detail.TmcType.ShortName == "INS")
            .Sum(move => (move.GetType() != typeof(ImportReworkMove) ? move.Count : -move.Count) * move.Cost);

        /// <summary>
        /// Стоймость материалов выпущенного изделия.
        /// </summary>
        public decimal TotalMaterialSum => Moves.Where(i => i.Balance.Detail.TmcType.ShortName == "VSP"
                                                                   || i.Balance.Detail.TmcType.ShortName == "PLA")
            .Sum(move => (move.GetType() != typeof(ImportReworkMove) ? move.Count : -move.Count) * move.Cost);

        public ReleaseProduct()
        {
            ReleaseMoves = new HashSet<ReleaseMove>();
            EquipmentMoves = new HashSet<EquipmentMove>();
            ImportReworkMoves = new HashSet<ImportReworkMove>();
            ExportReworkMoves = new HashSet<ExportReworkMove>();

            ReleaseReplacementDetails = new HashSet<ReleaseReplacementDetail>();
        }

        public ReleaseProduct(long workGuildId, long productId, string factoryNumber, MonthYear monthYear) : this()
        {
            WorkGuildId = workGuildId;
            ProductId = productId;
            FactoryNumber = factoryNumber;
            Month = monthYear.Month;
            Year = monthYear.Year;      
        }

        /// <summary>
        /// Удалить выпущенное изделие.
        /// </summary>
        public void Delete(DomainContext context)
        {
            var balancesId = Moves.Select(j => j.BalanceId);
            var balances = context.Balances
                .Where(i => balancesId.Contains(i.Id))
                .ToList();

            context.Moves.RemoveRange(Moves);
            context.ReleaseReplacementDetails.RemoveRange(context.ReleaseReplacementDetails.Where(i => i.ReleaseProductId == Id));

            foreach (var balance in balances)
            {
                balance.CountEnd = balance.GetRecalculateCountEnd();
                context.Entry(balance).State = EntityState.Modified;
            }

            context.ReleaseProducts.Remove(context.ReleaseProducts.First(i => i.Id == Id));
        }

        /// <summary>
        /// Удалить введенное комплектующие из выпущенного изделия.
        /// </summary>
        public void DeleteReleaseKom(DomainContext context)
        {
            var moveKom = ReleaseMoves.Where(i => i.Balance.Detail.TmcType.ShortName == "KOM"
                                                  || i.Balance.Detail.TmcType.ShortName == "FAB"
                                                  || i.Balance.Detail.TmcType.ShortName == "INS");
            DeleteReleaseMove(context, moveKom);
        }

        /// <summary>
        /// Удалить введенные вспомагательные из выпущенного изделия.
        /// </summary>
        public void DeleteReleaseVsp(DomainContext context)
        {
            var moveVsp = ReleaseMoves.Where(i => i.Balance.Detail.TmcType.ShortName == "VSP"
                                                  || i.Balance.Detail.TmcType.ShortName == "PLA");
            DeleteReleaseMove(context, moveVsp);
        }

        /// <summary>
        /// Удалить указанные движения из выпуска.
        /// </summary>
        private void DeleteReleaseMove(DomainContext context, IEnumerable<ReleaseMove> moves)
        {
            var releaseMoves = moves as ReleaseMove[] ?? moves.ToArray();
            var balancesId = releaseMoves.Select(j => j.BalanceId);
            var balances = context.Balances
                .Where(i => balancesId.Contains(i.Id))
                .ToList();

            foreach (var move in releaseMoves)
            {
                context.ReleaseMoves.Remove(context.ReleaseMoves.First(i => i.Id == move.Id));
            }
            context.ReleaseReplacementDetails.RemoveRange(context.ReleaseReplacementDetails.Where(i => balancesId.Any(j => j == i.BalanceId)));

            foreach (var balance in balances)
            {
                balance.CountEnd = balance.GetRecalculateCountEnd();
                context.Entry(balance).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Удалить движение.
        /// </summary>
        public void DeleteMove(DomainContext context, long balanceId, Move move)
        {
            var balance = context.Balances.First(i => i.Id == balanceId);
            balance.DeleteMove(context, move);
            context.Entry(balance).State = EntityState.Modified;
        }

        /// <summary>
        /// Флаг, указывающий является ли выпущенное изделие доработкой.
        /// True - является доработкой.
        /// False - не является доработкой.
        /// </summary>
        public bool IsRework => ImportReworkMoves.Any() || ExportReworkMoves.Any();

        /// <summary>
        /// Номер доработки.
        /// Если не доработка то выводит пустую строку.
        /// </summary>
        public string NumberRework => IsRework
            ? "Доработка " + (ImportReworkMoves.Any() ? ImportReworkMoves.First().Number : ExportReworkMoves.First().Number) 
            : string.Empty;
    }
}
