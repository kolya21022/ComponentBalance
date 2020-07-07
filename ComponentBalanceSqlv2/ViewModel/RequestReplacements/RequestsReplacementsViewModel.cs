using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel.RequestReplacements
{
    public class RequestsReplacementsViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();
        private readonly MainViewModel _mainViewModel;

        private readonly WorkGuild _workGuild;
        private readonly MonthYear _monthYear;

        private HashSet<string> _request;
        private string _selectedRequest;
        private ObservableCollection<ReplacementDetail> _replacementDetails;
        #endregion
        #region Public Fields
        public HashSet<string> Request
        {
            get { return _request; }
            set
            {
                _request = value;
                OnPropertyChanged();
            }
        }

        public string SelectedRequest
        {
            get { return _selectedRequest; }
            set
            {
                _selectedRequest = value;
                OnPropertyChanged();
                ReplacementDetails = new ObservableCollection<ReplacementDetail>(_context.ReplacementDetails.Where(i => i.Cause.IndexOf(value) >= 0));
            }
        }

        public ObservableCollection<ReplacementDetail> ReplacementDetails
        {
            get { return _replacementDetails; }
            set
            {
                _replacementDetails = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public RequestsReplacementsViewModel(MainViewModel mainViewModel, WorkGuild workGuild, MonthYear monthYear)
        {
            _mainViewModel = mainViewModel;
            _workGuild = workGuild;
            _monthYear = monthYear;
            ReloadRequest(_monthYear);     
        }

        private void ReloadRequest(MonthYear monthYear)
        {
            //Request = new HashSet<string>(_context.ReplacementDetails
            //    .Where(i => !i.IsCanUse
            //                // ReSharper disable once StringIndexOfIsCultureSpecific.1
            //                && i.Cause.IndexOf("Запрос") >= 0
            //                && i.Month == monthYear.Month
            //                && i.Year == monthYear.Year)
            //    .Select(i => i.Cause));
        }

        #region Commands
        private ICommand _addCommand;
        public ICommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand(obj => Add()));
        private void Add()
        {
            _mainViewModel.HotkeysText = string.Empty;
            _mainViewModel.DisplayViewModel = new AddRequestReplacementsViewModel(_mainViewModel);
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(obj => Delete()));
        private void Delete()
        {
            if (SelectedRequest == null) { return; }
            var result = BuildMessageBox.GetConfirmMessageBox($"Вы точно хотите удалить {SelectedRequest}?");
            if (result != MessageBoxResult.Yes) return;
            _context.ReplacementDetails.RemoveRange(_context.ReplacementDetails.Where(i => i.Cause == SelectedRequest));
            _context.SaveChanges();
            ReloadRequest(_monthYear);
        }

        private ICommand _editCommand;
        public ICommand EditCommand => _editCommand ?? (_editCommand = new RelayCommand(obj => Edit()));
        private void Edit()
        {
            //if (SelectedRequest == null) { return; }
            //_mainViewModel.HotkeysText = string.Empty;
            //_mainViewModel.DisplayViewModel = new AddRequestReplacementsViewModel(_mainViewModel, _workGuild, _monthYear, SelectedRequest);
        }

        private ICommand _printCommand;
        public ICommand PrintCommand => _printCommand ?? (_printCommand = new RelayCommand(obj => Print()));
        private void Print()
        {
            if (SelectedRequest == null) { return; }

            const string heading = "Замены запроса";
            const string reportType = "RequestsReplacements";
            const string reportFileName = "RequestsReplacements.rdlc";

            var reportFile = Common.GetReportFilePath(reportFileName);

            var reportViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile, SelectedRequest, 
                _context.ReplacementDetails.Where(i => i.Cause == SelectedRequest).ToList());

            var addReleaseWindow = new ReportWindow()
            {
                DataContext = reportViewModel,
                Owner = Common.GetOwnerWindow()
            };
            addReleaseWindow.ShowDialog();
    }

        private ICommand _approveCommand;
        public ICommand ApproveCommand => _approveCommand ?? (_approveCommand = new RelayCommand(obj => Approve()));

        private void Approve()
        {
            //if (SelectedRequest == null) { return; }

            //// TODO пока что дадим разрешение цехам по их просьбе
            ////if (_mainViewModel.Login.Lvl != 1)
            ////{
            ////    BuildMessageBox.GetInformationMessageBox("У вас недостаточно прав для этого");
            ////    return;
            ////}

            //var result = BuildMessageBox.GetConfirmMessageBox("Вы уверены?");
            //if (result != MessageBoxResult.Yes) { return; }

            //var replacementDetails = _context.ReplacementDetails.Where(i => i.Cause == SelectedRequest);
            //foreach (var replacementDetail in replacementDetails)
            //{
            //    replacementDetail.IsCanUse = true;
            //}
            //_context.SaveChanges();

            //ReloadRequest(_monthYear);
        }

        #endregion
    }
}
