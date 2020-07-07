using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel
{
    // TODO: в AllFactoryNumbers только 1)числа 2); 3)-
    public class AddReleaseViewModel : BaseNotificationClass
    {
        #region Private Fields

        private readonly MainViewModel _mainViewModel;
        private readonly DomainContext _context = new DomainContext();

        private readonly bool _isAdd;
        private readonly WorkGuild _workGuild;
        private readonly WorkGuild _login;
        private readonly MonthYear _monthYear;
        private readonly PartRelease _partRelease;

        private readonly ReleaseProduct _releaseProduct;

        private string _allFactoryNumbers = string.Empty;
        private HashSet<int> _factoryNumbers;
        private string _searchTextProduct;
        private bool _isSearch;
        private ObservableCollection<Product> _filterProductsList;
        private Product _selectedProduct;

        private bool _isCompositionChecked = true;
        private List<Balance> _balances = new List<Balance>();
        private ObservableCollection<CompositionProduct> _сompositionProducts;
        private ICollectionView _displayCompositionProducts;
        private CompositionProduct _selectedCompositionProduct;


        private ObservableCollection<ReplacementDetail> _replacementDetails;
        private ReplacementDetail _selectedReplacementDetail;
        private ObservableCollection<ReplacementDetail> _displayReplacementDetails;

        private bool _isCanSave;
        private List<NotEnoughDetailReport> _notEnoughDetails;

        #endregion

        #region Public Fields
        public bool IsAdd => _isAdd;

        public string AllFactoryNumbers
        {
            get { return _allFactoryNumbers; }
            set
            {
                _allFactoryNumbers = value;
                OnPropertyChanged();
                FactoryNumbers = SplitFactoryNumber(value);
                _isCanSave = false;
            }
        }

        public HashSet<int> FactoryNumbers
        {
            get { return _factoryNumbers; }
            set
            {
                _factoryNumbers = value;
                OnPropertyChanged();
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
            set
            {
                _isSearch = value;
                OnPropertyChanged();
            }
        }

        public List<Product> ProductsList { get; }

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
                if (_selectedProduct == null)
                {
                    CompositionProducts = new ObservableCollection<CompositionProduct>();
                    ReplacementDetails = new ObservableCollection<ReplacementDetail>();
                    return;
                }

                CompositionProducts = new ObservableCollection<CompositionProduct>(
                    _context.CompositionProducts
                        .Where(i => i.ProductId == value.Id
                                    && i.WorkGuildId == _workGuild.Id

                                    && (((i.Detail.TmcType.ShortName != "VSP" && i.Detail.TmcType.ShortName != "PLA") &&
                                         _partRelease == PartRelease.Kom)
                                        || ((i.Detail.TmcType.ShortName == "VSP" ||
                                             i.Detail.TmcType.ShortName == "PLA") && _partRelease == PartRelease.Vsp)
                                        || _partRelease == PartRelease.All))
                        .Include(i => i.Detail)
                        .Include(i => i.Detail.Measure)
                        .Include(i => i.Detail.TmcType)
                        .AsNoTracking());

                var compositionProductsDetailCode = CompositionProducts.Select(j => j.Detail.Code).ToList();
                ReplacementDetails =
                    new ObservableCollection<ReplacementDetail>(
                        _context.ReplacementDetails
                            .Where(i => i.ShortProductCode == value.ShortCode
                                        && compositionProductsDetailCode.Contains(i.DetailStartCode))
                            .AsNoTracking());

                var detailsIdInCompositionProducts = CompositionProducts.Select(i => i.DetailId);
                var detailsEndCodeInReplacementDetails = ReplacementDetails.Select(i => i.DetailEndCode);
                _balances = new BalanceService(_context)
                    .GetBalancesByIdDetailsAndCodeDetails(_workGuild.Id, _monthYear,
                        detailsIdInCompositionProducts, detailsEndCodeInReplacementDetails).ToList();

                // Оставляем те что есть на балансе.
                var newReplacementDetails = new ObservableCollection<ReplacementDetail>();
                foreach (var replacementDetail in ReplacementDetails)
                {
                    // Нужно что бы правильно ед.измерения найти и ТМЦ, и склад.
                    var detailFirst = CompositionProducts.First(i => i.Detail.Code == replacementDetail.DetailStartCode)
                        .Detail;
                    // Находим на балансе замену с ед измерения и тмц как у стандартной детали. 
                    var balance = _balances.FirstOrDefault(i => i.Detail.Code == replacementDetail.DetailEndCode
                                                                && i.Detail.MeasureId == detailFirst.MeasureId
                                                                && i.Detail.TmcTypeId == detailFirst.TmcTypeId
                                                                && i.Detail.Warehouse == detailFirst.Warehouse);

                    if (balance != null && (balance.CountEnd > 0 
                                            || (_releaseProduct != null && balance.ReleaseMoves.FirstOrDefault(i => i.ReleaseProductId == _releaseProduct.Id) != null))) // РЕДАКТИРОВАНИЕ: если на балансе нет, то все равно попадет в список, так как какое то кол-во использовалась на замену.
                    {
                        newReplacementDetails.Add(replacementDetail);
                    }
                }

                ReplacementDetails = newReplacementDetails;

                _isCanSave = false;
                OnPropertyChanged();
            }
        }

        public bool IsCompositionChecked
        {
            get { return _isCompositionChecked; }
            set
            {
                _isCompositionChecked = value;
                if (DisplayCompositionProducts != null)
                {
                    foreach (var compositionProduct in DisplayCompositionProducts)
                    {
                        ((CompositionProduct)compositionProduct).IsSelectedInRelease = value;
                    }
                }

                OnPropertyChanged();
            }
        }

        public ObservableCollection<CompositionProduct> CompositionProducts
        {
            get { return _сompositionProducts; }
            set
            {
                _сompositionProducts = value;
                ReloadDisplayItems(FilterCriterias, MapFilterPredicate);
                OnPropertyChanged();
            }
        }

        public ICollectionView DisplayCompositionProducts
        {
            get { return _displayCompositionProducts; }
            set
            {
                _displayCompositionProducts = value;
                OnPropertyChanged();
            }
        }

        public CompositionProduct SelectedCompositionProduct
        {
            get { return _selectedCompositionProduct; }
            set
            {
                _selectedCompositionProduct = value;
                OnPropertyChanged();
                SelectedReplacementDetail = null;

                if (value != null)
                {
                    DisplayReplacementDetails = new ObservableCollection<ReplacementDetail>(
                        ReplacementDetails.Where(i => i.DetailStartCode == value.Detail.Code));
                    CountBalanceSelectedCompositionProduct = GetCountBalanceDetail(value.Detail.Code);
                    CountBalanceWithReplacementSelectedCompositionProduct = GetCountBalanceDetailWithReplacement();
                    CountEndSelectedCompositionProduct = value.Count * FactoryNumbers.Count;
                }
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

        public ReplacementDetail SelectedReplacementDetail
        {
            get { return _selectedReplacementDetail; }
            set
            {
                _selectedReplacementDetail = value;
                OnPropertyChanged();
                if (value != null)
                {
                    CountBalanceSelectedReplacementDetail = GetCountBalanceDetail(value.DetailEndCode);
                    CountBalanceWithReplacementSelectedCompositionProduct = GetCountBalanceDetailWithReplacement();
                }
                else
                {
                    CountBalanceSelectedReplacementDetail = 0M;
                }
            }
        }

        public ObservableCollection<ReplacementDetail> DisplayReplacementDetails
        {
            get { return _displayReplacementDetails; }
            set
            {
                _displayReplacementDetails = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Доп информация для цехов

        /// <summary>
        /// Кол-во на балансе выбранной детали.
        /// </summary>
        private decimal _countBalanceSelectedCompositionProduct;

        public decimal CountBalanceSelectedCompositionProduct
        {
            get { return _countBalanceSelectedCompositionProduct; }
            set
            {
                _countBalanceSelectedCompositionProduct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Кол-во на балансе + выбранные замены выбранной детали.
        /// </summary>
        private decimal _countBalanceWithReplacementSelectedCompositionProduct;

        public decimal CountBalanceWithReplacementSelectedCompositionProduct
        {
            get { return _countBalanceWithReplacementSelectedCompositionProduct; }
            set
            {
                _countBalanceWithReplacementSelectedCompositionProduct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Кол-во необходимое выбранной детали.
        /// </summary>
        private decimal _countEndSelectedCompositionProduct;

        public decimal CountEndSelectedCompositionProduct
        {
            get { return _countEndSelectedCompositionProduct; }
            set
            {
                _countEndSelectedCompositionProduct = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Кол-во на балансе выбранной замены.
        /// </summary>
        private decimal _countBalanceSelectedReplacementDetail;

        public decimal CountBalanceSelectedReplacementDetail
        {
            get { return _countBalanceSelectedReplacementDetail; }
            set
            {
                _countBalanceSelectedReplacementDetail = value;
                OnPropertyChanged();
            }
        }

        private decimal GetCountBalanceDetailWithReplacement()
        {
            var count = CountBalanceSelectedCompositionProduct;
            foreach (var displayReplacementDetail in DisplayReplacementDetails)
            {
                count += displayReplacementDetail.Count;
            }

            return count;
        }

        private decimal GetCountBalanceDetail(long detailCode)
        {
            decimal count = 0;
            var balance = _balances.FirstOrDefault(i => i.Detail.Code == detailCode);
            if (balance != null)
            {
                count = balance.CountEnd;
            }

            return count;
        }

        #endregion

        public AddReleaseViewModel(MainViewModel mainViewModel, WorkGuild login, WorkGuild workGuild,
            MonthYear monthYear, PartRelease partRelease)
        {
            _isAdd = true;

            _mainViewModel = mainViewModel;
            _login = login;
            _workGuild = workGuild;
            _monthYear = monthYear;
            _partRelease = partRelease;

            FactoryNumbers = new HashSet<int>();

            ProductsList = _context.Products.AsNoTracking().ToList();
            FilterProductsList = new ObservableCollection<Product>(ProductsList);
        }

        public AddReleaseViewModel(MainViewModel mainViewModel, WorkGuild login, WorkGuild workGuild,
            MonthYear monthYear, ReleaseProduct releaseProduct)
        {
            _isAdd = false;

            _mainViewModel = mainViewModel;
            _login = login;
            _workGuild = workGuild;
            _monthYear = monthYear;
            _partRelease = PartRelease.All;

            AllFactoryNumbers = releaseProduct.FactoryNumber;

            _releaseProduct = releaseProduct;
            SelectedProduct = releaseProduct.Product;

            // заполнение выбранных деталей документации
            IsCompositionChecked = false;
            //PS. releasemove.balanceId -> деталь которую спесали, значит если этот balanceId есть среди ReleaseReplacementDetail.BalanceId то это была замена
            foreach (var releaseMoveWithoutReplacement in releaseProduct.ReleaseMoves.Where(i => !releaseProduct.ReleaseReplacementDetails.Select(j => j.BalanceId).Contains(i.BalanceId)))
            {
                var compositionProduct = CompositionProducts.FirstOrDefault(i =>
                    i.Detail.Code == releaseMoveWithoutReplacement.Balance.Detail.Code);
                if (compositionProduct == null)
                {
                    // todo ошибка, изменилась документация, деталь не найдена в документации
                    CloseCommand.Execute(true);
                }

                compositionProduct.IsSelectedInRelease = true;
            }

            // заполнение введенных замен.
            foreach (var replacement in releaseProduct.ReleaseReplacementDetails)
            {
                var compositionProduct = CompositionProducts.FirstOrDefault(i =>
                    i.DetailId == replacement.DetailId);

                if (compositionProduct == null)
                {
                    // todo ошибка, изменилась документация, деталь не найдена в документации
                    CloseCommand.Execute(true);
                }
                compositionProduct.IsSelectedInRelease = true; // заполнение выбранных деталей документации

                var replacementDetail = ReplacementDetails.FirstOrDefault(i =>
                    i.DetailStartCode == replacement.Detail.Code
                    && i.DetailEndCode == replacement.Balance.Detail.Code);
                if (replacementDetail != null)
                {
                    replacementDetail.Count = replacement.Count;
                }
            }
        }

        #region Command
        #region Кнопка: [MAX]
        private RelayCommand _maxSelectedReplacementCommand;

        public ICommand MaxSelectedReplacementCommand => _maxSelectedReplacementCommand ??
                                                         (_maxSelectedReplacementCommand =
                                                             new RelayCommand(obj => Max()));

        private void Max()
        {
            if (SelectedReplacementDetail == null)
            {
                return;
            }

            if (SelectedReplacementDetail.Count != 0M)
            {
                SelectedReplacementDetail.Count = 0M;
                return;
            }

            var count = CountEndSelectedCompositionProduct - CountBalanceWithReplacementSelectedCompositionProduct;
            if (count <= 0)
            {
                return;
            }

            SelectedReplacementDetail.Count = CountBalanceSelectedReplacementDetail > count
                ? count
                : CountBalanceSelectedReplacementDetail;
        }

        #endregion

        #region Кнопка: [Проверка]

        private RelayCommand _checkCommand;
        public ICommand CheckCommand => _checkCommand ?? (_checkCommand = new RelayCommand(obj => Check()));

        private void Check()
        {
            if (_isAdd)
            {
                if (!FactoryNumbers.Any())
                {
                    BuildMessageBox.GetLogicErrorMessageBox("Кол-во не может равнятся 0");
                    return;
                }

                var releaseProducts = _context.ReleaseProducts
                    .Include(i => i.Product)
                    .Include(i => i.ReleaseMoves.Select(j => j.Balance.Detail.TmcType))
                    .Where(i => FactoryNumbers.Select(str => str.ToString()).Contains(i.FactoryNumber)
                                && i.WorkGuildId == _workGuild.Id
                                && i.Month == _monthYear.Month
                                && i.Year == _monthYear.Year).ToList();

                foreach (var releaseProduct in releaseProducts)
                {
                    if (releaseProduct.ProductId != SelectedProduct.Id)
                    {
                        BuildMessageBox.GetLogicErrorMessageBox(
                            $"Вы уже ввели ранее изделие {releaseProduct.Product.Designation} с заводским номером {releaseProduct.FactoryNumber}!\n" +
                            "Разные изделия не могут иметь одинаковые заводские номера");
                        return;
                    }

                    switch (_partRelease)
                    {
                        case PartRelease.All:
                            if (releaseProduct.IsHaveReleaseKom)
                            {
                                BuildMessageBox.GetLogicErrorMessageBox(
                                    $"Для изделия с заводским номером {releaseProduct.FactoryNumber} уже введена комплектация!\n");
                                return;
                            }

                            if (releaseProduct.IsHaveReleaseVsp)
                            {
                                BuildMessageBox.GetLogicErrorMessageBox(
                                    $"Для изделия с заводским номером {releaseProduct.FactoryNumber} уже введены вспомагательные!\n");
                                return;
                            }

                            break;
                        case PartRelease.Kom:
                            if (releaseProduct.IsHaveReleaseKom)
                            {
                                BuildMessageBox.GetLogicErrorMessageBox(
                                    $"Для изделия с заводским номером {releaseProduct.FactoryNumber} уже введена комплектация!\n");
                                return;
                            }

                            break;
                        case PartRelease.Vsp:
                            if (releaseProduct.IsHaveReleaseVsp)
                            {
                                BuildMessageBox.GetLogicErrorMessageBox(
                                    $"Для изделия с заводским номером {releaseProduct.FactoryNumber} уже введены вспомагательные!\n");
                                return;
                            }

                            break;
                    }
                }
            }

            _mainViewModel.TaskInProgress = true;
            Task.Run(() => DoCheck()).ContinueWith(task =>
            {
                _mainViewModel.TaskInProgress = false;
            });

            while (_mainViewModel.TaskInProgress)
            {
                // костыль.
            }

            if (_isCanSave)
            {
                BuildMessageBox.GetInformationMessageBox("Вы можете провести выпуск.");
                return;
            }

            {
                var result = BuildMessageBox.GetConfirmMessageBox("Вы не можете провести выпуск.\n" +
                                                                  "Вам не хватает деталей.\n " +
                                                                  "Хотите распечатать их?");
                if (result == MessageBoxResult.Yes)
                {
                    const string heading = "Недостающие детали в изделии";
                    const string reportType = "NotEnoughDetailReport";
                    const string reportFileName = "NotEnoughDetail.rdlc";

                    var reportFile = Common.GetReportFilePath(reportFileName);

                    var reportViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                        SelectedProduct.Code, SelectedProduct.Name, SelectedProduct.Designation, FactoryNumbers.Count,
                        _notEnoughDetails);

                    var addReleaseWindow = new ReportWindow()
                    {
                        DataContext = reportViewModel,
                        Owner = Common.GetOwnerWindow()
                    };
                    addReleaseWindow.ShowDialog();
                }
            }
            {
                var result = BuildMessageBox.GetConfirmMessageBox("Снять галочки с красных позиций?");
                if (result == MessageBoxResult.Yes)
                {
                    foreach (var compositionProduct in CompositionProducts.Where(i => i.Enough == Enough.No))
                    {
                        compositionProduct.IsSelectedInRelease = false;
                    }
                }
            }
        }

        private void DoCheck()
        {
            _notEnoughDetails = new List<NotEnoughDetailReport>();

            ResetEnough(CompositionProducts);
            ResetEnough(ReplacementDetails);

            var isCanRelease = true;
            var builder = new StringBuilder();

            foreach (var compositionProduct in CompositionProducts.Where(i => i.IsSelectedInRelease))
            {
                var count = compositionProduct.Count * FactoryNumbers.Count;
                // Проверяем каждую деталь на наличие замены и уменьшаем нужное для снятия кол-во, если есть замена
                foreach (var replacementDetail in ReplacementDetails.Where(j => j.Count > 0M))
                {
                    if (compositionProduct.Detail.Code != replacementDetail.DetailStartCode)
                    {
                        continue;
                    }

                    count -= replacementDetail.Count;
                    if (count < 0)
                    {
                        isCanRelease = false;
                        builder.Append($"Внимание! Кол-во замены детали {compositionProduct.Detail.Code} " +
                                       "было указано больше чем надо.\n");
                        break;
                    }      
                }

                if (count == 0M)
                {
                    compositionProduct.Enough = Enough.Yes;
                    continue;
                }

                var balance = _balances.FirstOrDefault(i => i.DetailId == compositionProduct.DetailId);
                if (balance != null)
                {
                    var balanceCount = balance.CountEnd;

                    if (!_isAdd) // РЕДАКТИРОВАНИЕ: нужно в баланс включить те что уже были списаны при выпуске
                    {
                        var release = _releaseProduct.ReleaseMoves.FirstOrDefault(i => i.BalanceId == balance.Id);
                        if (release != null)
                        {
                            balanceCount += release.Count;
                        }
                    }

                    if (balanceCount >= count)
                    {
                        compositionProduct.Enough = Enough.Yes;
                        continue;
                    }

                    if (balanceCount > 0 && compositionProduct.IsCanUsePart)
                    {
                        compositionProduct.Enough = Enough.Optional;
                        continue;
                    }
                }

                isCanRelease = false;
                compositionProduct.Enough = Enough.No;

                _notEnoughDetails.Add(new NotEnoughDetailReport("Документация", SelectedProduct, FactoryNumbers.Count,
                    compositionProduct.Detail, compositionProduct.Count, balance?.CountEnd ?? 0M));
            }

            // Теперь сами замены проверяем что бы были
            foreach (var replacementDetail in ReplacementDetails.Where(j => j.Count > 0M))
            {
                // Нужно что бы правильно ед.измерения найти и ТМЦ, и склад.
                var detailFirst = CompositionProducts.First(i => i.Detail.Code == replacementDetail.DetailStartCode)
                    .Detail;
                // Находим на балансе замену с ед измерения и тмц как у стандартной детали. 
                var balance = _balances.FirstOrDefault(i => i.Detail.Code == replacementDetail.DetailEndCode
                                                            && i.Detail.MeasureId == detailFirst.MeasureId
                                                            && i.Detail.TmcTypeId == detailFirst.TmcTypeId
                                                            && i.Detail.Warehouse == detailFirst.Warehouse);

                if (balance != null)
                {
                    var balanceCount = balance.CountEnd;

                    if (!_isAdd) // РЕДАКТИРОВАНИЕ: нужно в баланс включить те что уже были списаны при выпуске
                    {
                        var release = _releaseProduct.ReleaseMoves.FirstOrDefault(i => i.BalanceId == balance.Id);
                        if (release != null)
                        {
                            balanceCount += release.Count;
                        }
                    }

                    if (balanceCount >= replacementDetail.Count)
                    {
                        replacementDetail.Enough = Enough.Yes;
                        continue;
                    }
                }

                // Если не нашли или кол-во меньше указанного.
                isCanRelease = false;
                replacementDetail.Enough = Enough.No;

                _notEnoughDetails.Add(new NotEnoughDetailReport("Указанная замена", SelectedProduct,
                    FactoryNumbers.Count,
                    replacementDetail, detailFirst.Measure.ShortName, balance?.CountEnd ?? 0M));
            }

            if (isCanRelease)
            {
                _isCanSave = true;
                return;
            }

            _isCanSave = false;
            if (builder.Length > 0)
            {
                BuildMessageBox.GetLogicErrorMessageBox(builder.ToString());
            }
        }

        #endregion

        #region Кнопка: [Сохранить]

        private RelayCommand _saveCommand;

        public ICommand SaveCommand =>
            _saveCommand ?? (_saveCommand = new RelayCommand(obj => Save(), obj => _isCanSave));

        private void Save()
        {
            // TODO переформулировать сообщение
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              $"Вы хотите выпустить {AllFactoryNumbers}\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) return;

            _mainViewModel.TaskInProgress = true;
            Task.Run(() =>
            {
                if (!_isAdd)
                {
                    DoDeleteReleaseMove();
                }

                DoSave();
            }).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
        }

        private void DoDeleteReleaseMove()
        {
            _releaseProduct.DeleteReleaseKom(_context);
            _releaseProduct.DeleteReleaseVsp(_context);
        }

        private void DoSave()
        {
            _isCanSave = false;
            foreach (var factoryNumber in FactoryNumbers)
            {
                var releaseProductId = new ReleaseProductService(_context).FindOrAddIfNotExists(_workGuild.Id,
                    SelectedProduct.Id, factoryNumber.ToString(), _monthYear).Id;
                foreach (var compositionProduct in CompositionProducts.Where(i => i.IsSelectedInRelease))
                {
                    var count = compositionProduct.Count;
                    // Ищем замену
                    foreach (var replacementDetail in ReplacementDetails.Where(j => j.Count > 0M))
                    {
                        if (compositionProduct.Detail.Code == replacementDetail.DetailStartCode)
                        {
                            if (replacementDetail.Count >= count)
                            {
                                replacementDetail.Count -= count;
                                AddReplacementDetail(releaseProductId, replacementDetail.DetailStartCode,
                                    replacementDetail.DetailEndCode, count,
                                    compositionProduct.DetailId);
                                count = 0M;
                                break;
                            }
                            else
                            {
                                if (replacementDetail.Count == 0) break;
                                AddReplacementDetail(releaseProductId, replacementDetail.DetailStartCode,
                                    replacementDetail.DetailEndCode, replacementDetail.Count,
                                    compositionProduct.DetailId);
                                count -= replacementDetail.Count;
                                replacementDetail.Count = 0M;
                            }
                        }
                    }

                    if (count != 0M)
                    {
                        var balance = _context.Balances.FirstOrDefault(i => i.DetailId == compositionProduct.DetailId
                                                                            && i.WorkGuildId == _workGuild.Id
                                                                            && i.Month == _monthYear.Month
                                                                            && i.Year == _monthYear.Year);
                        // Условие было добавлено для необязательных деталей по просьбе сверху.
                        if (balance != null && balance.CountEnd > 0M)
                        {
                            balance.AddReleaseMove(
                                count <= balance.CountEnd
                                    ? count
                                    : balance.CountEnd, // Условие было добавлено для необязательных деталей по просьбе сверху.
                                releaseProductId);
                        }
                    }
                }

                _context.SaveChanges();
            }

            BuildMessageBox.GetInformationMessageBox("Выпуск успешно добавлен.\n");

            _mainViewModel.HotkeysText = string.Empty;
            _mainViewModel.DisplayViewModel =
                new ReleaseViewModel(_mainViewModel, _mainViewModel.Login, _workGuild, _monthYear);
        }

        private void AddReplacementDetail(long releaseProductId, long replacementDetailStartCode,
            long replacementDetailEndCode, decimal count,
            long detailStartId)
        {
            // Нужно что бы правильно ед.измерения найти и ТМЦ, и склад.
            var detailFirst = CompositionProducts.First(i => i.Detail.Code == replacementDetailStartCode).Detail;
            // Находим на балансе замену с ед измерения и тмц как у стандартной детали. 
            var balance = _context.Balances.First(i => i.Detail.Code == replacementDetailEndCode
                                                       && i.Detail.MeasureId == detailFirst.MeasureId
                                                       && i.Detail.TmcTypeId == detailFirst.TmcTypeId
                                                       && i.Detail.Warehouse == detailFirst.Warehouse

                                                       && i.Month == _monthYear.Month
                                                       && i.Year == _monthYear.Year
                                                       && i.WorkGuildId == _workGuild.Id);
            balance.AddReleaseMove(count, releaseProductId);

            // Добавление записи в справочник замен.
            _context.ReleaseReplacementDetails.Add(new ReleaseReplacementDetail(releaseProductId, detailStartId,
                balance.Id, count));
        }

        #endregion

        #region Кнопка: [Отмена]

        private RelayCommand _closeCommand;

        public ICommand CloseCommand =>
            _closeCommand ?? (_closeCommand = new RelayCommand(obj => OpenReleaseUserConrol()));

        private void OpenReleaseUserConrol()
        {
            _mainViewModel.TaskInProgress = true;
            Task.Run(() =>
            {
                var releaseViewModel = new ReleaseViewModel(_mainViewModel, _login, _workGuild, _monthYear);
                _mainViewModel.DisplayViewModel = releaseViewModel;
            }).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
        }

        #endregion

        #endregion

        /// <summary>
        /// Получить из строки AllFactoryNumbers(заданного формата) все входящие значения FactoryNumber.
        /// </summary>
        private HashSet<int> SplitFactoryNumber(string allFactoryNumbers)
        {
            var factoryNumbers = new HashSet<int>();

            var groupsFactoryNumbers = allFactoryNumbers.Trim().Split(';');
            foreach (var groupFactoryNumber in groupsFactoryNumbers)
            {
                if (groupFactoryNumber.Length == 0) continue;

                var startAndEndFactoryNumbers = groupFactoryNumber.Trim().Split('-');
                if (startAndEndFactoryNumbers.Count() < 2)
                {
                    factoryNumbers.Add(Convert.ToInt32(startAndEndFactoryNumbers[0]));
                }
                else
                {
                    for (var i = Convert.ToInt32(startAndEndFactoryNumbers[0]);
                        i <= Convert.ToInt32(startAndEndFactoryNumbers[1]);
                        i++)
                    {
                        factoryNumbers.Add(i);
                    }
                }
            }

            return factoryNumbers;
        }

        /// <summary>
        /// Сброс проверки наличия на балансе.
        /// </summary>
        private void ResetEnough(IEnumerable<IEnoughDetail> items)
        {
            foreach (var item in items)
            {
                item.Enough = Enough.Unknown;
            }
        }

        /// <summary>
        /// Поиск нужного изделия.
        /// </summary>
        private void SearchProduct(string value)
        {
            FilterProductsList = new ObservableCollection<Product>();

            if (value.Trim() == string.Empty)
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

        public ICommand AllFilterDeleteCommand => _allFilterDeleteCommand ??
                                                  (_allFilterDeleteCommand =
                                                      new RelayCommand(obj => AllFilterDelete()));

        /// <summary>
        /// Нажатие кнопки [УДЛ] DataGrid фильтрации - удаление указанного фильтра
        /// </summary>
        private RelayCommand _filterDeleteCommand;

        public ICommand FilterDeleteCommand
        {
            get
            {
                return _filterDeleteCommand ??
                       (_filterDeleteCommand = new RelayCommand(obj => FilterDelete(obj as string)));
            }
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
            var columnName = (string) popupFilterValue.Tag; // Получения столбца из свойства Tag поля ввода
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
                var item = new Dictionary<string, FilterValue> {[displayedItem.Key] = displayedItem.Value};
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
                var item = new Dictionary<string, FilterValue> {[displayedItem.Key] = displayedItem.Value};
                DisplayedFilter.Add(item);
            }

            ReloadDisplayItems(FilterCriterias, MapFilterPredicate);
        }

        /// <summary>
        /// Перезаполнение главного DataGrid страницы с учётом фильтров
        /// </summary>
        public void ReloadDisplayItems(FilterCriterias filterCriterias, Predicate<object> predicate)
        {
            var viewSource = CollectionViewSource.GetDefaultView(CompositionProducts);
            viewSource.Filter = filterCriterias.IsEmpty ? null : predicate;
            DisplayCompositionProducts = viewSource;
        }

        /// <summary>
        /// Метод-предикат (булевый) текущей записи коллекции сущностей, который возвращает true или 
        /// false в зависимости от попадания в диапазон фильтра по всем полям фильтрации.
        /// </summary>
        private bool MapFilterPredicate(object rawEntity)
        {
            var entity = (CompositionProduct) rawEntity;
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
                      || FilterCriterias.ContainsLine(entity.Detail.Code.ToString(CultureInfo.CurrentCulture), buffer);
            result &= !filter.GetValue("Detail.Name", out buffer)
                      || FilterCriterias.ContainsLine(entity.Detail.Name, buffer);
            result &= !filter.GetValue("Detail.Designation", out buffer)
                      || FilterCriterias.ContainsLine(entity.Detail.Designation, buffer);
            result &= !filter.GetValue("Detail.Gost", out buffer)
                      || FilterCriterias.ContainsLine(entity.Detail.Gost, buffer);
            result &= !filter.GetValue("Detail.Gost", out buffer)
                      || FilterCriterias.ContainsLine(entity.Count.ToString(CultureInfo.InvariantCulture), buffer);
            result &= !filter.GetValue("Detail.Measure.ShortName", out buffer)
                      || FilterCriterias.ContainsLine(entity.Detail.Measure.ShortName, buffer);
            result &= !filter.GetValue("Detail.TmcType.ShortName", out buffer)
                      || FilterCriterias.ContainsLine(entity.Detail.TmcType.ShortName, buffer);
            return result;
        }

        #endregion
    }
}
