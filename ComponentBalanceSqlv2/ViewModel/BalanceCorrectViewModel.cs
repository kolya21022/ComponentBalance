using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class BalanceCorrectViewModel : BaseNotificationClass
    {
        #region Ctor

        #region Private Fields  
        #region Is
        private readonly bool _isClose;

        private bool _isEnabledStart;

        private bool _isVisibleWorkGuildCombobox;
        private bool _isVisibleLongUpDown;
        private bool _isVisibleNumber;
        private bool _isVisibleCount;
        private bool _isVisibleCost;

        private bool _isEnabledMove;
        #endregion
        private readonly DomainContext _context = new DomainContext();
        private ObservableCollection<Balance> _balancesCollection;     
        private readonly WorkGuild _login;
        private readonly MonthYear _monthYear;

        private TmcType _selectedTmcType;
        private MoveType _selectedMoveType;

        private Balance _selectedBalance;

        private WorkGuild _selectedWorkGuildTransfer;
        private long _valueLongUpDown;
        private string _numberLabel;
        private string _longUpDownLabel;
        private string _numberTextBox;
        private decimal _countDecimalUpDown;
        private decimal _costDecimalUpDown;

        private ICollectionView _displayBalancesByTypeTmc;
        #endregion
        #region Public Properties      
        #region Is
        public bool IsEnabledStart
        {
            get { return _isEnabledStart; }
            set
            {
                _isEnabledStart = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisibleWorkGuildCombobox
        {
            get { return _isVisibleWorkGuildCombobox; }
            set
            {
                _isVisibleWorkGuildCombobox = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisibleLongUpDown
        {
            get { return _isVisibleLongUpDown; }
            set
            {
                _isVisibleLongUpDown = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisibleNumber
        {
            get { return _isVisibleNumber; }
            set
            {
                _isVisibleNumber = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisibleCount
        {
            get { return _isVisibleCount; }
            set
            {
                _isVisibleCount = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisibleCost
        {
            get { return _isVisibleCost; }
            set
            {
                _isVisibleCost = value;
                OnPropertyChanged();
            }
        }
        public bool IsEnabledMove
        {
            get { return _isEnabledMove; }
            set
            {
                _isEnabledMove = value;
                OnPropertyChanged();
            }
        }
        #endregion
        public ObservableCollection<Balance> BalancesCollection
        {
            get { return _balancesCollection; }
            set
            {
                _balancesCollection = value;
                ReloadDisplayItems(FilterCriterias, MapFilterPredicate);
                OnPropertyChanged();
            }
        }

        public WorkGuild WorkGuildWork { get; }
        public string InfoCalculate { get; }
        public List<TmcType> TmcTypes { get; }
        public TmcType SelectedTmcType
        {
            get
            {
                return _selectedTmcType;
            }
            set
            {
                _selectedTmcType = value;
                OnPropertyChanged();

                CollectionRefresh();
            }
        }
        public Balance SelectedBalance
        {
            get { return _selectedBalance; }
            set
            {
                if (value != null)
                {
                    if (_selectedBalance != null)
                    {
                        _context.SaveChanges();
                    }

                    _context.Entry(value).Reload();
                    if (MoveTypes != null)
                    {
                        MoveRefresh();
                    }
                }
                _selectedBalance = value;
                OnPropertyChanged();

                VisibleMove();
            }
        }

        public string FilterHotKey { get; }

        public ICollectionView DisplayBalancesByTypeTmc
        {
            get { return _displayBalancesByTypeTmc; }
            set
            {
                _displayBalancesByTypeTmc = value;
                OnPropertyChanged();
            }
        }

        public List<MoveType> MoveTypes { get; }
        public MoveType SelectedMoveType
        {
            get { return _selectedMoveType; }
            set
            {
                _selectedMoveType = value;
                OnPropertyChanged();

                VisibleRefresh();
            }
        }
        public List<WorkGuild> WorkGuildsTransfer { get; }
        public WorkGuild SelectedWorkGuildTransfer
        {
            get { return _selectedWorkGuildTransfer; }
            set
            {
                _selectedWorkGuildTransfer = value;
                OnPropertyChanged();
            }
        }
        public long ValueLongUpDown
        {
            get { return _valueLongUpDown; }
            set
            {
                _valueLongUpDown = value;
                OnPropertyChanged();
            }
        }
        public string NumberLabel
        {
            get { return _numberLabel; }
            set
            {
                _numberLabel = value;
                OnPropertyChanged();
            }
        }
        public string LongUpDownLabel
        {
            get { return _longUpDownLabel; }
            set
            {
                _longUpDownLabel = value;
                OnPropertyChanged();
            }
        }
        public string NumberTextBox
        {
            get { return _numberTextBox; }
            set
            {
                _numberTextBox = value;
                OnPropertyChanged();
            }
        }
        public decimal CountDecimalUpDown
        {
            get { return _countDecimalUpDown; }
            set
            {
                _countDecimalUpDown = value;
                OnPropertyChanged();
            }
        }
        public decimal CostDecimalUpDown
        {
            get { return _costDecimalUpDown; }
            set
            {
                _costDecimalUpDown = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        private ICommand _addCommand;
        public ICommand AddCommand => _addCommand ?? (_addCommand = new RelayCommand(OnAdd, IsCanAdd));

        public void OnAdd(object parameter)
        {
            switch (SelectedMoveType.Name)
            {
                case "Брак":
                    SelectedBalance.AddDefectMove(CountDecimalUpDown, CostDecimalUpDown, NumberTextBox); 
                    break;
                case "Докомплектация":
                    var releaseProducts = _context.ReleaseProducts
                        .Where(i => i.FactoryNumber == ValueLongUpDown.ToString()

                                    && i.WorkGuildId == WorkGuildWork.Id
                                    && i.Month == _monthYear.Month
                                    && i.Year == _monthYear.Year).ToList();
                    if (!releaseProducts.Any())
                    {
                        BuildMessageBox.GetLogicErrorMessageBox("Указанный выпуск не найден.\n Добавление отменено.");
                        return;
                    }

                    var releaseProduct = releaseProducts.First();
                    //var releaseProduct = releaseProducts.FirstOrDefault() ??
                    //    new ReleaseProductService(_context).FindOrAddIfNotExists(WorkGuildWork.Id,
                    //        releaseProducts[0].ProductId, ValueLongUpDown.ToString(), _monthYear);
                    SelectedBalance.AddEquipmentMove(CountDecimalUpDown, CostDecimalUpDown, releaseProduct.Id, NumberTextBox);
                    break;
                case "На склад":
                    SelectedBalance.AddExportWarehouseMove(CountDecimalUpDown, NumberTextBox);
                    break;
                case "В другой цех":
                    var transferCost = SelectedBalance.GetTransferCost();
                    var transferBalance = new BalanceService(_context).FindOrAddIfNotExists(SelectedWorkGuildTransfer.Id, SelectedBalance.Detail.Id, _monthYear);

                    SelectedBalance.AddExportWorkGuildMove(CountDecimalUpDown, transferCost, transferBalance.Id);
                    transferBalance.AddImportWorkGuildMove(CountDecimalUpDown, transferCost, SelectedBalance.Id);

                    if (!_context.Entry(transferBalance).Reference(c => c.WorkGuild).IsLoaded)
                    {
                        _context.Entry(transferBalance).Reference(c => c.WorkGuild).Load();
                    }
                    break;
                case "Со склада":
                    var product = _context.Products.AsNoTracking().FirstOrDefault(i => i.Code == ValueLongUpDown);
                    if (product == null)
                    {
                        BuildMessageBox.GetLogicErrorMessageBox($"Продукт с кодом {ValueLongUpDown} не найден.\nДобавление отменено.");
                        return;
                    }
                    SelectedBalance.AddImportWarehouseMove(CountDecimalUpDown, CostDecimalUpDown, product.Id);
                    break;
                case "Запчасть":
                    SelectedBalance.AddSparePartMove(CountDecimalUpDown, NumberTextBox);
                    break;
            }

            _context.SaveChanges();
            _context.Entry(SelectedBalance).Reload();

            MoveRefresh();
        }
        public bool IsCanAdd(object parameter)
        {
            if (SelectedBalance == null)
            {
                return false;
            }
            if (SelectedMoveType == null)
            {
                return false;
            }
            if (CountDecimalUpDown == 0M)
            {
                return false;
            }

            return true;
        }

        private ICommand _infoMovesCommand;
        public ICommand InfoMovesCommand => _infoMovesCommand ?? (_infoMovesCommand = new RelayCommand(obj => InfoMoves()));

        public void InfoMoves()
        {
            var infoDvijWindowViewModel = new InfoMoveWindowViewModel(_context, _login, _isClose, SelectedBalance);

            var infoDvijWindow = new InfoMoveWindow()
            {
                DataContext = infoDvijWindowViewModel,
                Owner = Common.GetOwnerWindow()
            };
            infoDvijWindow.ShowDialog();
        }
        #endregion Commands   


        public BalanceCorrectViewModel(WorkGuild login, WorkGuild workGuildWork,
            MonthYear monthYear)
        {
            _login = login;
            WorkGuildWork = workGuildWork;
            _monthYear = monthYear;

            BalancesCollection = new ObservableCollection<Balance>();

            CollectionRefresh();
            // Комбобокс ТМЦ
            var tmcTypes = _context.TmcTypes.AsNoTracking().ToList();
            tmcTypes.Add(new TmcType
            {
                Name = "Все",
                ShortName = "Все"
            });
            TmcTypes = tmcTypes;
            SelectedTmcType = TmcTypes.LastOrDefault();

            // Комбобокс виды движения.
            var moveTypes = _context.MoveTypes.AsNoTracking().Where(i => i.IsView).ToList();
            if (_login.Lvl != 1)
            {
                moveTypes.Remove(moveTypes.First(i => i.Name == "Со склада"));
            }
            MoveTypes = moveTypes;
            SelectedMoveType = MoveTypes.FirstOrDefault();

            // Комбобокс переброски цехов.
            var workGuildsTransfer = _context.WorkGuilds.AsNoTracking().Where(i => i.Lvl != 1).ToList();
            workGuildsTransfer.Remove(workGuildsTransfer.First(i => i.Id == WorkGuildWork.Id));
            WorkGuildsTransfer = workGuildsTransfer;
            SelectedWorkGuildTransfer = WorkGuildsTransfer.FirstOrDefault();

            InfoCalculate = BalancesCollection.Count > 0
                ? BalancesCollection.First().InfoCalculate
                : "Расчет не произведен";

            FilterHotKey = Constants.FilterBarCoverLabel;
            DisplayedFilter = new ObservableCollection<Dictionary<string, FilterValue>>();

            if (_login.Lvl == 1)
            {
                _isClose = false;
            }
            else
            {
                var monthCloses = _context.MonthCloses.FirstOrDefault(i => i.Month == _monthYear.Month
                                                                           && i.Year == _monthYear.Year
                                                                           && i.WorkGuildId == WorkGuildWork.Id);
                _isClose = monthCloses?.IsClose ?? true;
            }

            VisibleMove();
        }

        /// <summary>
        /// Обновление коллекции.
        /// </summary>
        private void CollectionRefresh()
        {
            var balances = _context.Balances
                // Загружает сразу все связанные данные что бы не делать кучу запросов к бд во время работы
                .Include(c => c.Detail)

                .Include(c => c.DefectMoves)
                .Include(c => c.EquipmentMoves.Select(i => i.ReleaseProduct.Product))
                .Include(c => c.ExportReworkMoves.Select(i => i.ReleaseProduct.Product))
                .Include(c => c.ExportWarehouseMoves)
                .Include(c => c.ExportWorkGuildMoves)
                .Include(c => c.ImportReworkMoves.Select(i => i.ReleaseProduct.Product))
                .Include(c => c.ImportWarehouseMoves.Select(i => i.Product))
                .Include(c => c.ImportWorkGuildMoves)
                .Include(c => c.ReleaseMoves.Select(i => i.ReleaseProduct.Product))
                .Include(c => c.SparePartMoves)

                .Include(c => c.TransferExportWorkGuildMoves)
                .Include(c => c.TransferImportWorkGuildMoves)
                .Where(i => i.WorkGuildId == WorkGuildWork.Id
                            && i.Month == _monthYear.Month 
                            && i.Year == _monthYear.Year) /*.OrderBy(i => i.RowNumber)*/;

            if (SelectedTmcType != null && SelectedTmcType.Id > 0)
            {
                balances = balances.Where(i => i.Detail.TmcTypeId == SelectedTmcType.Id);  
            }

            BalancesCollection = new ObservableCollection<Balance>(balances);
            SelectedBalance = BalancesCollection.FirstOrDefault();
        }

        /// <summary>
        /// Обновление видимости.
        /// </summary>
        private void VisibleRefresh()
        {
            if (SelectedMoveType == null)
            {
                return;
            }

            switch (SelectedMoveType.Name)
            {
                case "Брак":
                    NumberLabel = "№ акта";
                    IsVisibleWorkGuildCombobox = false;
                    IsVisibleLongUpDown = false;
                    IsVisibleNumber = true;
                    IsVisibleCount = true;
                    IsVisibleCost = true;
                    break;
                case "Докомплектация":
                    NumberLabel = "№ акта";
                    LongUpDownLabel = "№ выпуска";
                    IsVisibleWorkGuildCombobox = false;
                    IsVisibleLongUpDown = true;
                    IsVisibleNumber = true;
                    IsVisibleCount = true;
                    IsVisibleCost = true;
                    break;
                case "На склад":
                    NumberLabel = "Номер";
                    IsVisibleWorkGuildCombobox = false;
                    IsVisibleLongUpDown = false;
                    IsVisibleNumber = true;
                    IsVisibleCount = true;
                    IsVisibleCost = false;
                    break;
                case "В другой цех":
                    IsVisibleWorkGuildCombobox = true;
                    IsVisibleLongUpDown = false;
                    IsVisibleNumber = false;
                    IsVisibleCount = true;
                    IsVisibleCost = false;
                    break;
                case "Со склада":
                    LongUpDownLabel = "№ изделия";
                    IsVisibleWorkGuildCombobox = false;
                    IsVisibleLongUpDown = true;
                    IsVisibleNumber = false;
                    IsVisibleCount = true;
                    IsVisibleCost = true;
                    break;
                case "Запчасть":
                    NumberLabel = "№ акта";
                    IsVisibleWorkGuildCombobox = false;
                    IsVisibleLongUpDown = false;
                    IsVisibleNumber = true;
                    IsVisibleCount = true;
                    IsVisibleCost = false;
                    break;
            }
        }

        /// <summary>
        /// Обновление видимости движения.
        /// </summary>
        private void VisibleMove()
        {
            IsEnabledStart = _login.Lvl == 1;
            IsEnabledMove = SelectedBalance != null && !_isClose;
        }

        /// <summary>
        /// Обнулить движение.
        /// </summary>
        private void MoveRefresh()
        {
            NumberTextBox = string.Empty;
            CountDecimalUpDown = 0M;
            CostDecimalUpDown = 0M;
            SelectedMoveType = MoveTypes.Last();
        }

        #region Filter 
        public FilterCriterias FilterCriterias = new FilterCriterias();
        public ObservableCollection<Dictionary<string, FilterValue>> DisplayedFilter
        {
            get { return _displayedFilter; }
            set
            {
                _displayedFilter = value;
                IsFilterVisible = !FilterCriterias.IsEmpty;
                OnPropertyChanged();
            }
        }

        public bool IsFilterVisible
        {
            get { return _isFilterVisible; }
            set
            {
                _isFilterVisible = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Dictionary<string, FilterValue>> _displayedFilter;
        private bool _isFilterVisible;

        #region Commands
        /// <summary>
        /// Нажатие кнопки [УДЛ] DataGrid фильтрации - удаление указанного фильтра
        /// </summary>
        private RelayCommand _allFilterDeleteCommand;
        public ICommand AllFilterDeleteCommand => _allFilterDeleteCommand ?? (_allFilterDeleteCommand = new RelayCommand(obj => AllFilterDelete()));

        /// <summary>
        /// Нажатие кнопки [УДЛ] DataGrid фильтрации - удаление указанного фильтра
        /// </summary>
        private RelayCommand _filterDeleteCommand;
        public ICommand FilterDeleteCommand
        {
            get { return _filterDeleteCommand ?? (_filterDeleteCommand = new RelayCommand(obj => FilterDelete(obj as string))); }
        }

        private RelayCommand _filterCommand;
        public ICommand FilterCommand => _filterCommand ?? (_filterCommand = new RelayCommand(Filter));

        #endregion Commands

        public void AllFilterDelete()
        {
            FilterCriterias.ClearAll();
            DisplayFilterAndReload();
        }

        public void FilterDelete(string deletedColumn)
        {
            FilterCriterias.RemoveCriteria(deletedColumn);
            DisplayFilterAndReload();
        }

        public void Filter(object parameter)
        {
            var grid = parameter as Grid; // Grid-обёртка полей контекстного меню

            var menuItem = grid?.Parent as MenuItem; // MenuItem контекстного меню

            var contextMenu = menuItem?.Parent as ContextMenu; // Контекстное меню DataGrid
            if (contextMenu == null)
            {
                return;
            }

            var textBlocks = grid.Children.OfType<TextBlock>().ToArray();
            var textBoxes = grid.Children.OfType<TextBox>().ToArray();
            if (textBlocks.Length != 1 || textBoxes.Length != 1)
            {
                return;
            }

            var popupFilterName = textBlocks[0]; // Контрол для надписи
            var popupFilterValue = textBoxes[0]; // Поле ввода
            var columnName = (string)popupFilterValue.Tag; // Получения столбца из свойства Tag поля ввода
            var columnDysplayedName = popupFilterName.Text; // Получение отображаемого имени столбца из надписи
            var filterValue = popupFilterValue.Text.Trim(); // Получение значения фильтра
            if (string.IsNullOrWhiteSpace(filterValue))
            {
                return;
            }

            // Добавление/обновления критерия фильтрации и скрытие контекстного меню
            FilterCriterias.UpdateCriteria(columnName, filterValue, columnDysplayedName);
            contextMenu.Visibility = Visibility.Collapsed;

            // Отображение параметров фильтра
            DisplayedFilter = new ObservableCollection<Dictionary<string, FilterValue>>();
            foreach (var displayedItem in FilterCriterias.DisplayedDictionary)
            {
                var item = new Dictionary<string, FilterValue> { [displayedItem.Key] = displayedItem.Value };
                DisplayedFilter.Add(item);

            }
            DisplayFilterAndReload();
        }

        /// <summary>
        /// Отображение выбранных параметров фильтра, перерасчет с учетом фильтра.
        /// </summary>
        public void DisplayFilterAndReload()
        {
            // Отображение параметров фильтра
            DisplayedFilter = new ObservableCollection<Dictionary<string, FilterValue>>();
            foreach (var displayedItem in FilterCriterias.DisplayedDictionary)
            {
                var item = new Dictionary<string, FilterValue> { [displayedItem.Key] = displayedItem.Value };
                DisplayedFilter.Add(item);
            }

            ReloadDisplayItems(FilterCriterias, MapFilterPredicate);
        }

        /// <summary>
        /// Перезаполнение главного DataGrid страницы с учётом фильтров
        /// </summary>
        public void ReloadDisplayItems(FilterCriterias filterCriterias, Predicate<object> predicate)
        {
            var viewSourceBalacesCollectionView = CollectionViewSource.GetDefaultView(BalancesCollection);
            viewSourceBalacesCollectionView.Filter = filterCriterias.IsEmpty ? null : predicate;
            DisplayBalancesByTypeTmc = viewSourceBalacesCollectionView;
        }

        /// <summary>
        /// Метод-предикат (булевый) текущей записи коллекции сущностей, который возвращает true или 
        /// false в зависимости от попадания в диапазон фильтра по всем полям фильтрации.
        /// </summary>
        private bool MapFilterPredicate(object rawEntity)
        {
            var balaceEntity = (Balance)rawEntity;
            if (FilterCriterias.IsEmpty)
            {
                return true;
            }
            var result = true;

            // Проверка наличия полей сущности в критериях фильтрации и содержит ли поле искомое значение фильтра
            // Если в фильтре нет поля сущности, поле считается совпадающим по критерию
            string buffer;
            var filter = FilterCriterias;
            result &= !filter.GetValue("Detail.Code", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Code.ToString(CultureInfo.CurrentCulture), buffer);
            result &= !filter.GetValue("Detail.Name", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Name, buffer);
            result &= !filter.GetValue("Detail.Designation", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Designation, buffer);
            result &= !filter.GetValue("Detail.Gost", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Gost, buffer);
            result &= !filter.GetValue("Expenditure", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Expenditure.ToString(CultureInfo.CurrentCulture), buffer);

            result &= !filter.GetValue("Detail.TmcType.ShortName", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.TmcType.ShortName, buffer);
            result &= !filter.GetValue("Detail.Measure.ShortName", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Measure.ShortName, buffer);
            result &= !filter.GetValue("Detail.Warehouse", out buffer)
                      || FilterCriterias.ContainsLine(balaceEntity.Detail.Warehouse.ToString(CultureInfo.CurrentCulture), buffer);
            return result;
        }
        #endregion
        #endregion
    }
}