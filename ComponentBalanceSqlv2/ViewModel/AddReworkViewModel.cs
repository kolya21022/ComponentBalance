using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class AddReworkViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();

        private readonly MainViewModel _mainViewModel;
        private readonly MonthYear _monthYear;

        private string _numberAct;

        private string _factoryNumber;
        private int _selectedMonthRelease;
        private int _selectedYearRelease;
        private ReleaseProduct _selectedReleaseProduct;
        private ObservableCollection<Move> _movesSelectedReleaseProduct;


        private List<Product> _products;
        private string _searchTextProduct;
        private bool _isSearchProduct;
        private ObservableCollection<Product> _filterProductsList;
        private Product _selectedProduct;
        private ObservableCollection<CompositionProduct> _compositionProducts;
        private CompositionProduct _selectedCompositionProduct;
        private ObservableCollection<ReplacementDetail> _replacementDetails; // все замены _compositionProducts, что бы вечно в базу не лазить
        private ObservableCollection<ReplacementDetail> _displayReplacementDetails; // замены _selectedCompositionProduct
        private ReplacementDetail _selectedReplacementDetail;
        private List<Balance> _balances = new List<Balance>(); // баланс _compositionProducts и _replacementDetails
        private bool _isCompositionChecked;

        private bool _isCanSave;
        private List<NotEnoughDetailReport> _notEnoughDetails;
        #endregion
        #region Public Fields
        public WorkGuild WorkGuild { get; }
        public string NumberAct
        {
            get { return _numberAct; }
            set
            {
                _numberAct = value;
                OnPropertyChanged();
            }
        }
        public string FactoryNumber
        {
            get { return _factoryNumber; }
            set
            {
                _factoryNumber = value;
                OnPropertyChanged();
            }
        }
        public Dictionary<int, string> MonthDictionary { get; }  
        public int SelectedMonthRelease
        {
            get { return _selectedMonthRelease; }
            set
            {
                _selectedMonthRelease = value;
                OnPropertyChanged();
            }
        }
        public int SelectedYearRelease
        {
            get { return _selectedYearRelease; }
            set
            {
                _selectedYearRelease = value;
                OnPropertyChanged();
            }
        }
        public ReleaseProduct SelectedReleaseProduct
        {
            get { return _selectedReleaseProduct; }
            set
            {
                _selectedReleaseProduct = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Move> MovesSelectedReleaseProduct
        {
            get { return _movesSelectedReleaseProduct; }
            set
            {
                _movesSelectedReleaseProduct = value;
                OnPropertyChanged();
            }
        }
        public List<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
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
        public bool IsSearchProduct
        {
            get { return _isSearchProduct; }
            set { _isSearchProduct = value; OnPropertyChanged(); }
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
                IsSearchProduct = false;
                OnPropertyChanged();

                ReloadProduct();
                ReloadReleaseMoves();
            }
        }
        public ObservableCollection<CompositionProduct> CompositionProducts
        {
            get { return _compositionProducts; }
            set
            {
                _compositionProducts = value;
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
        public ObservableCollection<ReplacementDetail> DisplayReplacementDetails
        {
            get { return _displayReplacementDetails; }
            set { _displayReplacementDetails = value; OnPropertyChanged(); }
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
        public bool IsCompositionChecked
        {
            get { return _isCompositionChecked; }
            set
            {
                _isCompositionChecked = value;
                if (CompositionProducts != null)
                {
                    foreach (var compositionProduct in CompositionProducts)
                    {
                        compositionProduct.IsSelectedInRelease = value;
                    }
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region Command
        #region Кнопка: [MAX]
        private RelayCommand _maxSelectedReplacementCommand;
        public ICommand MaxSelectedReplacementCommand => _maxSelectedReplacementCommand ?? (_maxSelectedReplacementCommand = new RelayCommand(obj => Max()));

        private void Max()
        {
            if (SelectedReplacementDetail == null || SelectedReplacementDetail.Count != 0M) { return; }

            var count = SelectedCompositionProduct.Count - CountBalanceWithReplacementSelectedCompositionProduct;
            SelectedReplacementDetail.Count = CountBalanceSelectedReplacementDetail > count
                ? count
                : CountBalanceSelectedReplacementDetail;
        }
        #endregion
        #region Кнопка: [Поиск]
        private RelayCommand _searchCommand;
        public ICommand SearchCommand => _searchCommand ?? (_searchCommand = new RelayCommand(obj => SearchRelease(), obj => CanSearchRelease()));

        private void SearchRelease()
        {
            _mainViewModel.TaskInProgress = true;
            Task.Run(() => DoSearchRelease()).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
        }

        private bool CanSearchRelease()
        {
            if (string.IsNullOrEmpty(FactoryNumber))
            {
                return false;
            }
            // Сказали что доработка делается только на выпущенные ранее станки.
            if (SelectedMonthRelease == _monthYear.Month && SelectedYearRelease == _monthYear.Year)
            {
                return false;
            }
            return true;
        }

        private void DoSearchRelease()
        {
            SelectedReleaseProduct = _context.ReleaseProducts
                .Include(i => i.Product)
                .Include(i => i.Moves.Select(j => j.Balance.Detail.Measure))
                .Include(i => i.Moves.Select(j => j.Balance.Detail.TmcType))
                .FirstOrDefault(i => i.WorkGuildId == WorkGuild.Id
                                     && i.Month == SelectedMonthRelease
                                     && i.Year == SelectedYearRelease
                                     && i.FactoryNumber == FactoryNumber);
            ReloadReleaseMoves();
            ReloadProduct();
        }
        #endregion
        #region Кнопка: [Разница]
        private RelayCommand _calculateDifferenceCommand;
        public ICommand CalculateDifferenceCommand => _calculateDifferenceCommand ??
                                         (_calculateDifferenceCommand = new RelayCommand(obj => CalculateDifference(), obj => CanCalculateDifference()));

        private void CalculateDifference()
        {
            _mainViewModel.TaskInProgress = true;
            Task.Run(() => Application.Current.Dispatcher.Invoke(DoCalculateDifference)).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
        }

        // наверно лучше коментов накидать на всякий
        // детали дорабатываемого изделия идут на приход (+)
        // детали доработанного изделия идут на расход (-)
        private void DoCalculateDifference()
        {
            // новый список деталей на приход
            var newMoves = new List<Move>();
            // идем по списку на приход
            foreach (var move in MovesSelectedReleaseProduct)
            {
                // ищем нужна ли детали прихода в расходе
                var compositionProduct = CompositionProducts.FirstOrDefault(i => i.DetailId == move.Balance.DetailId);
                // если не нужна оставляем в списке на приход
                if (compositionProduct == null)
                {
                    newMoves.Add(move);
                    continue;
                }

                // нужна в расходе но ее меньше чем надо - уменьшаем кол-во нужных в расходе и в списке на приход удаляем
                if (compositionProduct.Count > move.Count)
                {
                    compositionProduct.Count -= move.Count;
                    continue;
                }

                // нужна в расходе и ее столько же на приходе - выкидываем из обоих списков
                if (compositionProduct.Count == move.Count)
                {
                    CompositionProducts.Remove(compositionProduct);
                    continue;
                }

                // нужна в расходе и ее больше чем надо - выкидываем из списка расхода и в списке на приход уменьшаем кол-во
                if (compositionProduct.Count < move.Count)
                {
                    CompositionProducts.Remove(compositionProduct);
                    move.Count -= compositionProduct.Count;
                    newMoves.Add(move);
                }
            }
            // формируем новый список на приход из оставшихся деталей
            MovesSelectedReleaseProduct = new ObservableCollection<Move>(newMoves);
            newMoves = new List<Move>();

            // если что то осталось надо глянуть может на замену в расходе можно запихнуть
            foreach (var move in MovesSelectedReleaseProduct)
            {
                // ищем все детали прихода которые подходят под замену
                var replacementDetails = ReplacementDetails.Where(i => i.DetailEndCode == move.Balance.Detail.Code);
                // если есть
                foreach (var replacementDetail in replacementDetails)
                {
                    // ищем в расходе основную деталь которая заменяется подходящим приходом (не забываем про ед изм, в заменах они не указываются)
                    var compositionProducts = CompositionProducts.Where(i => i.Detail.Code == replacementDetail.DetailStartCode
                                                                             && i.Detail.MeasureId == move.Balance.Detail.MeasureId).ToList();
                    // если нашлось что то
                    foreach (var compositionProduct in compositionProducts)
                    {
                        // если на приходе меньше чем надо в расходе - уменьшаем расход, обнуляем приход(в будущем что б удалить)
                        if (compositionProduct.Count > move.Count)
                        {
                            compositionProduct.Count -= move.Count;
                            move.Count = 0M;
                            break;
                        }
                        // если на приходе столько же как и на расходе - удаляем из расхода, обнуляем приход
                        if (compositionProduct.Count == move.Count)
                        {
                            CompositionProducts.Remove(compositionProduct);
                            move.Count = 0M;
                            break;
                        }
                        // если меньше на расходе чем на приходе - удаляем из расхода и уменьшаем на приходе
                        if (compositionProduct.Count < move.Count)
                        {
                            CompositionProducts.Remove(compositionProduct);
                            move.Count -= compositionProduct.Count;
                            
                        }
                    }
                }
                // если не обнулен то добавляем в новый список прихода
                if (move.Count != 0M)
                {
                    newMoves.Add(move);
                }
            }
            // формируем новый список на приход из оставшихся на приход
            MovesSelectedReleaseProduct = new ObservableCollection<Move>(newMoves);

            BuildMessageBox.GetInformationMessageBox("Осталась разница.");
        }
        private bool CanCalculateDifference()
        {
            if (!MovesSelectedReleaseProduct.Any() || !CompositionProducts.Any())
            {
                return false;
            }
            return true;
        }
        #endregion
        #region Кнопка: [Проверка]
        private RelayCommand _checkCommand;
        public ICommand CheckCommand => _checkCommand ?? (_checkCommand = new RelayCommand(obj => Check()));

        private void Check()
        {
            _mainViewModel.TaskInProgress = true;
            Task.Run(() => DoCheck()).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
            while (_mainViewModel.TaskInProgress)
            {
                // думаю это костыль
            }

            if (_isCanSave)
            {
                BuildMessageBox.GetInformationMessageBox("Вы можете провести доработку.");
                return;
            }
            var result = BuildMessageBox.GetConfirmMessageBox("Вы не можете провести доработку.\n" +
                                                              "Вам не хватает деталей.\n " +
                                                              "Хотите распечатать их?");
            if (result != MessageBoxResult.Yes) { return; }

            const string heading = "Недостающие детали в изделии";
            const string reportType = "NotEnoughDetailReport";
            const string reportFileName = "NotEnoughDetail.rdlc";

            var reportFile = Common.GetReportFilePath(reportFileName);

            var reportViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                SelectedProduct.Code, SelectedProduct.Name, SelectedProduct.Designation, 1,
                _notEnoughDetails);

            var addReleaseWindow = new ReportWindow()
            {
                DataContext = reportViewModel,
                Owner = Common.GetOwnerWindow()
            };
            addReleaseWindow.ShowDialog();
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
                var count = compositionProduct.Count;
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
                    }
                    break;
                }

                var balance = _balances.FirstOrDefault(i => i.DetailId == compositionProduct.DetailId);
                if (balance != null)
                {
                    if (balance.CountEnd >= count)
                    {
                        compositionProduct.Enough = Enough.Yes;
                        continue;
                    }
                    if (balance.CountEnd > 0 && compositionProduct.IsCanUsePart)
                    {
                        compositionProduct.Enough = Enough.Optional;
                        continue;
                    }
                }

                isCanRelease = false;
                compositionProduct.Enough = Enough.No;

                _notEnoughDetails.Add(new NotEnoughDetailReport("Документация", SelectedProduct, 1,
                    compositionProduct.Detail, compositionProduct.Count, balance?.CountEnd ?? 0M));
            }
            // Теперь сами замены проверяем что бы были
            foreach (var replacementDetail in ReplacementDetails.Where(j => j.Count > 0M))
            {
                // Нужно что бы правильно ед.измерения найти и ТМЦ, и склад.
                var detailFirst = CompositionProducts.First(i => i.Detail.Code == replacementDetail.DetailStartCode).Detail;
                // Находим на балансе замену с ед измерения и тмц как у стандартной детали. 
                var balance = _balances.FirstOrDefault(i => i.Detail.Code == replacementDetail.DetailEndCode
                                                           && i.Detail.MeasureId == detailFirst.MeasureId
                                                           && i.Detail.TmcTypeId == detailFirst.TmcTypeId
                                                           && i.Detail.Warehouse == detailFirst.Warehouse);
                if (balance != null && balance.CountEnd >= replacementDetail.Count)
                {
                    replacementDetail.Enough = Enough.Yes;
                    continue;
                }
                // Если не нашли или кол-во меньше указанного.
                isCanRelease = false;
                replacementDetail.Enough = Enough.No;

                _notEnoughDetails.Add(new NotEnoughDetailReport("Указанная замена", SelectedProduct, 1,
                    replacementDetail, detailFirst.Measure.ShortName, balance?.CountEnd ?? 0M));
            }
            if (isCanRelease)
            {
                _isCanSave = true;
                return;
            }
            if (builder.Length > 0)
            {
                BuildMessageBox.GetLogicErrorMessageBox(builder.ToString());
            }
        }
        #endregion
        #region Кнопка: [Сохранить]
        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(obj => Save(), obj => _isCanSave));
        private void Save()
        {
            // todo переформулировать сообщение
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              $"Вы хотите провести доработку {NumberAct}\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) return;

            _mainViewModel.TaskInProgress = true;
            Task.Run(() => DoSave()).ContinueWith(task => { _mainViewModel.TaskInProgress = false; });
        }

        private void DoSave()
        {
            var releaseProductId = new ReleaseProductService(_context).FindOrAddIfNotExists(WorkGuild.Id,
                SelectedProduct.Id, SelectedReleaseProduct.FactoryNumber, _monthYear).Id;
            AddExportRework(_context, releaseProductId);
            AddImportRework(_context, releaseProductId);

            _context.SaveChanges();

            BuildMessageBox.GetInformationMessageBox("Доработка успешно проведена.\n");
            _isCanSave = false;
        }

        private void AddExportRework(DomainContext context, long releaseProductId)
        {
            foreach (var compositionProduct in CompositionProducts.Where(i => i.IsSelectedInRelease))
            {
                var balance = context.Balances.First(i => i.DetailId == compositionProduct.DetailId
                                                          && i.WorkGuildId == WorkGuild.Id
                                                          && i.Month == _monthYear.Month
                                                          && i.Year == _monthYear.Year);
                var count = compositionProduct.Count;
                // Ищем замену
                foreach (var replacementDetail in ReplacementDetails.Where(j => j.Count > 0M))
                {
                    if (compositionProduct.Detail.Code == replacementDetail.DetailStartCode)
                    {
                        if (replacementDetail.Count >= count)
                        {
                            replacementDetail.Count -= count;
                            AddExportReworkOnReplacementDetail(context, releaseProductId,
                                replacementDetail.DetailStartCode,
                                replacementDetail.DetailEndCode, count);
                            count = 0M;
                            break;
                        }

                        if (replacementDetail.Count == 0)
                        {
                            break;
                        }

                        AddExportReworkOnReplacementDetail(context, releaseProductId,
                            replacementDetail.DetailStartCode,
                            replacementDetail.DetailEndCode,
                            replacementDetail.Count);
                        count -= replacementDetail.Count;
                        replacementDetail.Count = 0M;
                    }
                }

                if (count != 0M
                    && balance.CountEnd > 0M) // Условие было добавлено для необязательных деталей по просьбе сверху.
                {
                    balance.AddExportReworkMove(
                        count <= balance.CountEnd ? count : balance.CountEnd,  // Условие было добавлено для необязательных деталей по просьбе сверху.
                        releaseProductId, NumberAct);
                }
            }
        }

        /// <summary>
        /// Добавление замен расхода доработки.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="releaseProductId">Выпуск дорабатываемый</param>
        /// <param name="replacementDetailStartCode">Заменяймая деталь расхода доработки</param>
        /// <param name="replacementDetailEndCode">Замена детали расхода доработки</param>
        /// <param name="count">Количество расхода на доработку</param>
        private void AddExportReworkOnReplacementDetail(DomainContext context, long releaseProductId, long replacementDetailStartCode, long replacementDetailEndCode, decimal count)
        {
            // Нужно что бы правильно ед.измерения найти и ТМЦ, и склад.
            var detailFirst = CompositionProducts.First(i => i.Detail.Code == replacementDetailStartCode).Detail;
            // Находим на балансе замену с ед измерения и тмц как у заменяймой детали. 
            var balance = context.Balances.First(i => i.Detail.Code == replacementDetailEndCode
                                                      && i.Detail.MeasureId == detailFirst.MeasureId
                                                      && i.Detail.TmcType == detailFirst.TmcType
                                                      && i.Detail.Warehouse == detailFirst.Warehouse

                                                      && i.Month == _monthYear.Month
                                                      && i.Year == _monthYear.Year
                                                      && i.WorkGuildId == WorkGuild.Id);

            balance.AddExportReworkMove(count, releaseProductId, NumberAct);
        }

        private void AddImportRework(DomainContext context, long releaseProductId)
        {
            foreach (var move in MovesSelectedReleaseProduct)
            {
                // Найти или создать баланс детали в этом месяце, так как ImportRework это прошлые месяцы.
                var balance = new BalanceService(context).FindOrAddIfNotExists(WorkGuild.Id, move.Balance.DetailId, _monthYear);
                balance.AddImportReworkMove(move.Count, move.Cost, releaseProductId, NumberAct);
            }
        }
        #endregion
        #endregion

        public AddReworkViewModel(MainViewModel mainViewModel, WorkGuild workGuild, MonthYear monthYear)
        {
            _mainViewModel = mainViewModel;
            WorkGuild = workGuild;
            _monthYear = monthYear;

            MonthDictionary = Constants.MonthsFullNames;
            SelectedMonthRelease = monthYear.Month;
            SelectedYearRelease = monthYear.Year;

            _products = _context.Products.AsNoTracking().ToList();
            FilterProductsList = new ObservableCollection<Product>(_products);

            MovesSelectedReleaseProduct = new ObservableCollection<Move>();
            CompositionProducts = new ObservableCollection<CompositionProduct>();
        }
   
        #region Доп информация для цехов
        /// <summary>
        /// Кол-во на балансе выбранной детали.
        /// </summary>
        private decimal _countBalanceSelectedCompositionProduct;
        /// <summary>
        /// Кол-во на балансе + выбранные замены выбранной детали.
        /// </summary>
        private decimal _countBalanceWithReplacementSelectedCompositionProduct;
        /// <summary>
        /// Кол-во на балансе выбранной замены.
        /// </summary>
        private decimal _countBalanceSelectedReplacementDetail;
        private decimal GetCountBalanceDetailWithReplacement()
        {
            var count = CountBalanceSelectedCompositionProduct;
            foreach (var displayReplacementDetail in DisplayReplacementDetails)
            {
                count += displayReplacementDetail.Count;
            }
            return count;
        }

        public decimal CountBalanceSelectedCompositionProduct
        {
            get { return _countBalanceSelectedCompositionProduct; }
            set
            {
                _countBalanceSelectedCompositionProduct = value;
                OnPropertyChanged();
            }
        }
        public decimal CountBalanceWithReplacementSelectedCompositionProduct
        {
            get { return _countBalanceWithReplacementSelectedCompositionProduct; }
            set
            {
                _countBalanceWithReplacementSelectedCompositionProduct = value;
                OnPropertyChanged();
            }
        }
        public decimal CountBalanceSelectedReplacementDetail
        {
            get { return _countBalanceSelectedReplacementDetail; }
            set
            {
                _countBalanceSelectedReplacementDetail = value;
                OnPropertyChanged();
            }
        }
        private decimal GetCountBalanceDetail(long detailEndCode)
        {
            decimal count = 0;
            var balance = _balances.FirstOrDefault(i => i.Detail.Code == detailEndCode);
            if (balance != null)
            {
                count = balance.CountEnd;
            }
            return count;
        }
        #endregion
        private void ReloadReleaseMoves()
        {
            if (SelectedReleaseProduct == null)
            {
                MovesSelectedReleaseProduct = new ObservableCollection<Move>();
                return;
            }

            MovesSelectedReleaseProduct = new ObservableCollection<Move>(SelectedReleaseProduct.Moves
                .Where(i =>
                    i.Balance.Detail.TmcType.ShortName == "KOM" ||
                    i.Balance.Detail.TmcType.ShortName == "FAB"));

            //_isCanSave = false;
        }
        private void ReloadProduct()
        {
            if (SelectedProduct != null)
            {
                CompositionProducts =
                    new ObservableCollection<CompositionProduct>(
                        _context.CompositionProducts
                            .Where(i => i.ProductId == SelectedProduct.Id
                                        && i.WorkGuildId == WorkGuild.Id
                                        && (i.Detail.TmcType.ShortName == "KOM" || i.Detail.TmcType.ShortName == "FAB"))
                            .Include(i => i.Detail)
                            .Include(i => i.Detail.Measure)
                            .Include(i => i.Detail.TmcType)
                            .AsNoTracking());

                var detailCodeInCompositionProducts = CompositionProducts.Select(j => j.Detail.Code).ToList();
                ReplacementDetails =
                    new ObservableCollection<ReplacementDetail>(
                        _context.ReplacementDetails
                            .Where(i => i.ShortProductCode == SelectedProduct.ShortCode
                                        && detailCodeInCompositionProducts.Contains(i.DetailStartCode))
                            .AsNoTracking());

                var detailsIdInCompositionProducts = CompositionProducts.Select(i => i.DetailId);
                var detailsEndCodeInReplacementDetails = ReplacementDetails.Select(i => i.DetailEndCode);
                _balances = new BalanceService(_context)
                    .GetBalancesByIdDetailsAndCodeDetails(WorkGuild.Id, _monthYear,
                        detailsIdInCompositionProducts, detailsEndCodeInReplacementDetails)
                    .ToList();
            }
            //_isCanSave = false;
        }
        /// <summary>
        /// Сброс проверки наличия на балансе.
        /// </summary>
        private static void ResetEnough(IEnumerable<IEnoughDetail> items)
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
                IsSearchProduct = false;
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;

            foreach (var product in _products)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= product.Code.ToString(CultureInfo.CurrentCulture).IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || product.Name.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || product.Designation.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }
                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterProductsList.Add(product);
                }
            }
            IsSearchProduct = FilterProductsList.Count != 0;
        }
    }
}
