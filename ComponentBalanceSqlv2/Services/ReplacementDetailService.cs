using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.OleDb;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Services
{
    public class ReplacementDetailService
    {
        private readonly DomainContext _context;

        public ReplacementDetailService()
        {
            _context = new DomainContext();
        }
        public ReplacementDetailService(DomainContext context)
        {
            _context = context;
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Получить состав всех замен указанного месяца из старой БД
        /// </summary>
        public static List<ReplacementDetail> GetAllMonthYear(MonthYear monthYear)
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Limit;
            const string query = "SELECT DISTINCT " +
                                     "god, " +
                                     "nm, " +
                                     "ki, " +
                                     "chto, " +
                                     "naim, " +
                                     "marka, " +
                                     "gost, " +
                                     "kmat_zam, " +
                                     "naim_zam, " +
                                     "marka_zam, " +
                                     "gost_zam, " +
                                     "osnovanie2 " +
                                 "FROM [zam_spec] " +
                                 "WHERE god = ? " +
                                    "AND nm = ? " +
                                    "AND chto <> 0 " +
                                    "AND chto <> kmat_zam";

            var result = new List<ReplacementDetail>();
            try
            {
                using (var connection = DbControl.GetConnection(dbFolder))
                {
                    connection.TryConnectOpen();

                    // Проверки наличия установленных кодировок в DBF-файлах
                    connection.VerifyInstalledEncoding("zam_spec");

                    using (var oleDbCommand = new OleDbCommand(query, connection))
                    {
                        oleDbCommand.Parameters.AddWithValue("god", monthYear.Year);
                        oleDbCommand.Parameters.AddWithValue("nm", monthYear.Month);
                        using (var reader = oleDbCommand.ExecuteReader())
                        {
                            while (reader != null && reader.Read())
                            {
                                //var year = (int)reader.GetDecimal(0);
                                //var month = (int)reader.GetDecimal(1);
                                var shortProductCode = Convert.ToInt32(reader.GetString(2).Trim());
                                var detailFirstCode = (long)reader.GetDecimal(3);
                                var detailFirstName = reader.GetString(4).Trim();
                                var detailFirstDesignation = reader.GetString(5).Trim();
                                var detailFirstGost = reader.GetString(6).Trim();
                                var detailSecondCode = (long)reader.GetDecimal(7);
                                var detailSecondName = reader.GetString(8).Trim();
                                var detailSecondDesignation = reader.GetString(9).Trim();
                                var detailSecondGost = reader.GetString(10).Trim();
                                var cause = reader.GetString(11).Trim();

                                result.Add(new ReplacementDetail(shortProductCode,
                                    new Detail(detailFirstCode, detailFirstName, detailFirstDesignation, detailFirstGost),
                                    new Detail(detailSecondCode, detailSecondName, detailSecondDesignation, detailSecondGost),
                                    cause));
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

        private void Insert(ReplacementDetail replacementDetail)
        {
            var checkReplDetail = _context.ReplacementDetails.FirstOrDefault(i => i.ShortProductCode == replacementDetail.ShortProductCode
                                                                                  && i.DetailStartCode == replacementDetail.DetailStartCode
                                                                                  && i.DetailEndCode == replacementDetail.DetailEndCode);

            // Последнее офифальное разрешение что б видно было.
            if (checkReplDetail != null)
            {
                checkReplDetail.Cause = replacementDetail.Cause;
                _context.Entry(checkReplDetail).State = EntityState.Modified;
                return;
            }

            _context.ReplacementDetails.Add(new ReplacementDetail
            {
                ShortProductCode = replacementDetail.ShortProductCode,
                DetailStartCode = replacementDetail.DetailStartCode,
                DetailStartName = replacementDetail.DetailStartName,
                DetailStartDesignation = replacementDetail.DetailStartDesignation,
                DetailStartGost = replacementDetail.DetailStartGost,
                DetailEndCode = replacementDetail.DetailEndCode,
                DetailEndName = replacementDetail.DetailEndName,
                DetailEndDesignation = replacementDetail.DetailEndDesignation,
                DetailEndGost = replacementDetail.DetailEndGost,
                Cause = replacementDetail.Cause,       
            });
        }

        /// <summary>
        /// Добавить новые замены из старой БД.
        /// </summary>
        public void AddFromDbf(MonthYear monthYear)
        {
            var replacementDetails = new List<ReplacementDetail>();
            try
            {
                replacementDetails = GetAllMonthYear(monthYear);
            }
            catch (StorageException ex)
            {
                BuildMessageBox.GetCriticalErrorMessageBox(Common.ShowDetailExceptionMessage(ex));
            }

            foreach (var replacementDetail in replacementDetails)
            {
                try
                {
                    Insert(replacementDetail);
                    _context.SaveChanges();
                }
                catch (Exception)
                {
                    // ignored (уникальность)
                }
            }
        }

        public int GetNextNumberRequest()
        {
            var requests = _context.ReplacementDetails.Where(i => i.Cause.IndexOf("Запрос") >= 0).Select(i => i.Cause);

            var requestNumberMax = 0;
            foreach (var request in requests)
            {
                var requestSplit = request.Split(' ');
                var requestConvert = Convert.ToInt32(requestSplit[1]);
                if (requestConvert > requestNumberMax)
                {
                    requestNumberMax = requestConvert;
                }
            }
            return ++requestNumberMax;
        }
    }

    #region формирование стартового массива
    //public static List<ReplacementDetail> GetAllOld()
    //{
    //    var dbFolder = Properties.Settings.Default.FoxProDbFolder_Composition;
    //    const string query = "SELECT DISTINCT " +
    //                             "ki, " +
    //                             "chto, " +
    //                             "kmat_zam, " +
    //                             "naim_zam, " +
    //                             "marka_zam, " +
    //                             "gost_zam " +
    //                         "FROM [zam] " +
    //                         "WHERE chto <> 0 " +
    //                            "AND chto <> kmat_zam " +
    //                            "AND kmat_zam <> 0";

    //    var result = new List<ReplacementDetail>();
    //    try
    //    {
    //        using (var connection = DbControl.GetConnection(dbFolder))
    //        {
    //            connection.TryConnectOpen();

    //            // Проверки наличия установленных кодировок в DBF-файлах
    //            connection.VerifyInstalledEncoding("zam_spec");

    //            using (var oleDbCommand = new OleDbCommand(query, connection))
    //            {
    //                using (var reader = oleDbCommand.ExecuteReader())
    //                {
    //                    while (reader != null && reader.Read())
    //                    {
    //                        var shortProductCode = Convert.ToInt32(reader.GetString(0).Trim());
    //                        var detailFirstCode = (long)reader.GetDecimal(1);
    //                        var detailSecondCode = (long)reader.GetDecimal(2);
    //                        var detailSecondName = reader.GetString(3).Trim();
    //                        var detailSecondDesignation = reader.GetString(4).Trim();
    //                        var detailSecondGost = reader.GetString(5).Trim();

    //                        result.Add(new ReplacementDetail(shortProductCode,
    //                            new Detail(detailFirstCode, string.Empty, string.Empty, string.Empty),
    //                            new Detail(detailSecondCode, detailSecondName, detailSecondDesignation, detailSecondGost),
    //                            "Сформированная база из разрешенных замен прошлых лет."));
    //                    }
    //                }
    //            }
    //        }
    //        return result;
    //    }
    //    catch (OleDbException ex)
    //    {
    //        throw DbControl.HandleKnownDbFoxProExceptions(ex);
    //    }
    //}


    //insert into[ComponentBalanceDb].[dbo].[ReplacementDetails] (
    //[ShortProductCode]

    //,[DetailStartCode]
    //,[DetailStartName]
    //,[DetailStartDesignation]
    //,[DetailStartGost]

    //,[DetailEndCode]
    //,[DetailEndName]
    //,[DetailEndDesignation]
    //,[DetailEndGost]

    //,[Cause])
    //SELECT 105
    //,l1.[DetailStartCode]
    //,l1.[DetailStartName]
    //,l1.[DetailStartDesignation]
    //,l1.[DetailStartGost]

    //,l1.[DetailEndCode]
    //,l1.[DetailEndName]
    //,l1.[DetailEndDesignation]
    //,l1.[DetailEndGost]

    //,l1.[Cause]

    //FROM(SELECT[DetailStartCode]
    //, [DetailStartName]
    //, [DetailStartDesignation]
    //, [DetailStartGost]

    //, [DetailEndCode]
    //, [DetailEndName]
    //, [DetailEndDesignation]
    //, [DetailEndGost]

    //, [Cause] FROM[ComponentBalanceDb].[dbo].[ReplacementDetails] where ShortProductCode = 86) as l1
    //left join(select[DetailStartCode], [DetailEndCode], [Cause]
    //from [ComponentBalanceDb].[dbo].[ReplacementDetails] where ShortProductCode = 105) as l2
    //on l1.DetailStartCode = l2.DetailStartCode and l1.DetailEndCode = l2.DetailEndCode

    //where l2.Cause is null
    #endregion
}
