using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ComponentBalanceSqlv2.Data;

namespace ComponentBalanceSqlv2.Model
{
    /// <summary>
    /// Запись отчета [Заводской выпуск изделия].
    /// </summary>
    public class ReleaseReport
    {
        public long ProductCode { get; set; }
        public string FactoryNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductMark { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal BuyCost { get; set; }
        public int WorkGuildNumber { get; set; }

        public static List<ReleaseReport> GetReleases(DomainContext context, MonthYear monthYear, WorkGuild workGuild)
        {
            var releaseProducts = context.ReleaseProducts
                    .Where(i => i.Month == monthYear.Month
                                && i.Year == monthYear.Year)
                    .Include(c => c.Product) 
                    .Include(c => c.ReleaseMoves)
                    .Include(c => c.EquipmentMoves)
                    .ToList();

            if (workGuild != null)
            {
                releaseProducts = releaseProducts.Where(i => i.WorkGuildId == workGuild.Id).ToList();
            }

            var result = releaseProducts.GroupBy(p =>
                    new { p.FactoryNumber, p.Product.Code, p.Product.Name, p.Product.Designation, p.WorkGuild })
                .Select(gr => new ReleaseReport
                {
                    FactoryNumber = gr.Key.FactoryNumber,
                    ProductCode = gr.Key.Code,
                    ProductName = gr.Key.Name,
                    ProductMark = gr.Key.Designation,
                    MaterialCost = gr.Sum(v => v.TotalMaterialSum),
                    BuyCost = gr.Sum(v => v.TotalBuySum),
                    WorkGuildNumber = gr.Key.WorkGuild.WorkGuildNumber
                    
                }).ToList();

            return result;
        }
    }
}
