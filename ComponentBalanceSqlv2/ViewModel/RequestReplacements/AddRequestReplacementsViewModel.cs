using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel.RequestReplacements
{
    public class AddRequestReplacementsViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();
        private readonly MainViewModel _mainViewModel;

        private string _request;

        private ObservableCollection<ReplacementDetail> _replacementDetails;
        private ReplacementDetail _selectedReplacementDetail;
        private int _shortProductCode;


        private string _searchTextDetailFirst;
        private bool _isSearchFirst;
        private Detail _selectedDetailFirst;
        private string _searchTextDetailSecond;
        private bool _isSearchSecond;
        private Detail _selectedDetailSecond;
        private ObservableCollection<Detail> _filterDetailsList;
        #endregion
        #region Public Fields
        public ObservableCollection<ReplacementDetail> ReplacementDetails
        {
            get { return _replacementDetails; }
            set
            {
                _replacementDetails = value;
                OnPropertyChanged();
            }
        }
        public ReplacementDetail SelectedReplacementDetail
        {
            get { return _selectedReplacementDetail; }
            set
            {
                _selectedReplacementDetail = value;
                OnPropertyChanged();
            }
        }
        public int ShortProductCode
        {
            get { return _shortProductCode; }
            set
            {
                _shortProductCode = value;
                OnPropertyChanged();
            }
        }
        public string SearchTextDetailFirst
        {
            get { return _searchTextDetailFirst; }
            set
            {
                _searchTextDetailFirst = value;
                SearchDetail(value, true);
                OnPropertyChanged();
            }
        }
        public bool IsSearchFirst
        {
            get { return _isSearchFirst; }
            set
            {
                _isSearchFirst = value;
                OnPropertyChanged();
            }
        }
        public Detail SelectedDetailFirst
        {
            get { return _selectedDetailFirst; }
            set
            {
                _selectedDetailFirst = value;
                IsSearchFirst = false;
                OnPropertyChanged();
            }
        }
        public string SearchTextDetailSecond
        {
            get { return _searchTextDetailSecond; }
            set
            {
                _searchTextDetailSecond = value;
                SearchDetail(value, false);
                OnPropertyChanged();
            }
        }
        public bool IsSearchSecond
        {
            get { return _isSearchSecond; }
            set
            {
                _isSearchSecond = value;
                OnPropertyChanged();
            }
        }
        public Detail SelectedDetailSecond
        {
            get { return _selectedDetailSecond; }
            set
            {
                _selectedDetailSecond = value;
                IsSearchSecond = false;
                OnPropertyChanged();
            }
        }
        public List<Detail> DetailsList { get; }

        public ObservableCollection<Detail> FilterDetailsList
        {
            get { return _filterDetailsList; }
            set
            {
                _filterDetailsList = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public AddRequestReplacementsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            DetailsList = _context.Details.AsNoTracking().ToList();
            FilterDetailsList = new ObservableCollection<Detail>(DetailsList);

            ReplacementDetails = new ObservableCollection<ReplacementDetail>();
        }

        /// <summary>
        /// Поиск нужной детали.
        /// </summary>
        private void SearchDetail(string value, bool flagIsFirst)
        {
            FilterDetailsList = new ObservableCollection<Detail>();

            if (value.Trim() == string.Empty)
            {
                IsSearchFirst = false;
                IsSearchSecond = false;
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

            foreach (var item in DetailsList)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= item.Code.ToString(CultureInfo.CurrentCulture).IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || item.Name.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || item.Designation.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || item.Gost.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }
                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterDetailsList.Add(item);
                }
            }

            if (flagIsFirst)
            {
                IsSearchFirst = FilterDetailsList.Count != 0;
            }
            else
            {
                IsSearchSecond = FilterDetailsList.Count != 0;
            }
        }

        #region Commands
        private ICommand _addInListCommand;
        public ICommand AddInListCommand => _addInListCommand ?? (_addInListCommand = new RelayCommand(obj => AddInList()));
        private void AddInList()
        {
            if (!AddValidation()) { return; }

            ReplacementDetails.Add(new ReplacementDetail(ShortProductCode, SelectedDetailFirst, SelectedDetailSecond, string.Empty));
            ReloadSecond();
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(obj => Delete()));
        private void Delete()
        {
            if (SelectedReplacementDetail == null) { return; }

            ReplacementDetails.Remove(
                ReplacementDetails.First(i => 
                    i.DetailStartCode == SelectedReplacementDetail.DetailStartCode 
                    && i.DetailEndCode == SelectedReplacementDetail.DetailEndCode));
            SelectedReplacementDetail = null;
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(obj => Cancel()));
        private void Cancel()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Вы действительно хотите выйти?\n" +
                                                              "Не сохраненные данные будут утеряны.");
            if (result != MessageBoxResult.Yes) { return; }

            _mainViewModel.HotkeysText = string.Empty;

            //_mainViewModel.DisplayViewModel = new RequestsReplacementsViewModel(_mainViewModel, _workGuild, _monthYear);
            _mainViewModel.DisplayViewModel = new ReplacementDetailsViewModel();
        }

        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ??
                                       (_saveCommand =
                                           new RelayCommand(obj => Save()));

        private void Save()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              "Вы хотите сохранить запрос.\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) { return; }

            _mainViewModel.TaskInProgress = true;
            Task.Run(() =>
            {

                if (!string.IsNullOrEmpty(_request))
                {
                    _context.ReplacementDetails.RemoveRange(
                        _context.ReplacementDetails.Where(i => i.Cause == _request));
                }
                else
                {
                    _request = "Запрос " + new ReplacementDetailService(_context).GetNextNumberRequest();
                }

                foreach (var replacementDetail in ReplacementDetails)
                {
                    replacementDetail.Cause = _request;
                    _context.ReplacementDetails.Add(replacementDetail);
                }

                _context.SaveChanges();

            }).ContinueWith(task =>
            {
                _mainViewModel.TaskInProgress = false;
                BuildMessageBox.GetInformationMessageBox("Сохранение успешно произведено.");

                Reload();
            });
        }

        #endregion

        /// <summary>
        /// Проверка корректности данных.
        /// </summary>
        /// <returns> True - данныее корректны. </returns>
        private bool AddValidation()
        {
            if (ShortProductCode == 0)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Код изделия не указан");
                return false;
            }
            if (SelectedDetailFirst == null)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Заменяймая деталь не выбрана");
                return false;
            }
            if (SelectedDetailSecond == null)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Замена не выбрана");
                return false;
            }
            if (SelectedDetailFirst.Code == SelectedDetailSecond.Code)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Заменяймая деталь и замена должны отличаться");
                return false;
            }

            foreach (var replacementDetail in ReplacementDetails)
            {
                if (replacementDetail.DetailStartCode == SelectedDetailFirst.Code
                    && replacementDetail.DetailEndCode == SelectedDetailSecond.Code
                    && replacementDetail.ShortProductCode == ShortProductCode)
                {
                    BuildMessageBox.GetLogicErrorMessageBox("Вы уже добавили в список эту замену");
                    return false;
                }
            }

            var requestInBase = _context.ReplacementDetails.FirstOrDefault(i => i.ShortProductCode ==
                                                                                    ShortProductCode
                                                                                && i.DetailStartCode ==
                                                                                    SelectedDetailFirst.Code
                                                                                && i.DetailEndCode ==
                                                                                    SelectedDetailSecond.Code);
            if (requestInBase != null)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Выбранная замена уже была разрешена");
                return false;
            }
          
            return true;
        }


        private void Reload()
        {
            ReloadFirst();
            ReloadSecond();
            ReplacementDetails = new ObservableCollection<ReplacementDetail>();
        }

        /// <summary>
        /// Обнуление выбранных значений первой детали.
        /// </summary>
        private void ReloadFirst()
        {
            SelectedDetailFirst = null;
            SearchTextDetailFirst = string.Empty;
        }

        /// <summary>
        /// Обнуление выбранных значений второй детали.
        /// </summary>
        private void ReloadSecond()
        {
            SelectedDetailSecond = null;
            SearchTextDetailSecond = string.Empty;
        }
    }
}
