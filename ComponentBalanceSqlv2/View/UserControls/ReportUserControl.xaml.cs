using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ComponentBalanceSqlv2.ViewModel;
using Microsoft.Reporting.WinForms;
using Visual = ComponentBalanceSqlv2.Utils.Styles.Visual;

namespace ComponentBalanceSqlv2.View.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReportUserControl.xaml
    /// </summary>
    public partial class ReportUserControl 
    {
        private ReportDataSource _reportDataSource; // Источник данных печатаемого списка
        private IEnumerable<ReportParameter> _reportParameters; // Одиночные строковые параметры отчёта

        public ReportUserControl()
        {
            InitializeComponent();
            VisualInitializeComponent();
        }

        /// <summary>
        /// Визуальная инициализация страницы (цвета и размеры шрифтов контролов)
        /// </summary>
        public void VisualInitializeComponent()
        {
            FontSize = Visual.FontSize;

            // Заголовок страницы
            TitlePageGrid.Background = Visual.BackColor4_BlueBayoux;
            var titleLabels = TitlePageGrid.Children.OfType<Label>();
            foreach (var titleLabel in titleLabels)
            {
                titleLabel.Foreground = Visual.ForeColor2_PapayaWhip;
            }
        }

        private void UpdateReportDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ReportsViewModel)
            {
                var viewModel = e.OldValue as ReportsViewModel;
                viewModel.PropertyChanged -= ViewModelPropertyChanged;
            }
            if (DataContext is ReportsViewModel)
            {
                var viewModel = DataContext as ReportsViewModel;
                viewModel.PropertyChanged += ViewModelPropertyChanged;
                SetReportData();
            }

        }

        void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ReportName" || e.PropertyName == "ReportData")
                SetReportData();
        }

        private void SetReportData()
        {
            var viewModel = DataContext as ReportsViewModel;
            if (viewModel != null)
            {
                switch (viewModel.ReportType)
                {
                    case "ReleaseProductReport":
                        GetReleaseProductReportParameterAndDataSource(viewModel);
                        break;
                    case "ReleaseReport":
                        GetReleaseReportParameterAndDataSource(viewModel);
                        break;
                    case "Balance":
                        GetBalanceReportParameterAndDataSource(viewModel);
                        break;
                    case "NotEnoughDetailReport":
                        GetNotEnoughDetailReportParameterAndDataSource(viewModel);
                        break;
                    case "RequestsReplacements":
                        GetRequestsReplacementsReportParameterAndDataSource(viewModel);
                        break;

                }

                var reportFile = viewModel.ReportFile;
                var reportView = ReportHost.Child as ReportViewer;

                reportView.SetDisplayMode(DisplayMode.PrintLayout); // Режим предпросмотра "Разметка страницы"
                reportView.LocalReport.ReportPath = reportFile; // Путь к файлу отчёта
                reportView.ZoomMode = ZoomMode.PageWidth; // Режим масштабирования "По ширине страницы"          
                var pageSettings = reportView.GetPageSettings(); // Получить настройки страницы
                pageSettings.Landscape = viewModel.IsAlbumPageSettings; // Изменить в них ориентацию 
                reportView.SetPageSettings(pageSettings); // Установка ориентации листа (книжный / альбомный)
                reportView.Visible = true;

                reportView.LocalReport.SetParameters(_reportParameters); // Одиночные строковые параметры
                reportView.LocalReport.DataSources.Clear();
                reportView.LocalReport.DataSources.Add(_reportDataSource);
                reportView.RefreshReport();
            }
        }

        private void GetReleaseProductReportParameterAndDataSource(ReportsViewModel viewModel)
        {
            var selectedWorkGuild = viewModel.ParameterSelectedWorkGuild;
            var selectedMonthYear = viewModel.ParameterSelectedMonthYear;
            var resultReportList = viewModel.ResultReportReleaseProductReportList;
            
            var monthName = selectedMonthYear.DisplayMonth;
            var monthYear = monthName + " " + selectedMonthYear.Year;

            _reportParameters = new[]
            {
                new ReportParameter("WorkGuild", selectedWorkGuild?.WorkGuildNumber.ToString(CultureInfo.CurrentCulture) ?? "n"),
                new ReportParameter("MonthYear", monthYear)
            };

            const string dataSourceName = "ReleaseProductReport";
            _reportDataSource = new ReportDataSource(dataSourceName, resultReportList);
        }

        private void GetReleaseReportParameterAndDataSource(ReportsViewModel viewModel)
        {
            var selectedMonthYear = viewModel.ParameterSelectedMonthYear;
            var selectedWorkGuild = viewModel.ParameterSelectedWorkGuild;
            var resultReportList = viewModel.ResultReportReleaseReportList;

            var monthName = selectedMonthYear.DisplayMonth;
            var monthYear = monthName + " " + selectedMonthYear.Year;

            _reportParameters = new[]
            {
                new ReportParameter("MonthYear", monthYear),
                new ReportParameter("WorkGuild",selectedWorkGuild?.WorkGuildNumber.ToString(CultureInfo.CurrentCulture) ?? "n")
            };

            const string dataSourceName = "ReleaseReport";
            _reportDataSource = new ReportDataSource(dataSourceName, resultReportList);
        }

        private void GetBalanceReportParameterAndDataSource(ReportsViewModel viewModel)
        {
            var parametrSelectedWorkGuild = viewModel.ParameterSelectedWorkGuild;
            var parametrSelectedTypeTmc = viewModel.ParameterSelectedTmcType;
            var parametrSelectedMonthYear = viewModel.ParameterSelectedMonthYear;
            var resultReportList = viewModel.ResultReportPkomList;
            
            _reportParameters = new[]
            {
                new ReportParameter("Date", DateTime.Today.ToShortDateString()),
                new ReportParameter("TypeOfTMC", parametrSelectedTypeTmc.ShortName),
                new ReportParameter("Month", parametrSelectedMonthYear.DisplayMonth),
                new ReportParameter("Year", parametrSelectedMonthYear.Year.ToString()),
                new ReportParameter("WorkGuild", parametrSelectedWorkGuild.DisplayWorkguildString),
                new ReportParameter("NegativeMark", viewModel.ParameterNegativeMark),
                new ReportParameter("ShortMark", viewModel.ParameterShortMark)
            };

            const string dataSourceName = "Pkom";
            _reportDataSource = new ReportDataSource(dataSourceName, resultReportList);
        }

        private void GetNotEnoughDetailReportParameterAndDataSource(ReportsViewModel viewModel)
        {
            var parameterSelectedCode = viewModel.ParameterSelectedCode;
            var parameterSelectedName = viewModel.ParameterSelectedName;
            var parameterSelectedDesignation = viewModel.ParameterSelectedDesignation;
            var parameterSelectedCount = viewModel.ParameterSelectedCount;
            var resultReportList = viewModel.ResultReportNotEnoughDetailList;

            _reportParameters = new[]
            {
                new ReportParameter("Code", parameterSelectedCode.ToString(CultureInfo.CurrentCulture)),
                new ReportParameter("Name", parameterSelectedName),
                new ReportParameter("Designation", parameterSelectedDesignation),
                new ReportParameter("Count", parameterSelectedCount.ToString(CultureInfo.CurrentCulture)),
            };

            const string dataSourceName = "NotEnoughDetailReport";
            _reportDataSource = new ReportDataSource(dataSourceName, resultReportList);
        }

        private void GetRequestsReplacementsReportParameterAndDataSource(ReportsViewModel viewModel)
        {
            var selectedRequestsName = viewModel.ParameterSelectedName;
            var resultReportList = viewModel.ResultReportReplacementDetailReportList;

            _reportParameters = new[]
            {
                new ReportParameter("Request", selectedRequestsName)
            };

            const string dataSourceName = "ReplacementDetail";
            _reportDataSource = new ReportDataSource(dataSourceName, resultReportList);
        }

    }
}
