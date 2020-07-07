using System.Collections.Generic;
using System.Data.OleDb;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Services
{
    public class ProductService
    {
        private readonly DomainContext _context;

        public ProductService()
        {
            _context = new DomainContext();
        }
        public ProductService(DomainContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Найти изделие, если нет то добавить
        /// </summary>
        public Product FindOrAddIfNotExists(long code, string name, string designation)
        {
            var product = new Product(code, name, designation);
            var result = _context.Set<Product>().FindFirstOrAddIfNotExists(product, i => i.Code == code);
            return result;
        }

        /// <summary>
        /// Получить все продукты из старой БД
        /// </summary>
        public static IEnumerable<Product> GetAllInDbf()
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            const string query = "SELECT DISTINCT kod_mater, naim, marka FROM [prdsetmc] WHERE kod_mater >= 100000000000";

            var result = new List<Product>();
            try
            {
                using (var connection = DbControl.GetConnection(dbFolder))
                {
                    connection.TryConnectOpen();

                    // Проверки наличия установленных кодировок в DBF-файлах
                    connection.VerifyInstalledEncoding("prdsetmc");

                    using (var oleDbCommand = new OleDbCommand(query, connection))
                    {
                        using (var reader = oleDbCommand.ExecuteReader())
                        {
                            while (reader != null && reader.Read())
                            {
                                var code = (long)reader.GetDecimal(0);
                                var name = reader.GetString(1).Trim();
                                var designation = reader.GetString(2).Trim();
                                

                                result.Add(new Product(code, name, designation));
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
    }
}
