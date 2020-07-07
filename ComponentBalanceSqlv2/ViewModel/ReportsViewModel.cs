using System.Collections.Generic;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class ReportsViewModel : BaseNotificationClass
    {
        public string Heading { get; }

        /// <summary>
        /// Вид отчета
        /// </summary>
        public string ReportType { get; }

        /// <summary>
        /// Название файла отчета
        /// </summary>
        public string ReportFileName { get; }

        /// <summary>
        /// Путь к файлу отчёта
        /// </summary>
        public string ReportFile { get; }

        /// <summary>
        /// Ориентация листа альбомная (да/нет).
        /// </summary>
        public bool IsAlbumPageSettings { get; }

        // Параметры отчетов

        public WorkGuild ParameterSelectedWorkGuild { get; set; }

        public MonthYear ParameterSelectedMonthYear { get; set; }
        public TmcType ParameterSelectedTmcType { get; set; }
        public string ParameterNegativeMark { get; set; }
        public string ParameterShortMark { get; set; }
        public decimal ParameterSelectedCode { get; set; }
        public string ParameterSelectedName { get; set; }
        public string ParameterSelectedDesignation { get; set; }
        public decimal ParameterSelectedCount;

        public List<ReleaseProductReport> ResultReportReleaseProductReportList { get; }
        public ReportsViewModel(string heading, string reportType, string reportFileName, string reportFile,
            WorkGuild parameterSelectedWorkGuild, MonthYear parameterSelectedMonthYear, List<ReleaseProductReport> resultReportReleaseProductReportList)
        {
            Heading = heading;
            ReportType = reportType;
            ReportFileName = reportFileName;
            ReportFile = reportFile;
            ParameterSelectedWorkGuild = parameterSelectedWorkGuild;
            ParameterSelectedMonthYear = parameterSelectedMonthYear;
            ResultReportReleaseProductReportList = resultReportReleaseProductReportList;
            IsAlbumPageSettings = false;
        }

        public List<ReleaseReport> ResultReportReleaseReportList { get; }
        public ReportsViewModel(string heading, string reportType, string reportFileName, string reportFile,
        MonthYear parameterSelectedMonthYear, WorkGuild parameterSelectedWorkGuild, List<ReleaseReport> resultReportReleaseReportList)
        {
            Heading = heading;
            ReportType = reportType;
            ReportFileName = reportFileName;
            ReportFile = reportFile;
            ParameterSelectedMonthYear = parameterSelectedMonthYear;
            ParameterSelectedWorkGuild = parameterSelectedWorkGuild;
            ResultReportReleaseReportList = resultReportReleaseReportList;
            IsAlbumPageSettings = false;
        }

        public List<Pkom> ResultReportPkomList { get; }
        public ReportsViewModel(string heading, string reportType, string reportFileName, string reportFile,
            WorkGuild parameterSelectedWorkGuild, MonthYear parameterSelectedMonthYear, TmcType parameterSelectedTmcType,
            string parameterNegativeMark, string parameterShortMark, List<Pkom> resultReportPkomList)
        {
            Heading = heading;
            ReportType = reportType;
            ReportFileName = reportFileName;
            ReportFile = reportFile;
            ParameterSelectedWorkGuild = parameterSelectedWorkGuild;
            ParameterSelectedMonthYear = parameterSelectedMonthYear;
            ParameterSelectedTmcType = parameterSelectedTmcType;
            ParameterNegativeMark = parameterNegativeMark;
            ParameterShortMark = parameterShortMark;
            ResultReportPkomList = resultReportPkomList;
            IsAlbumPageSettings = true;
        }

        public List<NotEnoughDetailReport> ResultReportNotEnoughDetailList { get; }
        public ReportsViewModel(string heading, string reportType, string reportFileName, string reportFile, decimal parameterSelectedCode, 
            string parameterSelectedName, string parameterSelectedDesignation, decimal parameterSelectedCount,
            List<NotEnoughDetailReport> resultReportNotEnoughDetailList)
        {
            Heading = heading;
            ReportType = reportType;
            ReportFileName = reportFileName;
            ReportFile = reportFile;
            ParameterSelectedCode = parameterSelectedCode;
            ParameterSelectedName = parameterSelectedName;
            ParameterSelectedDesignation = parameterSelectedDesignation;
            ParameterSelectedCount = parameterSelectedCount;
            ResultReportNotEnoughDetailList = resultReportNotEnoughDetailList;
            IsAlbumPageSettings = true;
        }

        public List<ReplacementDetail> ResultReportReplacementDetailReportList { get; }
        public ReportsViewModel(string heading, string reportType, string reportFileName, string reportFile, string parameterSelectedName,
        List<ReplacementDetail> resultReportReplacementDetailReportList)
        {
            Heading = heading;
            ReportType = reportType;
            ReportFileName = reportFileName;
            ReportFile = reportFile;
            ParameterSelectedName = parameterSelectedName;
            ResultReportReplacementDetailReportList = resultReportReplacementDetailReportList;
            IsAlbumPageSettings = true;
        }
    }
}
