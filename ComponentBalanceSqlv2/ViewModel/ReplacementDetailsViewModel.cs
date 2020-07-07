using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class ReplacementDetailsViewModel : BaseNotificationClass
    {
        #region Private Fields
        private readonly DomainContext _context = new DomainContext();
        private ICollectionView _displayReplacementDetails;
        #endregion
        #region Public Fields
        public List<ReplacementDetail> ReplacementDetailsCollection { get; }

        public ICollectionView DisplayReplacementDetails
        {
            get { return _displayReplacementDetails; }
            set
            {
                _displayReplacementDetails = value;
                OnPropertyChanged();
            }
        }

        public string FilterHotKey { get; }
        #endregion

        public ReplacementDetailsViewModel()
        {
            ReplacementDetailsCollection = _context.ReplacementDetails.AsNoTracking().ToList();
            ReloadDisplayItems(FilterCriterias, MapFilterPredicate);
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
            var viewSourceReplacementDetailsCollectionView = CollectionViewSource.GetDefaultView(ReplacementDetailsCollection);
            viewSourceReplacementDetailsCollectionView.Filter = filterCriterias.IsEmpty ? null : predicate;
            DisplayReplacementDetails = viewSourceReplacementDetailsCollectionView;
        }

        /// <summary>
        /// Метод-предикат (булевый) текущей записи коллекции сущностей, который возвращает true или 
        /// false в зависимости от попадания в диапазон фильтра по всем полям фильтрации.
        /// </summary>
        private bool MapFilterPredicate(object rawEntity)
        {
            var entity = (ReplacementDetail)rawEntity;
            if (FilterCriterias.IsEmpty)
            {
                return true;
            }
            var result = true;

            // Проверка наличия полей сущности в критериях фильтрации и содержит ли поле искомое значение фильтра
            // Если в фильтре нет поля сущности, поле считается совпадающим по критерию
            string buffer;
            var filter = FilterCriterias;
            result &= !filter.GetValue("ShortProductCode", out buffer)
                      || FilterCriterias.ContainsDecimal(entity.ShortProductCode, buffer);
            result &= !filter.GetValue("DetailStartCode", out buffer)
                      || FilterCriterias.ContainsDecimal(entity.DetailStartCode, buffer);
            result &= !filter.GetValue("DetailStartName", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailStartName, buffer);
            result &= !filter.GetValue("DetailStartDesignation", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailStartDesignation, buffer);
            result &= !filter.GetValue("DetailStartGost", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailStartGost, buffer);
            result &= !filter.GetValue("DetailEndCode", out buffer)
                      || FilterCriterias.ContainsDecimal(entity.DetailEndCode, buffer);
            result &= !filter.GetValue("DetailEndName", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailEndName, buffer);
            result &= !filter.GetValue("DetailEndDesignation", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailEndDesignation, buffer);
            result &= !filter.GetValue("DetailEndGost", out buffer)
                      || FilterCriterias.ContainsLine(entity.DetailEndGost, buffer);
            result &= !filter.GetValue("Cause", out buffer)
                      || FilterCriterias.ContainsLine(entity.Cause, buffer);

            return result;
        }
        #endregion
    }
}
