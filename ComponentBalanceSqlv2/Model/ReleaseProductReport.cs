using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Model
{
    /// <summary>
    /// Запись отчета [Выпущенные изделия].
    /// </summary>
    public class ReleaseProductReport
    {
        public int WorkGuildNumber { get; set; }
        public string FactoryNumber { get; set; }
        public int CountNumber { get; set; }
        public long ProductId { get; set; }      
        public string ProductName { get; set; }
        public string ProductMark { get; set; }
        public long DetailCode { get; set; }
        public string DetailName { get; set; }
        public string DetailMark { get; set; }
        public string DetailGost { get; set; }
        public string Measure { get; set; }
        public string Tmc { get; set; }
        public decimal Count { get; set; }
        public decimal Cost { get; set; }
        public string IsEquipment { get; set; }

        public ReleaseProductReport(int workGuildNumber, string factoryNumber, int countNumber,
            long productId, string productName, string productMark,
            long detailCode, string detailName, string detailMark, string detailGost, string measure,
            string tmc, decimal count, decimal cost, string isEquipment)
        {
            WorkGuildNumber = workGuildNumber;
            FactoryNumber = factoryNumber;
            CountNumber = countNumber;
            ProductId = productId;            
            ProductName = productName;
            ProductMark = productMark;
            DetailCode = detailCode;
            DetailName = detailName;
            DetailMark = detailMark;
            DetailGost = detailGost;
            Measure = measure;
            Tmc = tmc;
            Count = count;
            Cost = cost;
            IsEquipment = isEquipment;
        }

        /// <summary>
        /// Собрать выборку для отчета Выпуска
        /// </summary>
        public static List<ReleaseProductReport> GetReleaseProductReport(DomainContext context,
            WorkGuild workGuild, MonthYear monthYear)
        {
            var result = new List<ReleaseProductReport>();

            var releaseProducts = context.ReleaseProducts
                .Where(i => i.Month == monthYear.Month
                            && i.Year == monthYear.Year)
                .Include(c => c.WorkGuild) // Цех
                .Include(c => c.Product) // Продукт
                .Include(c => c.ReleaseMoves // Выпуск и вложение до тмц
                    .Select(x => x.Balance.Detail.TmcType))
                .Include(c => c.ReleaseMoves // Выпуск и вложение до ед изм
                    .Select(x => x.Balance.Detail.Measure))

                .Include(c => c.EquipmentMoves // Выпуск и вложение до тмц
                    .Select(x => x.Balance.Detail.TmcType))
                .Include(c => c.EquipmentMoves // Выпуск и вложение до ед изм
                    .Select(x => x.Balance.Detail.Measure))
                .ToList();

            if (workGuild != null)
            {
                releaseProducts = releaseProducts.Where(i => i.WorkGuildId == workGuild.Id).ToList();
            }

            var releases = new List<ReleaseInfo>();
            foreach (var releaseProduct in releaseProducts)
            {
                var release = new ReleaseInfo()
                {
                    WorkGuild = releaseProduct.WorkGuild,
                    FactoryNumber = releaseProduct.FactoryNumber,
                    Product = releaseProduct.Product,
                    Moves = new List<Move>()
                };
                foreach (var move in releaseProduct.ReleaseMoves)
                {
                    release.Moves.Add(new Move()
                    {
                        BalanceId = move.BalanceId,
                        Balance = move.Balance,
                        Count = move.Count,
                        Cost = move.Cost
                    });
                }

                foreach (var move in releaseProduct.EquipmentMoves)
                {
                    release.Moves.Add(new Move()
                    {
                        BalanceId = move.BalanceId,
                        Balance = move.Balance,
                        Count = move.Count,
                        Cost = move.Cost
                    });
                }
                releases.Add(release);
            }

            var releasesGroup = releases.GroupBy(p => new { p.Product, p.WorkGuild, Moves = new KeyMoves(p.Moves.OrderBy(i => i.BalanceId)) },
                (key, group) => new
                {
                    key.Product,
                    key.WorkGuild,
                    group.First().Moves,
                    group
                });

            foreach (var releaseGroup in releasesGroup)
            {
                var factoryNumbers = string.Empty;
                var count = 0;
                foreach (var releaseInfo in releaseGroup.group)
                {
                    factoryNumbers += releaseInfo.FactoryNumber + " ";
                    count++;
                }

                foreach (var move in releaseGroup.Moves)
                {
                    var isEquipment = move.GetType() == typeof(EquipmentMove) ? "+" : "-";

                    result.Add(new ReleaseProductReport(releaseGroup.WorkGuild.WorkGuildNumber,
                        factoryNumbers,
                        count,
                        releaseGroup.Product.Code,
                        releaseGroup.Product.Name,
                        releaseGroup.Product.Designation,
                        move.Balance.Detail.Code,
                        move.Balance.Detail.Name,
                        move.Balance.Detail.Designation,
                        move.Balance.Detail.Gost,
                        move.Balance.Detail.Measure.ShortName,
                        move.Balance.Detail.TmcType.Name,
                        move.Count,
                        move.Cost,
                        isEquipment));
                }
            }

            return result;
        }

        private class ReleaseInfo
        {
            public string FactoryNumber { get; set; }
            public WorkGuild WorkGuild { get; set; }
            public Product Product { get; set; }
            public List<Move> Moves { get; set; }
        }
    }
}
