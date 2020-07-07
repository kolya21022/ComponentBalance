using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ComponentBalanceSqlv2.Commands;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Data.Moves;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Model.ParametrsBuilder;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.View.Windows;

namespace ComponentBalanceSqlv2.ViewModel
{
   public class ReleaseViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly MainViewModel _mainViewModel;
        private readonly DomainContext _context = new DomainContext();
        private readonly WorkGuild _login;
        private readonly bool _isClose;
        
        private readonly WorkGuild _workGuild;
        private readonly MonthYear _monthYear;

        private ObservableCollection<ReleaseProduct> _releaseProductCollection;
        private ReleaseProduct _selectedReleaseProduct;
        private ObservableCollection<ReleaseProduct> _filterReleaseProductList;

        private ReleaseMove _selectedReleaseMove;
        private EquipmentMove _selectedEquipmentMove;

        private string _searchTextProduct;
        private bool _taskInProgress;
        #endregion

        #region Public Fields
        public ObservableCollection<ReleaseProduct> FilterReleaseProductList
        {
            get { return _filterReleaseProductList; }
            set
            {
                _filterReleaseProductList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReleaseProduct> ReleaseProductCollection
        {
            get { return _releaseProductCollection; }
            set
            {
                _releaseProductCollection = value;
                FilterReleaseProductList = value;
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

        public ReleaseMove SelectedReleaseMove
        {
            get { return _selectedReleaseMove; }
            set
            {
                _selectedReleaseMove = value;
                OnPropertyChanged();
            }
        }
        public EquipmentMove SelectedEquipmentMove
        {
            get { return _selectedEquipmentMove; }
            set
            {
                _selectedEquipmentMove = value;
                OnPropertyChanged();
            }
        }

        public string SearchTextProduct
        {
            get { return _searchTextProduct; }
            set
            {
                _searchTextProduct = value;
                Search(value);
                OnPropertyChanged();
            }
        }

        public bool TaskInProgress
        {
            get { return _taskInProgress; }
            set
            {
                _taskInProgress = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Command
        #region Кнопка: [Добавить расход]
        private RelayCommand _addReleaseCommand;
        public ICommand AddReleaseCommand => _addReleaseCommand ?? (_addReleaseCommand = new RelayCommand(obj => AddRelease(), obj => !_isClose));

        private void AddRelease()
        {
            var builder = new MainParametrsBuilder(_monthYear);
            builder.SetIsPartRelease(true);
            builder.SetMessage("Укажите какую часть вы хотите ввести");
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

            var selectedPartRelease = parametrsViewModel.SelectedPartRelease;

            TaskInProgress = true;
            Task.Run(() =>
            {
                var addReleaseViewModel =
                    new AddReleaseViewModel(_mainViewModel, _login, _workGuild, _monthYear, selectedPartRelease);
                _mainViewModel.HotkeysText =
                    "[Num0] -> Поставить/Снять ☑;   [PageDown] -> Переход между Деталь и Замена;    [RCtrl] -> Нажать MAX в замене";
                _mainViewModel.DisplayViewModel = addReleaseViewModel;
            }).ContinueWith(task => { TaskInProgress = false; });
        }

        #endregion
        #region Кнопка: [Изменить выпуск]
        private RelayCommand _editReleaseCommand;
        public ICommand EditReleaseCommand => _editReleaseCommand ?? (_editReleaseCommand = new RelayCommand(obj => DoEditRelease(), obj => !_isClose));

        private void DoEditRelease()
        {
            if (SelectedReleaseProduct == null)
            {
                return;
            }

            TaskInProgress = true;
            Task.Run(() =>
            {
                var addReleaseViewModel = new AddReleaseViewModel(_mainViewModel, _login, _workGuild, _monthYear, SelectedReleaseProduct);
                _mainViewModel.HotkeysText =
                    "[Num0] -> Поставить/Снять ☑;   [PageDown] -> Переход между Деталь и Замена;    [RCtrl] -> Нажать MAX в замене";
                _mainViewModel.DisplayViewModel = addReleaseViewModel;
            }).ContinueWith(task => { TaskInProgress = false; });
        }
        #endregion
        #region Кнопка: [Удалить расход]
        private RelayCommand _deleteReleaseMoveCommand;
        public ICommand DeleteReleaseMoveCommand => _deleteReleaseMoveCommand ?? (_deleteReleaseMoveCommand = new RelayCommand(obj => DeleteReleaseMove()));

        private void DeleteReleaseMove()
        {
            if (SelectedReleaseMove == null)
            {
                return;
            }

            if (_login.Lvl != 1)
            {
                BuildMessageBox.GetInformationMessageBox("У вас недостаточно прав для этого");
                return;
            }

            var result = BuildMessageBox.GetConfirmMessageBox("Вы уверены?");
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SelectedReleaseProduct.DeleteMove(_context, SelectedReleaseMove.BalanceId, SelectedReleaseMove);
            _context.SaveChanges();

            // TODO: исправить отображение datagrid и подсчет сумм
            BuildMessageBox.GetInformationMessageBox(
                "Удалено.\n Нажмите на шапку любого столбца, что бы обновить отображение.");
        }
        #endregion
        #region Кнопка: [Удалить докомплектацию]
        private RelayCommand _deleteEquipmentMoveCommand;
        public ICommand DeleteEquipmentMoveCommand => _deleteEquipmentMoveCommand ?? (_deleteEquipmentMoveCommand = new RelayCommand(obj => DeleteEquipmentMove(), obj => !_isClose));

        private void DeleteEquipmentMove()
        {
            if (SelectedEquipmentMove == null)
            {
                return;
            }

            var result = BuildMessageBox.GetConfirmMessageBox("Вы уверены?");
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SelectedReleaseProduct.DeleteMove(_context, SelectedEquipmentMove.BalanceId, SelectedEquipmentMove);
            _context.SaveChanges();

            // TODO: исправить отображение datagrid и подсчет сумм
            BuildMessageBox.GetInformationMessageBox(
                "Удалено.\n Нажмите на шапку любого столбца, что бы обновить отображение.");
        }
        #endregion
        #region Кнопка: [Удалить выпуск весь/часть]
        private RelayCommand _deleteReleaseCommand;
        public ICommand DeleteReleaseCommand => _deleteReleaseCommand ?? (_deleteReleaseCommand = new RelayCommand(obj => DoDeleteRelease(), obj => !_isClose));

        private void DoDeleteRelease()
        {
            if (SelectedReleaseProduct == null)
            {
                return;
            }

            var builder = new MainParametrsBuilder(_monthYear);
            builder.SetIsPartRelease(true);
            builder.SetMessage("Укажите какую часть вы хотите ввести");
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
            var selectedPartRelease = parametrsViewModel.SelectedPartRelease;

            MessageBoxResult result;
            switch (selectedPartRelease)
            {
                case PartRelease.All:
                    result = BuildMessageBox.GetConfirmMessageBox($"Вы хотите удалить введенный выпуск {SelectedReleaseProduct.FactoryNumber}\n" +
                                                                  "Желаете продолжить?");
                    break;
                case PartRelease.Kom:
                    result = BuildMessageBox.GetConfirmMessageBox($"Вы хотите удалить введенные КОМПЛЕКТУЮЩИЕ выпуска {SelectedReleaseProduct.FactoryNumber}\n" +
                                                                  "Желаете продолжить?");
                    break;
                case PartRelease.Vsp:
                    result = BuildMessageBox.GetConfirmMessageBox($"Вы хотите удалить введенные ВСПОМОГАТЕЛЬНЫЕ выпуска {SelectedReleaseProduct.FactoryNumber}\n" +
                                                                  "Желаете продолжить?");
                    break;
                default:
                    result = MessageBoxResult.No;
                    break;
            }

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            switch (selectedPartRelease)
            {
                case PartRelease.All:
                    DeleteRelease();
                    return;
                case PartRelease.Kom:
                    DeleteKomRelease();
                    break;
                case PartRelease.Vsp:
                    DeleteVspRelease();
                    break;
                default:
                    return;
            }
        }

        // TODO: progressbar 
        private void DeleteRelease()
        {
            SelectedReleaseProduct.Delete(_context);   
            _context.SaveChanges();

            ReleaseProductCollection.Remove(ReleaseProductCollection.First(i => i.FactoryNumber == SelectedReleaseProduct.FactoryNumber));
        }
        #endregion
        #region Кнопка: [Удалить комплектующие]
        private RelayCommand _deleteKomReleaseCommand;
        public ICommand DeleteKomReleaseCommand => _deleteKomReleaseCommand ?? (_deleteKomReleaseCommand = new RelayCommand(obj => DeleteKomRelease(), obj => !_isClose));

        // TODO: progressbar 
        private void DeleteKomRelease()
        {
            SelectedReleaseProduct.DeleteReleaseKom(_context);
            _context.SaveChanges();

            // TODO: исправить отображение datagrid и подсчет сумм
            BuildMessageBox.GetInformationMessageBox(
                "Удалено.\n Нажмите на шапку любого столбца, что бы обновить отображение.");
        }
        #endregion
        #region Кнопка: [Удалить вспомогательные]
        private RelayCommand _deleteVspReleaseCommand;
        public ICommand DeleteVspReleaseCommand => _deleteVspReleaseCommand ?? (_deleteVspReleaseCommand = new RelayCommand(obj => DeleteVspRelease(), obj => !_isClose));

        // TODO: progressbar 
        private void DeleteVspRelease()
        {
            SelectedReleaseProduct.DeleteReleaseVsp(_context);
            _context.SaveChanges();

            // TODO: исправить отображение datagrid и подсчет сумм
            BuildMessageBox.GetInformationMessageBox(
                "Удалено.\n Нажмите на шапку любого столбца, что бы обновить отображение.");
        }
        #endregion
        #endregion

        public ReleaseViewModel(MainViewModel mainViewModel, WorkGuild login, WorkGuild workGuild, MonthYear monthYear)
        {
            _mainViewModel = mainViewModel;
            _login = login;
            _workGuild = workGuild;
            _monthYear = monthYear;

            ReleaseProductCollection = new ObservableCollection<ReleaseProduct>(
                _context.ReleaseProducts
                    .Where(i => i.WorkGuildId == _workGuild.Id
                                && i.Month == _monthYear.Month
                                && i.Year == _monthYear.Year)
                    .Include(c => c.Product) // Продукт
                    .Include(c => c.ReleaseMoves // Выпуск и вложение до тмц
                        .Select(x => x.Balance.Detail.TmcType))
                    .Include(c => c.ReleaseMoves // Выпуск и вложение до ед изм
                        .Select(x => x.Balance.Detail.Measure))
                    .Include(c => c.EquipmentMoves // Комплектация и вложение до тмц
                        .Select(x => x.Balance.Detail.TmcType))
                    .Include(c => c.EquipmentMoves // Комплектация и вложение до ед изм
                        .Select(x => x.Balance.Detail.Measure))
                
                    .Include(c => c.ReleaseReplacementDetails));

            SelectedReleaseProduct = FilterReleaseProductList.FirstOrDefault();


            if (_login.Lvl == 1)
            {
                _isClose = false;
            }
            else
            {
                var monthCloses = _context.MonthCloses.FirstOrDefault(i => i.Month == _monthYear.Month
                                                                           && i.Year == _monthYear.Year
                                                                           && i.WorkGuildId == _workGuild.Id);
                _isClose = monthCloses?.IsClose ?? true;
            }
        }

        private void Search(string value)
        {
            FilterReleaseProductList = new ObservableCollection<ReleaseProduct>();
            if (value.Trim() == string.Empty)
            {
                FilterReleaseProductList = new ObservableCollection<ReleaseProduct>(ReleaseProductCollection);
                return;
            }

            // Разделение введенного пользователем текста по пробелам на массив слов
            var searchValues = value.Trim().Split(null);
            const StringComparison comparisonIgnoreCase = StringComparison.OrdinalIgnoreCase;


            foreach (var releaseProduct in ReleaseProductCollection)
            {
                // Поиск совпадений всех значений массива по требуемым полям сущности
                var isCoincided = true;

                var factoryNumber = releaseProduct.FactoryNumber.ToString(CultureInfo.CurrentCulture);
                var productCode = releaseProduct.Product.Code.ToString(CultureInfo.CurrentCulture);

                foreach (var searchValue in searchValues)
                {
                    isCoincided &= factoryNumber.IndexOf(searchValue, comparisonIgnoreCase) >= 0
                                   || productCode.IndexOf(searchValue, comparisonIgnoreCase) >= 0;
                }

                // Если в полях сущности есть введённые слова, добавляем объект в буферный список
                if (isCoincided)
                {
                    FilterReleaseProductList.Add(releaseProduct);
                }
            }
        }
    }
}
