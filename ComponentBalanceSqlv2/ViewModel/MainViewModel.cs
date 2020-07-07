using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Dbf;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Model.ParametrsBuilder;
using ComponentBalanceSqlv2.Services;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;
using ComponentBalanceSqlv2.ViewModel.RequestReplacements;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class MainViewModel : BaseNotificationClass
    {
        private readonly DomainContext _context = new DomainContext();

        #region Private Fields  
        private const int START_SELECTED_YEAR_CALCULATE = 1899;

        private BaseNotificationClass _displayViewModel;
        private bool _taskInProgress;
        private string _hotkeysText;
        private bool _hotkeysVisibility;
        private int _selectedMonth;
        private int _selectedYear = START_SELECTED_YEAR_CALCULATE;
        #endregion

        #region Public Fields  
        public Dictionary<int, string> MonthDictionary { get; }

        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                if (_selectedMonth != 0 && _selectedYear != START_SELECTED_YEAR_CALCULATE)
                {
                    WorkMonthYear.Month = value;
                    _context.Entry(WorkMonthYear).State = EntityState.Modified;
                    _context.SaveChanges();
                    OnPropertyChanged($"WorkMonthYear");
                }

                _selectedMonth = value;
                OnPropertyChanged();
            }
        }

        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                if (_selectedMonth != 0 && _selectedYear != START_SELECTED_YEAR_CALCULATE)
                {
                    WorkMonthYear.Year = value;
                    _context.Entry(WorkMonthYear).State = EntityState.Modified;
                    _context.SaveChanges();
                    OnPropertyChanged($"WorkMonthYear");
                }

                _selectedYear = value;
                OnPropertyChanged();
            }
        }

        public BaseNotificationClass DisplayViewModel
        {
            get { return _displayViewModel; }
            set { _displayViewModel = value; OnPropertyChanged(); }
        }

        public bool TaskInProgress
        {
            get { return _taskInProgress; }
            set { _taskInProgress = value; OnPropertyChanged(); }
        }

        public string HotkeysText
        {
            get { return _hotkeysText; }
            set
            {
                _hotkeysText = value;
                HotkeysVisibility = value != string.Empty;
                OnPropertyChanged();
            }
        }

        public bool HotkeysVisibility
        {
            get { return _hotkeysVisibility; }
            set
            { _hotkeysVisibility = value;   OnPropertyChanged(); }
        }

        public WorkGuild Login { get; }
        public bool IsVisibleLvl1 { get; }
        public MonthYear WorkMonthYear { get; }
        #endregion

        #region Commands  
        #region Меню: кнопка [Сменить пароля]
        private RelayCommand _changePasswordMenuItemCommand;

        public ICommand ChangePasswordMenuItemCommand => _changePasswordMenuItemCommand ??
                                                         (_changePasswordMenuItemCommand =
                                                             new RelayCommand(obj => DoChangePasswordWindow()));

        private void DoChangePasswordWindow()
        {
            var changePasswordWindowViewModel = new ChangePasswordViewModel(Login);

            var changePasswordWindow = new ChangePasswordWindow()
            {
                DataContext = changePasswordWindowViewModel,
                Owner = Common.GetOwnerWindow()
            };
            changePasswordWindow.ShowDialog();
        }
        #endregion
        #region Меню: кнопка [Пользовательские настройки]

        private RelayCommand _userConfigMenuItemCommand;
        public ICommand UserConfigMenuItemCommand => _userConfigMenuItemCommand ??
                                                     (_userConfigMenuItemCommand =
                                                         new RelayCommand(obj => UserConfigMenuItem()));

        private void UserConfigMenuItem()
        {
            var userConfigViewModel = new UserConfigViewModel();

            var userConfigWindow = new UserConfigWindow()
            {
                DataContext = userConfigViewModel,
                Owner = Common.GetOwnerWindow()
            };
            userConfigWindow.ShowDialog();
        }


        #endregion

        #region Меню [Справочник взаимозаменяемых деталей]
        private RelayCommand _replacementDetailsCommand;
        public ICommand ReplacementDetailsCommand => _replacementDetailsCommand ??
                                                     (_replacementDetailsCommand =
                                                         new RelayCommand(obj =>
                                                             ReplacementDetails()));
        private void ReplacementDetails()
        {
            TaskInProgress = true;
            Task.Run(() =>
                {
                    HotkeysText = string.Empty;
                    DisplayViewModel = new ReplacementDetailsViewModel();
                })
                .ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Меню [Корректировка обязательных деталей выпуска]
        private RelayCommand _editReleaseProductCommand;
        public ICommand EditReleaseProductCommand =>
            _editReleaseProductCommand ?? (_editReleaseProductCommand =
                new RelayCommand(obj => EditReleaseProduct()));

        private void EditReleaseProduct()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetMessage("Укажите цех  и изделие");
            builder.SetIsWorkGuild(true);
            builder.SetIsProduct(true);
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedProduct = parametrsViewModel.SelectedProduct;
            var selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;

            TaskInProgress = true;
            Task.Run(() =>
            {
                var editReleaseProductViewModel = new EditCompositionProductViewModel(selectedWorkGuild, selectedProduct);
                HotkeysText = string.Empty;
                DisplayViewModel = editReleaseProductViewModel;
            })
                .ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Меню [Добавить в выпуск не входящую по документации деталь]
        private RelayCommand _addDetailInReleaseCommand;
        public ICommand AddDetailInReleaseCommand =>
            _addDetailInReleaseCommand ??
            (_addDetailInReleaseCommand =
                new RelayCommand(obj => AddDetailInRelease()));

        private void AddDetailInRelease()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetMessage("Укажите цех, месяц и год");
            builder.SetIsMonthYear(true);
            builder.SetIsWorkGuild(true);
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            var selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;

            var addDetailInReleaseViewModel = new AddDetailInReleaseViewModel(selectedWorkGuild, selectedMonthYear);
            HotkeysText = string.Empty;
            DisplayViewModel = addDetailInReleaseViewModel;
        }

        #endregion
        #region Меню [Обновить состав изделия(одного)]
        private RelayCommand _updateСompositionOfProductCommand;

        public ICommand UpdateConsistOfProductCommand => _updateСompositionOfProductCommand ??
                                                         (_updateСompositionOfProductCommand =
                                                             new RelayCommand(obj => UpdateConsistOfProduct()));
        private void UpdateConsistOfProduct()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsProduct(true);
            builder.SetMessage("Укажите изделие");
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();

            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }
            var selectedProduct = parametrsViewModel.SelectedProduct;

            TaskInProgress = true;
            var compositionProductService = new CompositionProductService();
            Task.Run(() => compositionProductService.UpdateProductFromDbf(selectedProduct))
                .ContinueWith(task =>
                {
                    TaskInProgress = false;
                    HotkeysText = string.Empty;
                    compositionProductService.SaveChanges();
                    BuildMessageBox.GetInformationMessageBox("Обновление успешно завершено.");
                });
        }
        #endregion
        #region Меню [Обновить наименования Изделий и Деталей (из Prdsetmc)]
        private RelayCommand _updateNameProductsAndDetailsCommand;

        public ICommand UpdateNameProductsAndDetails => _updateNameProductsAndDetailsCommand ??
                                                         (_updateNameProductsAndDetailsCommand =
                                                             new RelayCommand(obj => UpdateNameProductsAndDetailsCommand()));
        private void UpdateNameProductsAndDetailsCommand()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              "Вы хотите обновить наименова изделий и деталей\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) { return; }

            TaskInProgress = true;
            Task.Run(() => DoUpdateNameProductsAndDetailsCommand())
                .ContinueWith(task =>
                {
                    TaskInProgress = false;
                    BuildMessageBox.GetInformationMessageBox("Обновление успешно завершено.");
                });
        }

        private void DoUpdateNameProductsAndDetailsCommand()
        {
            var proructs = ProductService.GetAllInDbf();
            var contextProducts = _context.Products.ToList();
            foreach (var proruct in proructs)
            {
                var list = contextProducts.Where(i => i.Code == proruct.Code).ToList();
                foreach (var item in list)
                {
                    item.Name = proruct.Name;
                    item.Designation = proruct.Designation;
                }
            }
            var details = DetailService.GetAllInDbf();
            var contextDetails = _context.Details.ToList();
            var contextReplacementDetails = _context.ReplacementDetails.ToList();
            foreach (var detail in details)
            {
                var list = contextDetails.Where(i => i.Code == detail.Code).ToList();
                foreach (var item in list)
                {
                    item.Name = detail.Name;
                    item.Designation = detail.Designation;
                    item.Gost = detail.Gost;
                } 
                               
                var list2 = contextReplacementDetails.Where(i => i.DetailStartCode == detail.Code).ToList();
                foreach (var item in list2)
                {
                    item.DetailStartName = detail.Name;
                    item.DetailStartDesignation = detail.Designation;
                    item.DetailStartGost = detail.Gost;
                }

                var list3 = contextReplacementDetails.Where(i => i.DetailEndCode == detail.Code).ToList();
                foreach (var item in list3)
                {
                    item.DetailEndName = detail.Name;
                    item.DetailEndDesignation = detail.Designation;
                    item.DetailEndGost = detail.Gost;
                }
            }

            foreach (var contextReplacementDetail in contextReplacementDetails)
            {
                var detail = details.FirstOrDefault(i => i.Code == contextReplacementDetail.DetailStartCode);
                if (detail == null)
                {
                    _context.ReplacementDetails.Remove(
                        _context.ReplacementDetails.First(i => i.Id == contextReplacementDetail.Id));
                }
                else
                {
                    contextReplacementDetail.DetailStartName = detail.Name;
                    contextReplacementDetail.DetailStartDesignation = detail.Designation;
                    contextReplacementDetail.DetailStartGost = detail.Gost;
                }
            }

            _context.SaveChanges();
        }
        #endregion

        #region Подготовка к счету нового месяца: кнопка [Расчет балансов всех цехов]
        private ICommand _calculateBalanceCommand;
        public ICommand CalculateBalanceCommand => _calculateBalanceCommand ?? (_calculateBalanceCommand = new RelayCommand(arg => CalculateBalance()));
        private void CalculateBalance()
        {
            var messages = "Внимание!\n" +
                           "Вы начинаете производить расчёт " +
                           "за " + WorkMonthYear.DisplayMonth + " " + WorkMonthYear.Year + " г.\n" +
                           "Желаете продолжить?";
            var result = BuildMessageBox.GetConfirmMessageBox(messages);
            if (result != MessageBoxResult.Yes) { return; }

            TaskInProgress = true;
            Task.Run(() => DoCalculateBalance()).ContinueWith(task =>
            {
                TaskInProgress = false;
            });   
        }

        private void DoCalculateBalance()
        {
            var previousMonthYear = WorkMonthYear.GetPreviousMonth();
            try
            {
                if (IsWasCalculate(_context, WorkMonthYear))
                {
                    return;
                }

                if (!IsAllWorkguildClose(_context, previousMonthYear))
                {
                    return;
                }

                if (!IsAllWarehouseClose(WorkMonthYear))
                {
                    return;
                }

                if (!IsPreparingFiles())
                {
                    return;
                }

                // Была ли незавершенка.
                var monthYearNzp = CalculateBalanceService.GetDateNzp();
                var isCheckedUnfinished = WorkMonthYear.Month == monthYearNzp.Month
                                          && WorkMonthYear.Year == monthYearNzp.Year;

                using (var transaction = _context.Database.BeginTransaction())
                {
                    CalculateBalanceService.InsertCopyOldPkomInNewPkom(_context, isCheckedUnfinished, WorkMonthYear);
                    _context.SaveChanges();
                    BuildMessageBox.GetInformationMessageBox("Остатки прошлого месяца перенесены.\n" +
                                                             "Идет добавление приходов со склада.\n" +
                                                             "Ожидайте...");

                    CalculateBalanceService.PreparingSupply(WorkMonthYear);
                    CalculateBalanceService.InsertImportWarehouseMove(_context, WorkMonthYear);
                    _context.SaveChanges();


                    // открыть месяца
                    foreach (var workGuild in _context.WorkGuilds.Where(i => i.Lvl == 0).AsNoTracking().ToList())
                    {
                        _context.MonthCloses.Add(new MonthClose()
                        {
                            WorkGuildId = workGuild.Id,
                            IsClose = false,
                            Month = WorkMonthYear.Month,
                            Year = WorkMonthYear.Year
                        });
                    }
                    _context.SaveChanges();

                    transaction.Commit();
                }

                BuildMessageBox.GetInformationMessageBox("Расчет успешно завершен.");
            }
            catch (StorageException ex)
            {
                BuildMessageBox.GetCriticalErrorMessageBox(Common.ShowDetailExceptionMessage(ex));
            }
        }

        /// <summary>
        /// Проверка баланс уже был рассчитан.
        /// </summary>
        /// <returns> True - рассчитан. </returns>
        private bool IsWasCalculate(DomainContext context, MonthYear monthYear)
        {
            var balanse = context.Balances
                .AsNoTracking()
                .FirstOrDefault(i => i.Month == monthYear.Month
                                     && i.Year == monthYear.Year);
            var result = balanse != null;
            if (result)
            {
                BuildMessageBox.GetLogicErrorMessageBox("Расчет баланса уже произведен.\n" +
                                                        "Расчет балансов прекращен!");
            }

            return result;
        }

        /// <summary>
        /// Проверка все цеха закрыты.
        /// </summary>
        /// <returns> True - все. </returns>
        private bool IsAllWorkguildClose(DomainContext context, MonthYear monthYear)
        {
            var monthCloses = context.MonthCloses
                .AsNoTracking()
                .Where(i => i.Month == monthYear.Month
                            && i.Year == monthYear.Year
                            && !i.IsClose) // не закрыт
                .ToList();

            var result = monthCloses.Any();
            if (result)
            {
                var workGuildsNotClose = string.Empty;
                foreach (var item in monthCloses)
                {
                    workGuildsNotClose += item.WorkGuild.DisplayWorkguildString + " ";
                }
                BuildMessageBox.GetLogicErrorMessageBox($"{workGuildsNotClose} не закрыли месяц.\n" +
                                                        "Расчет балансов прекращен!");
            }

            return !result;
        }

        /// <summary>
        /// Проверка все ли склады закрыты.
        /// </summary>
        /// <returns> True - все. </returns>
        private bool IsAllWarehouseClose(MonthYear monthYear)
        {
            try
            {
                var monthYearCen = CalculateBalanceService.GetSrCenAndYear();

                var result = monthYearCen.Month == monthYear.Month
                             && monthYearCen.Year == monthYear.Year;
                if (!result)
                {
                    BuildMessageBox.GetLogicErrorMessageBox("Закрытие складов за " + monthYear.Month + " месяц не произведено.\n " +
                                                            "Расчет балансов прекращен!");
                }
                return result;
            }
            catch (OleDbException ex)
            {
                throw DbControl.HandleKnownDbFoxProExceptions(ex);
            }
        }

        /// <summary>
        /// Подготовка файлов, очистка рабочих баз, перенос в архив.
        /// </summary>
        /// <returns> True - успех. False - нет. </returns>
        private static bool IsPreparingFiles()
        {
            var dbFolderTempWork = Properties.Settings.Default.FoxProDbFolder_Temp_Work;
            var dbFolderTemp = Properties.Settings.Default.FoxProDbFolder_Temp;

            // Очистка Temp/Work
            File.Delete(Path.Combine(dbFolderTempWork, "cen.dbf"));
            File.Delete(Path.Combine(dbFolderTempWork, "pri.dbf"));

            // Копирование пустых бд с Temp в Temp/Work для работы
            File.Copy(Path.Combine(dbFolderTemp, "cen.dbf"), Path.Combine(dbFolderTempWork, "cen.dbf"), true);
            File.Copy(Path.Combine(dbFolderTemp, "pri.dbf"), Path.Combine(dbFolderTempWork, "pri.dbf"), true);
            return true;
        }
        #endregion
        #region Подготовка к счету нового месяца: кнопка [Открыть месяц цеху]
        private ICommand _openCorrectBalanceCommand;

        public ICommand OpenCorrectBalanceCommand => _openCorrectBalanceCommand ??
                                                     (_openCorrectBalanceCommand =
                                                         new RelayCommand(arg => OpenCorrectBalance()));

        private void OpenCorrectBalance()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsWorkGuild(true);
            builder.SetMessage("Укажите цех которому открыть месяц.");
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();

            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }
            var selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;

            var monthClose = _context.MonthCloses.First(i => i.WorkGuildId == selectedWorkGuild.Id
                                                             && i.Month == WorkMonthYear.Month
                                                             && i.Year == WorkMonthYear.Year);
            monthClose.IsClose = false;
            _context.SaveChanges();

            BuildMessageBox.GetInformationMessageBox("Месяц успешно открыт.");
        }

        #endregion
        #region Подготовка к счету нового месяца: кнопка [Обновить список взаимозаменяемых деталей]
        private RelayCommand _updateReplacementDetailsCommand;
        public ICommand UpdateReplacementDetailsCommand => _updateReplacementDetailsCommand ??
                                                            (_updateReplacementDetailsCommand =
                                                                new RelayCommand(obj => UpdateReplacementDetails()));

        private void UpdateReplacementDetails()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              "Вы хотите обновить список взаимозаменяемых деталей\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) { return; }

            TaskInProgress = true;
            var replacementDetailService = new ReplacementDetailService();
            Task.Run(() => replacementDetailService.AddFromDbf(WorkMonthYear))
                .ContinueWith(task =>
                {
                    TaskInProgress = false;
                    replacementDetailService.SaveChanges();
                    BuildMessageBox.GetInformationMessageBox("Обновление успешно завершено.");
                });
        }
        #endregion
        #region Подготовка к счету нового месяца: кнопка [Обновить состав изделия]
        private RelayCommand _updateCompositionProductsCommand;
        public ICommand UpdateCompositionProductsCommand => _updateCompositionProductsCommand ??
                                                            (_updateCompositionProductsCommand =
                                                                new RelayCommand(obj => UpdateCompositionProducts()));

        private void UpdateCompositionProducts()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Внимание!\n" +
                                                              "Вы хотите обновить состав изделий\n" +
                                                              "Это займет больше часа\n" +
                                                              "Желаете продолжить?");
            if (result != MessageBoxResult.Yes) { return; }

            TaskInProgress = true;
            var compositionProductService = new CompositionProductService();
            Task.Run(() => compositionProductService.UpdateAllFromDbf())
                .ContinueWith(task =>
                {
                    TaskInProgress = false;
                    compositionProductService.SaveChanges();
                    BuildMessageBox.GetInformationMessageBox("Обновление успешно завершено.");
                });
        }
        #endregion

        #region Корректировка: кнопка [Выпуск]
        private RelayCommand _releaseCommand;
        public ICommand ReleaseCommand => _releaseCommand ?? (_releaseCommand = new RelayCommand(obj => Release()));

        private void Release()
        {
            var selectedWorkGuild = Login;

            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsMonthYear(true);
            if (Login.Lvl == 1)
            {
                builder.SetMessage("Укажите цех, месяц и год");
                builder.SetIsWorkGuild(true);
            }
            else
            {
                builder.SetMessage("Укажите месяц и год");
            }
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            if (Login.Lvl == 1)
            {
                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }

            TaskInProgress = true;
            Task.Run(() =>
            {
                var releaseViewModel = new ReleaseViewModel(this, Login, selectedWorkGuild, selectedMonthYear);
                HotkeysText = string.Empty;
                DisplayViewModel = releaseViewModel;
            }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Корректировка: кнопка [Корректировка баланса]
        private ICommand _correctBalanceCommand;
        public ICommand CorrectBalanceCommand => _correctBalanceCommand ?? (_correctBalanceCommand = new RelayCommand(arg => CorrectBalance()));

        private void CorrectBalance()
        {
            var selectedWorkGuild = Login;

            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsMonthYear(true);
            if (Login.Lvl == 1)
            {
                builder.SetMessage("Укажите цех, месяц и год");
                builder.SetIsWorkGuild(true);
            }
            else
            {
                builder.SetMessage("Укажите месяц и год");
            }
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            if (Login.Lvl == 1)
            {
                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }

            TaskInProgress = true;
            Task.Run(() =>
                {
                    var balanceCorrectUserControlViewModel = new BalanceCorrectViewModel(Login, selectedWorkGuild, selectedMonthYear);
                    HotkeysText = string.Empty;
                    DisplayViewModel = balanceCorrectUserControlViewModel;
                }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Корректировка: кнопка [Закрыть месяц]
        private ICommand _сloseCorrectBalanceCommand;
        public ICommand CloseCorrectBalanceCommand => _сloseCorrectBalanceCommand ?? (_сloseCorrectBalanceCommand = new RelayCommand(arg => СloseCorrectBalance()));
        private void СloseCorrectBalance()
        {
            var selectedWorkGuild = Login;
            if (Login.Lvl == 1)
            {
                var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
                builder.SetIsWorkGuild(true);
                builder.SetMessage("Укажите цех которому закрыть месяц.");
                var parametrsViewModel = builder.GetParametrsViewModel();
                var parametersWindow = new ParametersWindow()
                {
                    DataContext = parametrsViewModel,
                    Owner = Common.GetOwnerWindow()
                };
                parametersWindow.ShowDialog();
                if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
                {
                    return;
                }

                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }

            var messages = "Внимание!\n" +
                           "Вы хотите закрыть " + Common.MonthsFullNames()[WorkMonthYear.Month] +
                           " " + WorkMonthYear.Year + " г. " +
                           "Вы не сможете провести корректировку данного месяца.\n" +
                           "Желаете продолжить?";
            var result = BuildMessageBox.GetConfirmMessageBox(messages);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var monthClose = _context.MonthCloses.First(i => i.WorkGuildId == selectedWorkGuild.Id
                                                             && i.Month == WorkMonthYear.Month
                                                             && i.Year == WorkMonthYear.Year);
            monthClose.IsClose = true;
            _context.SaveChanges();

            BuildMessageBox.GetInformationMessageBox("Месяц успешно закрыт.");
        }
        #endregion 
        #region Корректировка: кнопка [Провести доработку]
        private ICommand _addReworkCommand;
        public ICommand AddReworkCommand => _addReworkCommand ?? (_addReworkCommand = new RelayCommand(arg => AddRework()));

        private void AddRework()
        {
            var selectedWorkGuild = Login;
            if (Login.Lvl == 1)
            {
                var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
                builder.SetMessage("Укажите цех которому провести доработку.");
                builder.SetIsWorkGuild(true);
                var parametrsViewModel = builder.GetParametrsViewModel();
                var parametersWindow = new ParametersWindow()
                {
                    DataContext = parametrsViewModel,
                    Owner = Common.GetOwnerWindow()
                };
                parametersWindow.ShowDialog();

                if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
                {
                    return;
                }

                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }
            var selectedMonthYear = new MonthYear(_selectedMonth, _selectedYear);
            TaskInProgress = true;
            Task.Run(() => { DisplayViewModel = new AddReworkViewModel(this, selectedWorkGuild, selectedMonthYear); })
                .ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Корректировка: кнопка [Добавить замены деталей]
        private ICommand _requestsCommand;
        public ICommand RequestsCommand => _requestsCommand ?? (_requestsCommand = new RelayCommand(arg => Requests()));

        private void Requests()
        {
            //var selectedWorkGuild = Login;

            //var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            //builder.SetIsMonthYear(true);
            //if (Login.Lvl == 1)
            //{
            //    builder.SetMessage("Укажите цех, месяц и год");
            //    builder.SetIsWorkGuild(true);
            //}
            //else
            //{
            //    builder.SetMessage("Укажите месяц и год");
            //}
            //var parametrsViewModel = builder.GetParametrsViewModel();
            //var parametersWindow = new ParametersWindow()
            //{
            //    DataContext = parametrsViewModel,
            //    Owner = Common.GetOwnerWindow()
            //};
            //parametersWindow.ShowDialog();
            //if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            //{
            //    return;
            //}

            //var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            //if (Login.Lvl == 1)
            //{
            //    selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            //}

            //TaskInProgress = true;
            //Task.Run(() =>
            //    {
            //        var requestsReplacementsViewModel = new RequestsReplacementsViewModel(this, selectedWorkGuild, selectedMonthYear);
            //        HotkeysText = string.Empty;
            //        DisplayViewModel = requestsReplacementsViewModel;
            //    }).ContinueWith(task => { TaskInProgress = false; });

            HotkeysText = string.Empty;
            DisplayViewModel = new AddRequestReplacementsViewModel(this);
        }
        #endregion

        #region Отчеты: кнопка [Баланс]
        private RelayCommand _balanceReportCommand;
        public ICommand BalanceReportCommand => _balanceReportCommand ??
                                                (_balanceReportCommand = new RelayCommand(arg => BalanceReport()));
        public void BalanceReport()
        {
            var selectedWorkGuild = Login;

            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsMonthYear(true);
            builder.SetIsTmcType(true);
            if (Login.Lvl == 1)
            {
                builder.SetMessage("Укажите цех, месяц, год и вид");
                builder.SetIsWorkGuild(true);
            }
            else
            {
                builder.SetMessage("Укажите месяц, год и вид");
            }
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            if (Login.Lvl == 1)
            {
                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }
            var selectedTmcType = parametrsViewModel.SelectedTmcType;

            TaskInProgress = true;
            Task.Run(() =>
            {
                const string heading = "Баланс";
                const string reportType = "Balance";
                const string reportFileName = "Balance.rdlc";
                var reportFile = Common.GetReportFilePath(reportFileName);

                List<Pkom> resultReportPkomList;
                using (var context = new DomainContext())
                {
                    resultReportPkomList =
                        Pkom.GetPkom(context, selectedMonthYear, selectedWorkGuild, selectedTmcType);
                }
                HotkeysText = string.Empty;
                DisplayViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                    selectedWorkGuild, selectedMonthYear, selectedTmcType, "-", "-", resultReportPkomList);
            }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Отчеты: кнопка [Баланс(короткий)]
        private RelayCommand _balanceShortReportCommand;
        public ICommand BalanceShortReportCommand => _balanceShortReportCommand ??
                                                (_balanceShortReportCommand = new RelayCommand(arg => BalanceShortReport()));
        public void BalanceShortReport()
        {
            var selectedWorkGuild = Login;

            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetIsMonthYear(true);
            builder.SetIsTmcType(true);
            if (Login.Lvl == 1)
            {
                builder.SetMessage("Укажите цех, месяц, год и вид");
                builder.SetIsWorkGuild(true);
            }
            else
            {
                builder.SetMessage("Укажите месяц, год и вид");
            }
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            if (Login.Lvl == 1)
            {
                selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            }
            var selectedTmcType = parametrsViewModel.SelectedTmcType;

            TaskInProgress = true;
            Task.Run(() =>
            {
                const string heading = "Баланс(короткий)";
                const string reportType = "Balance";
                const string reportFileName = "Balance.rdlc";
                var reportFile = Common.GetReportFilePath(reportFileName);

                List<Pkom> resultReportPkomList;
                using (var context = new DomainContext())
                {
                    resultReportPkomList =
                        Pkom.GetPkom(context, selectedMonthYear, selectedWorkGuild, selectedTmcType);
                }
                HotkeysText = string.Empty;
                DisplayViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                    selectedWorkGuild, selectedMonthYear, selectedTmcType, "-", "+", resultReportPkomList);
            }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Отчеты: кнопка [Выпуск изделий]
        private RelayCommand _releaseProductsReportCommand;
        public ICommand ReleaseProductsReportCommand => _releaseProductsReportCommand ??
                                                        (_releaseProductsReportCommand =
                                                            new RelayCommand(obj => ReleaseProductsReport()));
        private void ReleaseProductsReport()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetMessage("Укажите цех, месяц и год");
            builder.SetIsWorkGuildOrAll(true);
            builder.SetIsMonthYear(true);
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            var selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;

            TaskInProgress = true;
            Task.Run(() =>
                {
                    const string heading = "Выпуск изделий";
                    const string reportType = "ReleaseProductReport";
                    const string reportFileName = "ReleaseProducts.rdlc";

                    var reportFile = Common.GetReportFilePath(reportFileName);

                    List<ReleaseProductReport> resultReportList;
                    using (var context = new DomainContext())
                    {
                        resultReportList =
                            ReleaseProductReport.GetReleaseProductReport(context, selectedWorkGuild, selectedMonthYear);
                    }
                    HotkeysText = string.Empty;
                    DisplayViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                        selectedWorkGuild, selectedMonthYear, resultReportList);
                }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Отчеты: кнопка [Выпуск]
        private RelayCommand _releaseReportCommand;
        public ICommand ReleaseReportCommand => _releaseReportCommand ??
                                                        (_releaseReportCommand =
                                                            new RelayCommand(obj => ReleaseReport()));

        private void ReleaseReport()
        {
            var builder = new MainParametrsBuilder(new MonthYear(SelectedMonth, SelectedYear));
            builder.SetMessage("Укажите месяц и год");
            builder.SetIsMonthYear(true);
            builder.SetIsWorkGuildOrAll(true);
            var parametrsViewModel = builder.GetParametrsViewModel();
            var parametersWindow = new ParametersWindow()
            {
                DataContext = parametrsViewModel,
                Owner = Common.GetOwnerWindow()
            };
            parametersWindow.ShowDialog();
            if (!parametersWindow.DialogResult.HasValue || parametersWindow.DialogResult != true)
            {
                return;
            }

            var selectedMonthYear = new MonthYear(parametrsViewModel.SelectedMonth, parametrsViewModel.SelectedYear);
            var selectedWorkGuild = parametrsViewModel.SelectedWorkGuild;
            TaskInProgress = true;
            Task.Run(() =>
            {
                const string heading = "Выпуск";
                const string reportType = "ReleaseReport";
                const string reportFileName = "Release.rdlc";

                var reportFile = Common.GetReportFilePath(reportFileName);

                var resultReportList = Model.ReleaseReport.GetReleases(_context, selectedMonthYear, selectedWorkGuild);
                HotkeysText = string.Empty;
                DisplayViewModel = new ReportsViewModel(heading, reportType, reportFileName, reportFile,
                    selectedMonthYear, selectedWorkGuild, resultReportList);
            }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion   
        #endregion

        [Obsolete("Only needed for serialization and materialization", true)]
        public MainViewModel()
        {
        }

        public MainViewModel(WorkGuild login)
        {
            Login = login;
            IsVisibleLvl1 = Login.Lvl == 1;

            WorkMonthYear = _context.MonthYears.First(i => i.Description == "Текущий рабочий месяц");
            HotkeysText = string.Empty;
            MonthDictionary = Constants.MonthsFullNames;
            SelectedMonth = WorkMonthYear.Month;
            SelectedYear = WorkMonthYear.Year;
        }
    }
}