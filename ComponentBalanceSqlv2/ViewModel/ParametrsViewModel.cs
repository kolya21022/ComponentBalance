using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class ParametrsViewModel : BaseNotificationClass
    {
        #region Private Fields  
        private readonly DomainContext _context = new DomainContext();
        private string _message;

        private bool _isMonthYear;
        private int _selectedMonth;
        private int _selectedYear;

        private bool _isWorkGuild;
        private bool _isWorkGuildOrAll;
        private List<WorkGuild> _workGuildWork;
        private WorkGuild _selectedWorkGuild;
        private bool _isAllWorkGuilds;

        private bool _isTmcType;
        private List<TmcType> _tmcTypes;
        private TmcType _selectedTmcType;

        private bool _isProduct;
        private List<Product> _productsList;
        private ObservableCollection<Product> _filterProductsList;
        private Product _selectedProduct;
        private string _searchTextProduct;
        private bool _isSearch;

        private bool _isPartRelease;
        private bool _isReleaseAll;
        private bool _isReleaseKom;
        private bool _isReleaseVsp;
        private PartRelease _selectedPartRelease;
        #endregion
        #region Public Properties      
        public Dictionary<int, string> MonthDictionary { get; set; }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public bool IsMonthYear
        {
            get { return _isMonthYear; }
            set
            {
                _isMonthYear = value;
                OnPropertyChanged();
            }
        }
        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }
        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
            }
        }
        public bool IsWorkGuild
        {
            get { return _isWorkGuild; }
            set
            {
                _isWorkGuild = value;
                OnPropertyChanged();
                if (value) { ReloadWorkGuild(); }
            }
        }
        public bool IsWorkGuildOrAll
        {
            get { return _isWorkGuildOrAll; }
            set
            {
                _isWorkGuildOrAll = value;
                OnPropertyChanged();
                if (value) { ReloadWorkGuildOrAll(); }
            }
        }
        public List<WorkGuild> WorkGuildWork
        {
            get { return _workGuildWork; }
            set
            {
                _workGuildWork = value;
                OnPropertyChanged();
            }
        }
        public WorkGuild SelectedWorkGuild
        {
            get { return _selectedWorkGuild; }
            set
            {
                _selectedWorkGuild = value;
                OnPropertyChanged();
            }
        }

        public bool IsAllWorkGuilds
        {
            get { return _isAllWorkGuilds; }
            set
            {
                _isAllWorkGuilds = value;
                if (!value)
                {
                    ReloadWorkGuild();
                }
                else
                {
                    SelectedWorkGuild = null;
                }
                OnPropertyChanged();
            }
        }

        public bool IsTmcType
        {
            get { return _isTmcType; }
            set
            {
                _isTmcType = value;
                OnPropertyChanged();
                if (value) { ReloadTmcType(); }
            }
        }
        public List<TmcType> TmcTypes
        {
            get { return _tmcTypes; }
            set
            {
                _tmcTypes = value;
                OnPropertyChanged();
            }
        }
        public TmcType SelectedTmcType
        {
            get { return _selectedTmcType; }
            set
            {
                _selectedTmcType = value;
                OnPropertyChanged();
            }
        }
        public bool IsProduct
        {
            get { return _isProduct; }
            set
            {
                _isProduct = value;
                OnPropertyChanged();
                if (value) { ReloadProduct(); }
            }
        }

        public string SearchTextProduct
        {
            get { return _searchTextProduct; }
            set
            {
                _searchTextProduct = value;
                SearchProduct(value);
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Флаг идет ли поиск
        /// </summary>
        public bool IsSearch
        {
            get { return _isSearch; }
            set { _isSearch = value; OnPropertyChanged(); }
        }
        public List<Product> ProductsList
        {
            get { return _productsList; }
            set
            {
                _productsList = value;
                FilterProductsList = new ObservableCollection<Product>(ProductsList);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> FilterProductsList
        {
            get { return _filterProductsList; }
            set
            {
                _filterProductsList = value;
                OnPropertyChanged();
            }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                IsSearch = false;
                OnPropertyChanged();
            }
        }

        public bool IsPartRelease
        {
            get { return _isPartRelease; }
            set
            {
                _isPartRelease = value;
                OnPropertyChanged();
                if (value)
                {
                    SelectedPartRelease = PartRelease.All;
                }
            }
        }

        public bool IsReleaseAll
        {
            get { return _isReleaseAll; }
            set
            {
                _isReleaseAll = value;
                if (value)
                {
                    SelectedPartRelease = PartRelease.All;
                }
                OnPropertyChanged();
            }
        }

        public bool IsReleaseKom
        {
            get { return _isReleaseKom; }
            set
            {
                _isReleaseKom = value;
                if (value)
                {
                    SelectedPartRelease = PartRelease.Kom;
                }
                OnPropertyChanged();
            }
        }

        public bool IsReleaseVsp
        {
            get { return _isReleaseVsp; }
            set
            {
                _isReleaseVsp = value;
                if (value)
                {
                    SelectedPartRelease = PartRelease.Vsp;
                }
                OnPropertyChanged();
            }
        }

        public PartRelease SelectedPartRelease
        {
            get { return _selectedPartRelease; }
            set
            {
                _selectedPartRelease = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public ParametrsViewModel(MonthYear monthYear)
        {
            MonthDictionary = Constants.MonthsFullNames;
            SelectedMonth = monthYear.Month;
            SelectedYear = monthYear.Year;
            IsReleaseAll = true;
        }

        private void ReloadWorkGuild()
        {
            WorkGuildWork = _context.WorkGuilds.AsNoTracking().Where(i => i.Lvl == 0).ToList();
            SelectedWorkGuild = _workGuildWork.FirstOrDefault();
        }
        private void ReloadWorkGuildOrAll()
        {
            IsAllWorkGuilds = true;
            SelectedWorkGuild = null;
        }

        private void ReloadTmcType()
        {
            TmcTypes = _context.TmcTypes.AsNoTracking().ToList();
            SelectedTmcType = TmcTypes.FirstOrDefault();
        }

        private void ReloadProduct()
        {
            ProductsList = _context.Products.AsNoTracking().ToList();
        }

        /// <summary>
        /// Поиск нужного изделия.
        /// </summary>
        private void SearchProduct(string value)
        {
            FilterProductsList = new ObservableCollection<Product>();

            if (string.IsNullOrEmpty(value.Trim()))
            {
                IsSearch = false;
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

            foreach (var product in ProductsList)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= product.Code.ToString(CultureInfo.CurrentCulture)
                                       .IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || product.Name.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || product.Designation.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }
                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterProductsList.Add(product);
                }
            }
            IsSearch = FilterProductsList.Count != 0;
        }
    }
}