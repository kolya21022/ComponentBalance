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
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.ViewModel
{
    public class EditCompositionProductViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();

        private ObservableCollection<CompositionProduct> _compositionProductsCollection;
        private ICollectionView _displayCompositionProducts;
        #endregion
        #region Public Fields
        public Product Product { get; }
        public ObservableCollection<CompositionProduct> CompositionProductsCollection
        {
            get { return _compositionProductsCollection; }
            set
            {
                _compositionProductsCollection = value;
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

        public string FilterHotKey { get; }
        #endregion

        #region Commands
        private RelayCommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(obj => Save()));
        private void Save()
        {
            var result = BuildMessageBox.GetConfirmMessageBox("Вы хотите сохранить изменения?");
            if (result != MessageBoxResult.Yes) { return; }

            _context.SaveChanges();
            BuildMessageBox.GetInformationMessageBox("Сохранение успешно произведено.");
        }
        #endregion

        public EditCompositionProductViewModel(WorkGuild workGuild, Product product)
        {
            Product = product;

            CompositionProductsCollection = new ObservableCollection<CompositionProduct>(
                _context.CompositionProducts
                    .Where(i => i.ProductId == product.Id
                                && i.WorkGuildId == workGuild.Id)
                    .Include(i => i.Detail)
                    .Include(i => i.Detail.Measure)
                    .Include(i => i.Detail.TmcType));

            FilterHotKey = Constants.FilterBarCoverLabel;
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
            var viewSourceCompositionProductsCollectionView = CollectionViewSource.GetDefaultView(CompositionProductsCollection);
            viewSourceCompositionProductsCollectionView.Filter = filterCriterias.IsEmpty ? null : predicate;
            DisplayCompositionProducts = viewSourceCompositionProductsCollectionView;
        }

        /// <summary>
        /// Метод-предикат (булевый) текущей записи коллекции сущностей, который возвращает true или 
        /// false в зависимости от попадания в диапазон фильтра по всем полям фильтрации.
        /// </summary>
        private bool MapFilterPredicate(object rawEntity)
        {
            var entity = (CompositionProduct)rawEntity;
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
            return result;
        }
        #endregion
    }
}
