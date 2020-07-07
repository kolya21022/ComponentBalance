using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Services
{
    public class CompositionProductService
    {
        private readonly DomainContext _context;
        public CompositionProductService()
        {
            _context = new DomainContext();
        }
        public CompositionProductService(DomainContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Получить состав всех изделий из старой БД
        /// </summary>
        public static List<CompositionProduct> GetAllInDbf()
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_Composition;
            const string query = "SELECT " +
                                     "kc, " +
                                     "ki, " +
                                     "izd_naim, " +
                                     "izd_oboz, " +
                                     "chto, " +
                                     "naim, " +
                                     "marka, " +
                                     "gost, " +
                                     "sklad, " +
                                     "ed_izm, " +
                                     "nedizm, " +
                                     "pr_tmc, " +
                                     "sum(kol), " +
                                     "pr_ud " +
                                 "FROM [Ac_nrm] " +
                                 "WHERE (kc = 2 or kc = 3 or kc = 4 or kc = 5) and kol > 0 " +
                                 "GROUP BY kc, ki, izd_naim, izd_oboz, " +
                                    "chto, naim, marka, gost, " +
                                    "sklad, ed_izm, nedizm, pr_tmc, pr_ud";

            var result = new List<CompositionProduct>();
            try
            {
                using (var connection = DbControl.GetConnection(dbFolder))
                {
                    connection.TryConnectOpen();

                    // Проверки наличия установленных кодировок в DBF-файлах
                    connection.VerifyInstalledEncoding("Ac_nrm");

                    using (var oleDbCommand = new OleDbCommand(query, connection))
                    {
                        using (var reader = oleDbCommand.ExecuteReader())
                        {
                            while (reader != null && reader.Read())
                            {
                                var workGuildNumber = (int)reader.GetDecimal(0);
                                var productCode = (long)reader.GetDecimal(1);
                                var productName = reader.GetString(2).Trim();
                                var productDesignation = reader.GetString(3).Trim();
                                var detailCode = (long)reader.GetDecimal(4);
                                var detailName = reader.GetString(5).Trim();
                                var detailDesignation = reader.GetString(6).Trim();
                                var detailGost = reader.GetString(7).Trim();
                                var detailWarehouse = (int)reader.GetDecimal(8);
                                var measureOldDbCode = (int)reader.GetDecimal(9);
                                var measureName = reader.GetString(10).Trim();
                                var tmcTypeShortName = reader.GetString(11).Trim();
                                var count = reader.GetDecimal(12);
                                var prUd = reader.GetString(13).Trim();
                                const bool isCanDeleteInRelease = true; // TODO пока что по просьбе цехов удалить из выпуска можно любую деталь(даже если она обязательна)
                                //var isCanDeleteInRelease = prUd.Length != 0;
                                var isCanUsePart = prUd.Length != 0;

                                result.Add(new CompositionProduct(new WorkGuild(workGuildNumber),
                                    new Product(productCode, productName, productDesignation),
                                    new Detail(detailCode, detailName, detailDesignation, detailGost,
                                        new Measure(measureOldDbCode, measureName), new TmcType(tmcTypeShortName),
                                        detailWarehouse),
                                    count, isCanDeleteInRelease, isCanUsePart));
                            }
                        }
                    }
                }
                return result;
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Очистить состав всех изделий. 
        /// </summary>
        private void RemoveAll()
        {
            _context.CompositionProducts.RemoveRange(_context.CompositionProducts);
        }

        /// <summary>
        /// Очистить состав указанного изделия.
        /// </summary>
        private void RemoveProduct(Product product)
        {
            _context.CompositionProducts.RemoveRange(
                _context.CompositionProducts.Where(i => i.Product.Code == product.Code));
        }

        private void Insert(IEnumerable<CompositionProduct> compositionProducts)
        {
            foreach (var compositionProduct in compositionProducts)
            {
                var workGuildId =
                    _context.WorkGuilds.First(i => i.WorkGuildNumber == compositionProduct.WorkGuild.WorkGuildNumber).Id;
                var product = compositionProduct.Product;
                var productId = new ProductService().FindOrAddIfNotExists(product.Code, product.Name, product.Designation).Id;

                var detail = compositionProduct.Detail;
                var detailMeasure = detail.Measure;
                var detailTmcType = detail.TmcType;

                var detailId = new DetailService().FindOrAddIfNotExists(
                    detail.Code, detail.Name, detail.Designation, detail.Gost,
                    detailMeasure.OldDbCode, detailMeasure.ShortName,
                    detailTmcType.ShortName, detail.Warehouse).Id;

                _context.CompositionProducts.Add(new CompositionProduct
                {
                    WorkGuildId = workGuildId,
                    ProductId = productId,
                    DetailId = detailId,
                    Count = compositionProduct.Count,
                    IsCanDeleteInRelease = compositionProduct.IsCanDeleteInRelease,
                    IsCanUsePart = compositionProduct.IsCanUsePart
                });
            }
        }

        /// Быстрее чем Insert, но не надежен.
        private void Insert2(IEnumerable<CompositionProduct> compositionProducts)
        {
            var groupProducts = compositionProducts.GroupBy(p => new {p.Product.Code, p.Product.Name, p.Product.Designation});
            var productService = new ProductService(_context);
            foreach (var product in groupProducts)
            {
                productService.FindOrAddIfNotExists(product.Key.Code, product.Key.Name, product.Key.Designation);
                productService.SaveChanges();
            }

            var groupDetails = compositionProducts.GroupBy(p => new
            {
                p.Detail.Code, p.Detail.Name, p.Detail.Designation, p.Detail.Gost,
                MeasureOldDbCode = p.Detail.Measure.OldDbCode, MeasureShortName = p.Detail.Measure.ShortName,
                TmcTypeShortName = p.Detail.TmcType.ShortName, 
                p.Detail.Warehouse
            });
            var detailService = new DetailService(_context);
            foreach (var detail in groupDetails)
            {
                detailService.FindOrAddIfNotExists(
                    detail.Key.Code, detail.Key.Name, detail.Key.Designation, detail.Key.Gost,
                    detail.Key.MeasureOldDbCode, detail.Key.MeasureShortName,
                    detail.Key.TmcTypeShortName, detail.Key.Warehouse);
                detailService.SaveChanges();
            }

            foreach (var compositionProduct in compositionProducts)
            {
                _context.Database.ExecuteSqlCommand(
                    "INSERT INTO [dbo].[CompositionProducts] " +
                        "([WorkGuildId] " +
                        ",[ProductId] " +
                        ",[DetailId] " +
                        ",[Count] " +
                        ",[IsCanDeleteInRelease] " +
                        ",[IsCanUsePart]) " +
                    "VALUES " +
                    "((SELECT MIN(Id) FROM [dbo].[WorkGuilds] WHERE [WorkGuildNumber] = @p0) " +
                    ",(SELECT MIN(Id) FROM [dbo].[Products] WHERE [Code] = @p1) " +
                    ",(SELECT MIN(Details.Id) FROM [dbo].[Details] " +
                        "LEFT JOIN [dbo].[Measures] ON [dbo].[Details].[MeasureId] = [dbo].[Measures].[Id] " +
                        "WHERE [dbo].[Details].[Code] = @p2 " +
                            "AND [dbo].[Measures].[OldDbCode] = @p3 " +
                            "AND [dbo].[Details].[WareHouse] = @p4) " +
                    ",@p5 " +
                    ",@p6 " +
                    ",@p7)",
                    compositionProduct.WorkGuild.WorkGuildNumber,
                    compositionProduct.Product.Code,
                    compositionProduct.Detail.Code,
                    compositionProduct.Detail.Measure.OldDbCode,
                    compositionProduct.Detail.Warehouse,
                    compositionProduct.Count,
                    compositionProduct.IsCanDeleteInRelease,
                    compositionProduct.IsCanUsePart);
            }
        }

        /// <summary>
        /// Обновить состав указанного изделия данными из старой БД.
        /// </summary>
        public void UpdateProductFromDbf(Product product)
        {
            var compositionProducts = new List<CompositionProduct>();
            try
            {
                compositionProducts = GetAllInDbf().Where(i => i.Product.Code == product.Code).ToList();
            }
            catch (StorageException ex)
            {
                BuildMessageBox.GetCriticalErrorMessageBox(Common.ShowDetailExceptionMessage(ex));
            }

            RemoveProduct(product);
            Insert(compositionProducts);
        }

        /// <summary>
        /// Обновить состав всех изделий данными из старой БД.
        /// </summary>
        public void UpdateAllFromDbf()
        {
            var compositionProducts = new List<CompositionProduct>();
            try
            {
                compositionProducts = GetAllInDbf();
            }
            catch (StorageException ex)
            {
                BuildMessageBox.GetCriticalErrorMessageBox(Common.ShowDetailExceptionMessage(ex));
            }

            RemoveAll();
            Insert2(compositionProducts);
            //Insert(compositionProducts);
        }
    }
}
