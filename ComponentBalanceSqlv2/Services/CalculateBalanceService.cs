using System;
using System.Data.Entity;
using System.Data.OleDb;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;

namespace ComponentBalanceSqlv2.Services
{
    public static class CalculateBalanceService
    {
        #region Получение вспомогательных дат разных расчетов 

        /// <summary>
        /// Получить дату последней незавершенки
        /// </summary>
        public static MonthYear GetDateNzp()
        {
            const string query = "SELECT MAX(d_nzp) FROM dat_nzp";
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolder))
                {
                    oleDbConnection.TryConnectOpen();
                    var dateNzp = new DateTime();
                    using (var oleDbCommand = new OleDbCommand(query, oleDbConnection))
                    {
                        using (var reader = oleDbCommand.ExecuteReader())
                        {
                            while (reader != null && reader.Read())
                            {
                                dateNzp = reader.GetDateTime(0);
                            }
                        }
                    }

                    return new MonthYear(dateNzp.Month, dateNzp.Year);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Получение месяца и года последнего расчета CEH (SKL)
        /// </summary>
        public static MonthYear GetSrCenAndYear()
        {
            var dbFolderFoxproSkl = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl;
            const string query = "SELECT sr_cen, god FROM [prmt]";
            var srCen = 0M;
            var year = 0M;

            try
            {
                using (var connection = DbControl.GetConnection(dbFolderFoxproSkl))
                {
                    connection.TryConnectOpen();

                    using (var oleDbCommand = new OleDbCommand(query, connection))
                    {
                        using (var reader = oleDbCommand.ExecuteReader())
                        {
                            while (reader != null && reader.Read())
                            {
                                srCen = reader.GetDecimal(0);
                                year = reader.GetDecimal(1);
                            }
                        }
                    }
                }

                return new MonthYear((int)srCen, (int)year);
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }
        #endregion

        /// <summary>
        /// Копирование записей из Pkom_mespr (прошлый месяц) в Pkom (рабочая) c обновлением скопированных записей
        /// </summary>
        public static void InsertCopyOldPkomInNewPkom(DomainContext context, bool isCheckedUnfinished, MonthYear monthYear)
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();

                    var prevMonthYear = monthYear.GetPreviousMonth();
                    var balances = context.Balances.Where(i => i.Month == prevMonthYear.Month
                                                               && i.Year == prevMonthYear.Year).AsNoTracking().ToList();
                    foreach (var balance in balances)
                    {
                        if (balance.CountEnd != 0M)
                        {
                            context.Balances.Add(new Balance(balance.WorkGuildId, balance.DetailId,
                                balance.CountEnd, balance.CostEnd, balance.CountEnd, balance.CostEnd, monthYear));
                        }
                    }

                    // TODO незавершенка
                    //if (!isCheckedUnfinished)
                    //{
                    //    // если не было незавершенки
                    //    UpdatePkomStart02(oleDbConnection);
                    //}
                    //else
                    //{
                    //    // после незавершенки
                    //    // Формирование временных бд
                    //    InsertOstVsp();
                    //    InsertPkomOstVsp(oleDbConnection);
                    //}
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Подготовка прихода
        /// </summary>
        public static void PreparingSupply(MonthYear monthYear)
        {
            try
            {
                InsertPri(monthYear.Month, monthYear.Year);
                InsertCen(monthYear.MonthToString);
                UpdatePrinCen();
                DeletePriBySchet("10/16");
                RecalculateFounding();
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Если из одного литья делается несколько деталей, посадить на приход кол-во которое можно сделать.
        /// todo не тестил
        /// </summary>
        private static void RecalculateFounding()
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            var dbFolderFox60ArmBase = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;

            var query = "UPDATE Pri " +
                        "SET pri_kol = pri_kol * j.koli, pri_cena = pri_cena / j.koli " +
                        "FROM Pri " +
                        "JOIN (SELECT DISTINCT " +
                                "nu76.kod_mater, " +
                                "naim As cto_naim, " +
                                "marka As cto_oboz, " +
                                "ceh, " +
                                "Round(Val(kol_detal), 0) as koli, " +
                                "Round(Val(kod_zagot), 0) as kod_zagot, " +
                                "nu76.ed_izm__eb " +
                            "FROM \"" + dbFolderFox60ArmBase + "nu76.dbf\", " +
                                "\"" + dbFolderFox60ArmBase + "prdsetmc.dbf\" " +
                            "WHERE Val(nu76.kod_mater) = prdsetmc.kod_mater " +
                                "AND Inlist(kod_zagot, \"19\", \"25\", \"26\", \"28\", \"29\") " +
                                "AND kol_detal > \"00001\") as j " +
                        "ON Alltrim(Pri.cto_naim) = Alltrim(j.cto_naim) " +
                            "AND Alltrim(Pri.cto_oboz) = Alltrim(j.cto_oboz)";
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();
                    using (var oleDbCommand = new OleDbCommand(query, oleDbConnection))
                    {
                        oleDbCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Pri
        /// </summary>
        private static void InsertPri(int month, int year)
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();

                    InsertPri(oleDbConnection, month, year);
                    InsertPri58(oleDbConnection, month, year);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Cen
        /// </summary>
        private static void InsertCen(string monthString)
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();

                    InsertCen(oleDbConnection, monthString);
                    InsertCen58(oleDbConnection, monthString);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Pri из Skl
        /// </summary>
        private static void InsertPri(OleDbConnection oleDbConnection, int month, int year)
        {
            var dbFolderFox60ArmBase = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            var dbFolderFoxProSkl = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl;
            var queryInsert = "INSERT INTO Pri (ceh_hra, sklad, baz_kod, " +
                                "chto, cto_naim, cto_oboz, cto_gost, ed_izm, " +
                                "ed_izm_nai, nom_ras, pri_kol, pri_cena, " +
                                "pri_summa, pr_tmc, schet) " +
                              "SELECT DISTINCT " +
                                "Substr(Str(RASHOD.kpotr, 7), 4, 2) AS ceh_hra, " +
                                "Dvij.sklad as sklad, " +
                                "Chrtran(Substr(Dvij.kodzatr, 7, 3), \" \", \"0\") + \"00000000000\" as baz_kod, " +
                                "Dvij.kmat as chto, " +
                                "Space(30) as cto_naim, " +
                                "Space(30) as cto_oboz, " +
                                "Space(20) as cto_gost, " +
                                "Dvij.kedizm as ed_izm, " +
                                "Edizm.nedizm as ed_izm_nai, " +
                                "Dvij.nom_ras as nom_ras, " +
                                "Sum(Dvij.kol) as pri_kol, " +
                                "0.00 as pri_cena, " +
                                "0.00 as pri_summa, " +
                                "Space(3) as pr_tmc, " +
                                "'' as schet " +
                              "FROM \"" + dbFolderFoxProSkl + "dvij.dbf\", " +
                                "\"" + dbFolderFoxProSkl + "rashod.dbf\", " +
                                "\"" + dbFolderFox60ArmBase + "edizm.dbf\" " +
                              "WHERE Dvij.sklad  # 38 " +
                                "AND Dvij.nom_ras # 0  " +
                                "AND Dvij.nom_ras = Rashod.nom " +
                                "AND Rashod.kopdvi = 20 " +
                                "AND month(Rashod.data) = {0} " +
                                "AND year(Rashod.data) = {1} " +
                                "AND Dvij.kedizm = Edizm.kedizm " +
                              "GROUP BY Dvij.kmat, Dvij.cena, Dvij.kodzatr, Dvij.sklad, Rashod.kpotr, " +
                                "Dvij.kedizm, Edizm.nedizm, Dvij.nom_ras";
            try
            {
                using (var oleDbCommand = new OleDbCommand(string.Format(queryInsert, month, year), oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Pri из Skl58
        /// </summary>
        private static void InsertPri58(OleDbConnection oleDbConnection, int month, int year)
        {
            var dbFolderFox60ArmBase = Properties.Settings.Default.FoxProDbFolder_Fox60_Arm_Base;
            var dbFolderFoxProSkl58 = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl58;
            var queryInsert = "INSERT INTO Pri (ceh_hra, sklad, baz_kod, " +
                                "chto, cto_naim, cto_oboz, cto_gost, ed_izm, " +
                                "ed_izm_nai, nom_ras, pri_kol, pri_cena, " +
                                "pri_summa, pr_tmc, schet) " +
                              "SELECT DISTINCT " +
                                "Substr(Str(Rashod58.kpotr, 7), 4, 2) AS ceh_hra, " +
                                "Dvij58.sklad as sklad, " +
                                "Chrtran(Substr(Dvij58.kodzatr, 7, 3), \" \", \"0\") + \"00000000000\" as baz_kod, " +
                                "Dvij58.kmat as chto, " +
                                "Space(30) as cto_naim, " +
                                "Space(30) as cto_oboz, " +
                                "Space(20) as cto_gost, " +
                                "Dvij58.kedizm as ed_izm, " +
                                "Edizm.nedizm as ed_izm_nai, " +
                                "Dvij58.nom_ras as nom_ras, " +
                                "Sum(Dvij58.kol) as pri_kol, " +
                                "0.00 as pri_cena, " +
                                "0.00 as pri_summa, " +
                                "Space(3) as Pr_tmc, " +
                                "'' as schet " +
                              "FROM \"" + dbFolderFoxProSkl58 + "dvij.dbf\" as Dvij58, " +
                                "\"" + dbFolderFoxProSkl58 + "rashod.dbf\" as Rashod58, " +
                                "\"" + dbFolderFox60ArmBase + "edizm.dbf\" " +
                              "WHERE Dvij58.nom_ras # 0 " +
                                "AND Dvij58.nom_ras = Rashod58.nom " +
                                "AND Rashod58.kopdvi = 20 " +
                                "AND month(Rashod58.data) = {0} " +
                                "AND year(Rashod58.data) = {1} " +
                                "AND Dvij58.kedizm = Edizm.kedizm " +
                              "GROUP BY Dvij58.kmat, Dvij58.cena, Dvij58.kodzatr, Dvij58.sklad, Rashod58.kpotr, " +
                                "Dvij58.kedizm, Edizm.nedizm, Dvij58.nom_ras";
            try
            {
                using (var oleDbCommand = new OleDbCommand(string.Format(queryInsert, month, year), oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Cen из Skl
        /// </summary>
        private static void InsertCen(OleDbConnection oleDbConnection, string monthString)
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl;
            var queryInsert = "INSERT INTO Cen (kmat, kmat_s, sklad, cto_naim, " +
                              "cto_oboz, cto_gost, kedizm, cenasr, schet) " +
                              "SELECT kmat, " +
                              "kmat_s, " +
                              "sklad, " +
                              "Cpconvert(866, 1251, naim) as 'cto_naim', " +
                              "Cpconvert(866, 1251, marka) as 'cto_oboz', " +
                              "Cpconvert(866, 1251, gost) as 'cto_gost', " +
                              "kedizm, " +
                              "cenasr, " +
                              "schet " +
                              "FROM \"" + dbFolder + $"Cen{monthString}.dbf\"";
            try
            {
                using (var oleDbCommand =
                    new OleDbCommand(queryInsert, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Формирование записей временной бд Cen из Skl58
        /// </summary>
        private static void InsertCen58(OleDbConnection oleDbConnection, string monthString)
        {
            var dbFolder = Properties.Settings.Default.FoxProDbFolder_FoxPro_Skl58;
            var queryInsert = "INSERT INTO Cen (kmat, kmat_s, sklad, cto_naim, " +
                              "cto_oboz, cto_gost, kedizm, cenasr, schet) " +
                              "SELECT kmat, " +
                              "kmat, " +
                              "sklad, " +
                              "Cpconvert(866, 1251, naim) as 'cto_naim', " +
                              "Cpconvert(866, 1251, marka) as 'cto_oboz', " +
                              "Cpconvert(866, 1251, gost) as 'cto_gost', " +
                              "kedizm, " +
                              "cenasr, " +
                              "'' " +
                              "FROM \"" + dbFolder + $"Cen{monthString}.dbf\"";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryInsert, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей бд Prin записями из бд Cen
        /// </summary>
        private static void UpdatePrinCen()
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();
                    UpdatePriNameAndCen(oleDbConnection);
                    UpdatePriSklad(oleDbConnection);
                    UpdatePriTmc(oleDbConnection);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        private static void UpdatePriNameAndCen(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE pri " +
                                       "SET pri.chto = iif(cen.kmat_s <> 0.0, cen.kmat_s, pri.chto), " +
                                           "pri.cto_naim = cen.cto_naim, " +
                                           "pri.cto_oboz = cen.cto_oboz, " +
                                           "pri.cto_gost = cen.cto_gost, " +
                                           "pri.pri_cena = cen.cenasr, " +
                                           "pri.pri_summa = Round(pri.pri_kol * pri_cena, 0), " +
                                           "pri.schet = cen.schet " +
                                       "FROM pri " +
                                       "INNER JOIN cen ON pri.chto = cen.kmat " +
                                        "AND pri.sklad = cen.sklad";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Удаление записей бд Prin с указанным счетом.
        /// </summary>
        private static void DeletePriBySchet(string schet)
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();
                    DeletePriBySchet(oleDbConnection, schet);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        private static void DeletePriBySchet(OleDbConnection oleDbConnection, string schet)
        {
            const string query = "DELETE FROM pri WHERE schet = ?";
            try
            {
                using (var oleDbCommand = new OleDbCommand(query, oleDbConnection))
                {
                    oleDbCommand.Parameters.AddWithValue("schet", schet);
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновить sklad = 52 в Pri где schet = "10/15"
        /// </summary>
        /// <param name="oleDbConnection"></param>
        private static void UpdatePriSklad(OleDbConnection oleDbConnection)
        {
            const string query = "UPDATE pri SET sklad = 52 WHERE schet = ?";
            try
            {
                using (var oleDbCommand = new OleDbCommand(query, oleDbConnection))
                {
                    oleDbCommand.Parameters.AddWithValue("schet", "10/15");
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        private static void UpdatePriTmc(OleDbConnection oleDbConnection)
        {
            try
            {
                UpdatePriTmc01(oleDbConnection);
                UpdatePriTmc02(oleDbConnection);
                UpdatePriTmc03(oleDbConnection);
                UpdatePriTmc04(oleDbConnection);
                UpdatePriTmc05(oleDbConnection);
                UpdatePriTmc06(oleDbConnection);
                UpdatePriTmc07(oleDbConnection);
                UpdatePriTmc08(oleDbConnection);
                UpdatePriTmc09(oleDbConnection);

            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   в с п о м о г а т е л ь н ы х  материалов
        /// </summary>
        private static void UpdatePriTmc01(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"VSP\" WHERE sklad = 54";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   в с п о м о г а т е л ь н ы х  материалов
        /// </summary>
        private static void UpdatePriTmc02(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"VSP\" " +
                                       "WHERE sklad = 53 AND (\"СВИНЕЦ\" $ cto_naim " +
                                            "OR \"ПЛОМБА\" $ cto_naim " +
                                            "OR \"ЛЕНТА\" $ cto_naim " +
                                            "OR \"ПРИПОЙ\" $ cto_naim " +
                                            "OR \"ПРОВОЛОКА\" $ cto_naim " +
                                            "OR \"ТРУБКА\" $ cto_naim " +
                                            "OR \"ВОЛЬФРАМ\" $ cto_naim " +
                                            "OR \"АНОД\" $ cto_naim)";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   в с п о м о г а т е л ь н ы х  материалов
        /// </summary>
        private static void UpdatePriTmc03(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"KOM\" " +
                                       "WHERE sklad = 54 AND \"РЕМЕНЬ\" $ cto_naim";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   п л а с т м а с с о в ы х   изделий
        /// </summary>
        private static void UpdatePriTmc04(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"PLA\" " +
                                       "WHERE upper(trim(Cto_naim)) LIKE '%ВИНИПЛАСТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ВОЙЛОК%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ОРГСТЕКЛО%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПАРОНИТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПЛАСТИКАТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ЛАКОТКАНЬ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПЛАСТИНА ГУБЧАТАЯ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПЛАСТИНА РЕЗИНОВАЯ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПЛАСТИНА ТЕХНИЧЕСКАЯ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПОЛИАМИД%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПОЛИПРОПИЛЕН%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПОЛИСТИРОЛ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПОЛИУРЕТАН%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ПОЛИЭТИЛЕН%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%РЕЗИНА%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%СОПОЛИМЕР%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%СТЕКЛОТЕКСТОЛИТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ТЕКСТОЛИТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ФЕНОПЛАСТ%' " +
                                            "OR UPPER(TRIM(Cto_naim)) LIKE '%ФТОРОПЛАСТ%' ";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   к о м п л е к т у ю щ и х 
        /// </summary>
        private static void UpdatePriTmc05(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"KOM\" " +
                                       "WHERE sklad = 51";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   к о м п л е к т у ю щ и х 
        /// </summary>
        private static void UpdatePriTmc06(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"VSP\" " +
                                       "WHERE sklad = 51 AND \"ТРУБКА\" $ cto_naim";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   л и т ь я 
        /// </summary>
        private static void UpdatePriTmc07(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"FAB\" " +
                                       "WHERE sklad = 52";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri 
        /// Установка  признака   л и т ь я 
        /// </summary>
        private static void UpdatePriTmc08(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"FAB\" " +
                                       "WHERE sklad = 51 AND \"МТЗ\" $ cto_naim";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Обновление записей Pri
        /// Установка  признака   и н с т р у м е н т а
        /// </summary>
        private static void UpdatePriTmc09(OleDbConnection oleDbConnection)
        {
            const string queryUpdate = "UPDATE Pri SET Pr_tmc = \"INS\" " +
                                       "WHERE sklad = 58";
            try
            {
                using (var oleDbCommand = new OleDbCommand(queryUpdate, oleDbConnection))
                {
                    oleDbCommand.ExecuteNonQuery();
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        public static void InsertImportWarehouseMove(DomainContext context, MonthYear workMonthYear)
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            try
            {
                using (var oleDbConnection = DbControl.GetConnection(dbFolderTempWork))
                {
                    oleDbConnection.TryConnectOpen();
                    InsertImportWarehouseMove(oleDbConnection, context, workMonthYear);
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }


        private static void InsertImportWarehouseMove(OleDbConnection oleDbConnection, DomainContext context, MonthYear monthYear)
        {
            const string query = "SELECT ceh_hra, sklad, baz_kod, " +
                                    "chto, cto_naim, cto_oboz, cto_gost, " +
                                    "ed_izm, ed_izm_nai, " +
                                    "nom_ras, pri_kol, pri_cena, " +
                                    "pri_summa, pr_tmc " +
                                 "FROM Pri";
            try
            {
                using (var oleDbCommand = new OleDbCommand(query, oleDbConnection))
                {
                    using (var reader = oleDbCommand.ExecuteReader())
                    {
                        var productService = new ProductService(context);
                        var detailService = new DetailService(context);
                        var balanceService = new BalanceService(context);

                        while (reader != null && reader.Read())
                        {
                            var workGuildNumber = reader.GetDecimal(0);
                            var warehouse = reader.GetDecimal(1);
                            var productCode = reader.GetDecimal(2);

                            var detailCode = reader.GetDecimal(3);
                            var detailName = reader.GetString(4).Trim();
                            var detailDesignation = reader.GetString(5).Trim();
                            var detailGost = reader.GetString(6).Trim();

                            var measureOldDbCode = reader.GetDecimal(7);
                            var measureShortName = reader.GetString(8).Trim();

                            // TODO ед.изм поправка
                            var ratio = 1M;
                            switch ((int)measureOldDbCode)
                            {
                                case 503: // ДЕСЯТЬ ШТ.
                                    measureOldDbCode = 501M;
                                    measureShortName = "ШТ.";
                                    ratio = 10M;
                                    break;
                                    // в будущем другие проблемные ед.изм. дописать тут
                            }

                            var count = reader.GetDecimal(10) * ratio;
                            var cost = reader.GetDecimal(11) / ratio;

                            var tmcTypeShortName = reader.GetString(13).Trim();

                            var workguild = context.WorkGuilds.First(i => i.WorkGuildNumber == workGuildNumber);
                            var product = productService.FindOrAddIfNotExists((long)productCode, "", "");
                            var detail = detailService.FindOrAddIfNotExists((long)detailCode, detailName,
                                detailDesignation, detailGost, 
                                (int)measureOldDbCode, measureShortName, tmcTypeShortName, (int)warehouse);
                            var balance = balanceService.FindOrAddIfNotExists(workguild.Id, detail.Id, monthYear);
                            balance.AddImportWarehouseMove(count, cost, product.Id);

                            // Без этого получаем:
                            // Unable to determine the principal end 
                            // of the 'ComponentBalanceSqlv2.Data.Detail_Balances' relationship.
                            // Multiple added entities may have the same primary key.
                            // Я предпологаю что тут добавляется новая деталь и в будущем она еще раз садится на приход, 
                            // но так как ее еще не сохранили он не может ее найти.
                           context.SaveChanges();
                        }
                    }
                }
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }
    }
}
