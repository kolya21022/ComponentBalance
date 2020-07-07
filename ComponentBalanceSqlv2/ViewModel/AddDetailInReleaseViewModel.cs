using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class AddDetailInReleaseViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();

        private readonly WorkGuild _workGuild;
        private readonly MonthYear _monthYear;

        private List<ReleaseProduct> _releaseProductsList;
        private List<Detail> _detailsList;
        private ObservableCollection<ReleaseProduct> _filterReleaseProductsList;
        private ObservableCollection<Detail> _filterDetailsList;
        private ReleaseProduct _selectedReleaseProduct;
        private Detail _selectedDetail;

        private string _searchTextReleaseProduct;
        private string _searchTextDetail;

        private bool _isSearchReleaseProduct;
        private bool _isSearchDetail;
        private decimal _count;

        #endregion
        #region Public Fields
        public List<ReleaseProduct> ReleaseProductsList
        {
            get { return _releaseProductsList; }
            set
            {
                _releaseProductsList = value;
                FilterReleaseProductsList = new ObservableCollection<ReleaseProduct>(ReleaseProductsList);
                OnPropertyChanged();
            }
        }
        public List<Detail> DetailsList
        {
            get { return _detailsList; }
            set
            {
                _detailsList = value;
                FilterDetailsList = new ObservableCollection<Detail>(DetailsList);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ReleaseProduct> FilterReleaseProductsList
        {
            get { return _filterReleaseProductsList; }
            set
            {
                _filterReleaseProductsList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Detail> FilterDetailsList
        {
            get { return _filterDetailsList; }
            set
            {
                _filterDetailsList = value;
                OnPropertyChanged();
            }
        }
        public ReleaseProduct SelectedReleaseProduct
        {
            get { return _selectedReleaseProduct; }
            set
            {
                _selectedReleaseProduct = value;       
                IsSearchReleaseProduct = false;
                OnPropertyChanged();
            }
        }
        public Detail SelectedDetail
        {
            get { return _selectedDetail; }
            set
            {
                _selectedDetail = value;
                IsSearchDetail = false;
                OnPropertyChanged();
            }
        }
        public string SearchTextReleaseProduct
        {
            get { return _searchTextReleaseProduct; }
            set
            {
                _searchTextReleaseProduct = value;
                SearchProduct(value);
                OnPropertyChanged();
            }
        }
        public string SearchTextDetail
        {
            get { return _searchTextDetail; }
            set
            {
                _searchTextDetail = value;
                SearchDetail(value);
                OnPropertyChanged();
            }
        }
        public bool IsSearchReleaseProduct
        {
            get { return _isSearchReleaseProduct; }
            set { _isSearchReleaseProduct = value; OnPropertyChanged(); }
        }
        public bool IsSearchDetail
        {
            get { return _isSearchDetail; }
            set { _isSearchDetail = value; OnPropertyChanged(); }
        }
        public decimal Count
        {
            get { return _count; }
            set { _count = value; OnPropertyChanged(); }
        }
        #endregion

        public AddDetailInReleaseViewModel(WorkGuild workGuild, MonthYear monthYear)
        {
            _workGuild = workGuild;
            _monthYear = monthYear;

            ReleaseProductsList = _context.ReleaseProducts
                .Include(i => i.Product)
                .Where(i => i.WorkGuildId == workGuild.Id
                            && i.Month == monthYear.Month
                            && i.Year == monthYear.Year)
                .AsNoTracking()
                .ToList();

            DetailsList = _context.Details
                .Include(i => i.Measure)
                .Include(i => i.TmcType)
                .AsNoTracking().ToList();
        }

        #region Commands
        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(obj => Save(), obj => CanSave()));

        private void Save()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Вы уверены?");
            if (result != MessageBoxResult.Yes) { return; }

            var detail = SelectedReleaseProduct.ReleaseMoves.FirstOrDefault(i => i.Balance.DetailId == SelectedDetail.Id);
            if (detail != null)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Такая деталь уже имеется в выбранном выпуске");
                return;
            }

            var balance = new BalanceService(_context).FindOrAddIfNotExists(_workGuild.Id, SelectedDetail.Id, _monthYear);  
            _context.ReleaseMoves.Add(new ReleaseMove(balance.Id, Count, balance.CostEnd, SelectedReleaseProduct.Id, _monthYear));
            _context.SaveChanges();

            ReloadSelected();
            BuildMessageBox.GetInformationMessageBox("Сохранение успешно произведено.");
        }

        private bool CanSave()
        {
            if (SelectedReleaseProduct == null) { return false; }
            if (SelectedDetail == null) { return false; }
            if (Count == 0M) { return false; }
            return true;
        }
        #endregion

        private void ReloadSelected()
        {
            SelectedReleaseProduct = null;
            SelectedDetail = null;
        }

        /// <summary>
        /// Поиск нужного изделия.
        /// </summary>
        private void SearchProduct(string value)
        {
            FilterReleaseProductsList = new ObservableCollection<ReleaseProduct>();

            if (value.Trim() == string.Empty)
            {
                IsSearchReleaseProduct = false;
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

            foreach (var releaseProduct in ReleaseProductsList)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= releaseProduct.FactoryNumber.ToString(CultureInfo.CurrentCulture).IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || releaseProduct.Product.Code.ToString(CultureInfo.CurrentCulture).IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || releaseProduct.Product.Name.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || releaseProduct.Product.Designation.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }
                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterReleaseProductsList.Add(releaseProduct);
                }
            }
            IsSearchReleaseProduct = FilterReleaseProductsList.Count != 0;
        }

        /// <summary>
        /// Поиск нужного изделия.
        /// </summary>
        private void SearchDetail(string value)
        {
            FilterDetailsList = new ObservableCollection<Detail>();

            if (value.Trim() == string.Empty)
            {
                IsSearchDetail = false;
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

            foreach (var detail in DetailsList)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= detail.Code.ToString(CultureInfo.CurrentCulture).IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || detail.Name.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || detail.Designation.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || detail.Gost.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }
                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterDetailsList.Add(detail);
                }
            }
            IsSearchDetail = FilterDetailsList.Count != 0;
        }
    }
}
