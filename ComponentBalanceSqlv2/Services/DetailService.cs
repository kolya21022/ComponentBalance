using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Services
{
    public class DetailService
    {
        private readonly DomainContext _context;
        public DetailService()
        {
            _context = new DomainContext();
        }
        public DetailService(DomainContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Найти деталь, если нет то добавить
        /// </summary>
        public Detail FindOrAddIfNotExists(long code, string name, string designation,
            string gost, int measureOldDbCode, string measureShortName, string tmcTypeShortName, int warehouse)
        {
            var measure = new MeasureService(_context).FindOrAddIfNotExists(measureOldDbCode, measureShortName);
            var tmcType = _context.TmcTypes.First(i => i.ShortName == tmcTypeShortName);

            var detail = new Detail(code, name, designation, gost, measure, tmcType, warehouse);
            var result = _context.Set<Detail>().FindFirstOrAddIfNotExists(detail, i => i.Code == code
                                                                                      && i.TmcTypeId == tmcType.Id
                                                                                      && i.MeasureId == measure.Id
                                                                                      && i.Warehouse == warehouse);
            return result;
        }

        /// <summary>
        /// Получить все детали из старой БД
        /// </summary>
        public static IEnumerable<Detail> GetAllInDbf()
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            const string query = "SELECT DISTINCT kod_mater, naim, marka, gost FROM [prdsetmc] WHERE kod_mater < 100000000000";

            var result = new List<Detail>();
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
                                var gost = reader.GetString(3).Trim();


                                result.Add(new Detail(code, name, designation, gost));
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
