using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ComponentBalanceSqlv2.Data.Dbf
{
    /// <summary>
    /// Модель DBF таблицы которая используется в старых программах.
    /// В том числе для печати на матричном принтере. 
    /// </summary>
    public class Pkom
    {
        public int WorkGuild { get; set; }
        public string ShortNameTmcType { get; set; }

        public long CodeDetail { get; set; }
        public string NameDetail { get; set; }
        public string DesignationDetail { get; set; }
        public string GostDetail { get; set; }

        public int CodeMeasure { get; set; }
        public string NameMeasure { get; set; }

        ////////////////////////////////////
        public long CodeProduct { get; set; }
        public string NameProduct { get; set; }
        public string DesignationProduct { get; set; }

        public decimal CountImportWarehouse { get; set; }
        public decimal CostImportWarehouse { get; set; }

        public decimal CountRelease { get; set; }
        public decimal CostRelease { get; set; }

        public decimal CountEquipment { get; set; }
        public decimal CostEquipment { get; set; }

        public decimal CountImportRework { get; set; }
        public decimal CostImportRework { get; set; }

        public decimal CountExportRework { get; set; }
        public decimal CostExportRework { get; set; }
        ////////////////////////////////////

        public decimal CountStart { get; set; }
        public decimal CostStart { get; set; }

        public string NumberEquipment { get; set; }
        public string NumberImoprtRework { get; set; }
        public string NumberExportRework { get; set; }
        public string NumberRelease { get; set; }

        public string NumberDefect { get; set; }
        public decimal CountDefect { get; set; }
        public decimal CostDefect { get; set; }

        public string NumberExportWarehouse { get; set; }
        public decimal CountExportWarehouse { get; set; }
        public decimal CostExportWarehouse { get; set; }

        public string NumberExportWorkGuild { get; set; }
        public decimal CountExportWorkGuild { get; set; }
        public decimal CostExportWorkGuild { get; set; }

        public string NumberImportWorkGuild { get; set; }
        public decimal CountImportWorkGuild { get; set; }
        public decimal CostImportWorkGuild { get; set; }


        public decimal CountEnd { get; set; }
        public decimal CostEnd { get; set; }

        public int Warehouse { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public Pkom(int workGuildNumber, string shortNameTmcType,
            long codeDetail, string nameDetail, string designationDetail, string gostDetailDetail,
            int oldDbCodeMeasure, string shortNameMeasure, 
            decimal countStart, decimal costStart,
            string numberEquipment, string numberImportRework, string numberExportRework,
            string numberDefect, decimal countDefect, decimal costDefect,
            string numberExportWarehouse, decimal countExportWarehouse, decimal costExportWarehouse,
            string numberExportWorkGuild, decimal countExportWorkGuild, decimal costExportWorkGuild,
            string numberImportWorkGuild, decimal countImportWorkGuild, decimal costImportWorkGuild,
            decimal countEnd, decimal costEnd,
            int warehouse, int month, int year,

            string numberSparePart, decimal countSparePart, decimal costSparePart)
        {
            WorkGuild = workGuildNumber;
            ShortNameTmcType = shortNameTmcType;
            CodeDetail = codeDetail;
            NameDetail = nameDetail;
            DesignationDetail = designationDetail;
            GostDetail = gostDetailDetail;
            CodeMeasure = oldDbCodeMeasure;
            NameMeasure = shortNameMeasure;
            CountStart = countStart;
            CostStart = costStart;

            NumberEquipment = numberEquipment;
            NumberImoprtRework = numberImportRework;
            NumberExportRework = numberExportRework;

            NumberDefect = numberDefect;
            CountDefect = countDefect;
            CostDefect = costDefect;

            NumberExportWarehouse = numberExportWarehouse;
            CountExportWarehouse = countExportWarehouse;
            CostExportWarehouse = costExportWarehouse;

            NumberExportWorkGuild = numberExportWorkGuild;
            CountExportWorkGuild = countExportWorkGuild;
            CostExportWorkGuild = costExportWorkGuild;

            NumberImportWorkGuild = numberImportWorkGuild;
            CountImportWorkGuild = countImportWorkGuild;
            CostImportWorkGuild = costImportWorkGuild;

            CountEnd = countEnd;
            CostEnd = costEnd;
            Warehouse = warehouse;
            Month = month;
            Year = year;

            NumberRelease = numberSparePart;
            CountRelease = countSparePart;
            CostRelease = costSparePart;
        }

        public Pkom(int workGuildNumber, string shortNameTmcType,
            long codeDetail, string nameDetail, string designationDetail, string gostDetail,
            int oldDbCodeMeasure, string shortNameMeasure,
            long codeProduct, string nameProduct, string designationProduct,
            decimal countImportWarehouse, decimal costImportWarehouse,
            decimal countRelease, decimal costRelease,
            decimal countEquipment, decimal costEquipment,
            decimal countImportRework, decimal costImportRework,
            decimal countExportRework, decimal costExportRework,
            int warehouse, int month, int year)
        {
            WorkGuild = workGuildNumber;
            ShortNameTmcType = shortNameTmcType;
            CodeDetail = codeDetail;
            NameDetail = nameDetail;
            DesignationDetail = designationDetail;
            GostDetail = gostDetail;
            CodeMeasure = oldDbCodeMeasure;
            NameMeasure = shortNameMeasure;

            CodeProduct = codeProduct;
            NameProduct = nameProduct;
            DesignationProduct = designationProduct;

            CountImportWarehouse = countImportWarehouse;
            CostImportWarehouse = costImportWarehouse;
            CountRelease = countRelease;
            CostRelease = costRelease;
            CountEquipment = countEquipment;
            CostEquipment = costEquipment;
            CountImportRework = countImportRework;
            CostImportRework = costImportRework;
            CountExportRework = countExportRework;
            CostExportRework = costExportRework;

            Warehouse = warehouse;
            Month = month;
            Year = year;
        }

        /// <summary>
        /// Сформировать Pkom указанного месяца и года, цеха, типа ТМЦ.
        /// </summary>
        public static List<Pkom> GetPkom(DomainContext context, MonthYear monthYear, WorkGuild workGuild, TmcType tmcType)
        {
            var result = new List<Pkom>();
            var balances = context.Balances
                .Include(i => i.WorkGuild)

                .Include(i => i.DefectMoves)
                .Include(i => i.EquipmentMoves)
                .Include(c => c.EquipmentMoves.Select(i => i.ReleaseProduct.Product))
                .Include(i => i.ExportWarehouseMoves)
                .Include(i => i.ExportWorkGuildMoves)
                .Include(i => i.ImportWarehouseMoves)
                .Include(c => c.ImportWarehouseMoves.Select(i => i.Product))
                .Include(i => i.ImportWorkGuildMoves)
                .Include(i => i.ReleaseMoves)
                .Include(c => c.ReleaseMoves.Select(i => i.ReleaseProduct.Product))

                .Include(c => c.ImportReworkMoves.Select(i => i.ReleaseProduct.Product))
                .Include(c => c.ExportReworkMoves.Select(i => i.ReleaseProduct.Product))

                .Include(i => i.Detail)
                .Include(i => i.Detail.Measure)
                .Include(i => i.Detail.TmcType)
                .Where(i => i.Month == monthYear.Month
                            && i.Year == monthYear.Year)
                .AsNoTracking()
                .ToList()
                // в IQueryable нельзя передать workGuild == null и tmcType == null
                .Where(i => (workGuild == null || i.WorkGuild.WorkGuildNumber == workGuild.WorkGuildNumber)
                            && (tmcType == null || i.Detail.TmcType.ShortName == tmcType.ShortName));

            foreach (var balance in balances)
            {
                var pkomWithoutProduct = new Pkom(
                    balance.WorkGuild.WorkGuildNumber,
                    balance.Detail.TmcType.ShortName,
                    balance.Detail.Code,
                    balance.Detail.Name,
                    balance.Detail.Designation,
                    balance.Detail.Gost,
                    balance.Detail.Measure.OldDbCode,
                    balance.Detail.Measure.ShortName,
                    balance.CountStart,
                    balance.CostStart,

                    balance.GetNumbersEquipment(),
                    balance.GetNumbersImportRework(),
                    balance.GetNumbersExportRework(),

                    balance.GetNumbersDefect(),
                    balance.GetCountSelectedMoves(balance.DefectMoves),
                    balance.GetAverageCostSelectedMoves(balance.DefectMoves),

                    balance.GetNumberExportWarehouse(),
                    balance.GetCountSelectedMoves(balance.ExportWarehouseMoves),
                    balance.GetAverageCostSelectedMoves(balance.ExportWarehouseMoves),

                    balance.GetNumbersExportWorkGuild(),
                    balance.GetCountSelectedMoves(balance.ExportWorkGuildMoves),
                    balance.GetAverageCostSelectedMoves(balance.ExportWorkGuildMoves),

                    balance.GetNumbersImportWorkGuild(),
                    balance.GetCountSelectedMoves(balance.ImportWorkGuildMoves),
                    balance.GetAverageCostSelectedMoves(balance.ImportWorkGuildMoves),

                    balance.CountEnd,
                    balance.CostEnd,
                    balance.Detail.Warehouse,
                    monthYear.Month,
                    monthYear.Year,
                    
                    balance.GetNumberSparePart(),
                    balance.GetCountSelectedMoves(balance.SparePartMoves),
                    balance.GetAverageCostSelectedMoves(balance.SparePartMoves));
                result.Add(pkomWithoutProduct);

                var codeProducts = new HashSet<long>();
                foreach (var importWarehouseMove in balance.ImportWarehouseMoves)
                {
                    codeProducts.Add(importWarehouseMove.Product.Code);
                }
                foreach (var releaseMove in balance.ReleaseMoves)
                {
                    codeProducts.Add(releaseMove.ReleaseProduct.Product.Code);
                }
                foreach (var equipmentMove in balance.EquipmentMoves)
                {
                    codeProducts.Add(equipmentMove.ReleaseProduct.Product.Code);
                }
                foreach (var importReworkMove in balance.ImportReworkMoves)
                {
                    codeProducts.Add(importReworkMove.ReleaseProduct.Product.Code);
                }
                foreach (var exportReworkMove in balance.ExportReworkMoves)
                {
                    codeProducts.Add(exportReworkMove.ReleaseProduct.Product.Code);
                }

                var products = new List<Product>();
                foreach (var product in context.Products.Join(codeProducts, d1 => d1.Code, d2 => d2, (d1, d2) => d1).AsNoTracking())
                {
                    products.Add(product);
                }

                foreach (var codeProduct in codeProducts)
                {
                    var product = products.First(i => i.Code == codeProduct);
                    var pkomWithProduct = new Pkom(
                        balance.WorkGuild.WorkGuildNumber,
                        balance.Detail.TmcType.ShortName,
                        balance.Detail.Code,
                        balance.Detail.Name,
                        balance.Detail.Designation,
                        balance.Detail.Gost,
                        balance.Detail.Measure.OldDbCode,
                        balance.Detail.Measure.ShortName,

                        codeProduct,
                        product.Name,
                        product.Designation,
                        balance.GetCountImportWarehouse(codeProduct),
                        balance.GetAverageCostImportWarehouse(codeProduct),

                        balance.GetCountRelease(codeProduct),
                        balance.GetAverageCostRelease(codeProduct),

                        balance.GetCountEquipment(codeProduct),
                        balance.GetAverageCostEquipment(codeProduct),

                        balance.GetCountImportRework(codeProduct),
                        balance.GetAverageCostImportRework(codeProduct),

                        balance.GetCountExportRework(codeProduct),
                        balance.GetAverageCostExportRework(codeProduct),

                        balance.Detail.Warehouse,
                        monthYear.Month,
                        monthYear.Year);
                    result.Add(pkomWithProduct);
                }
            }
            return result;
        }

        /// <summary>
        /// Сформировать Pkom указанного месяца и года.
        /// </summary>
        public static List<Pkom> GetPkom(DomainContext context, MonthYear monthYear)
        {
            return GetPkom(context, monthYear, null, null);
        }
    }
}
