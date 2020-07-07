using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.ViewModel;
using Visual = ComponentBalanceSqlv2.Utils.Styles.Visual;

namespace ComponentBalanceSqlv2.View.UserControls
{
    /// <summary>
    /// Логика взаимодействия для AddReleaseUserControl.xaml
    /// </summary>
    public partial class AddReleaseUserControl : UserControl
    {
        public AddReleaseUserControl()
        {
            InitializeComponent();
            FontSize = Visual.FontSize;
        }

        private void SearchTextBox_OnPreviewKeyUp(object senderIsTextBox, KeyEventArgs eventArgs)
        {
            // TextBox поиска
            var searchTextBox = senderIsTextBox as TextBox;
            if (searchTextBox == null)
            {
                return;
            }

            // Grid-обёртка DataGrid и TextBox поиска
            var searchWrapperGrid = VisualTreeHelper.GetParent(searchTextBox) as Grid;
            // DataGrid поиска сущности
            var searchDataGrid = searchWrapperGrid?.Children.OfType<DataGrid>().FirstOrDefault();
            if (searchDataGrid == null)
            {
                return;
            }

            // Если нажата кнопка [Down] - перемещение клавиатурного фокуса на первую строку DataGrid поиска сущности
            switch (eventArgs.Key)
            {
                case Key.Down:
                    if (searchDataGrid.Items.Count <= 0)
                    {
                        return;
                    }

                    searchDataGrid.SelectedIndex = 0;

                    // NOTE: эта копипаста ниже не случайна, нужный функционал срабатывает только со второго раза.
                    // Решение в указаном ответе: https://stackoverflow.com/a/27792628 Работает, не трогай
                    var row = (DataGridRow) searchDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                    if (row == null)
                    {
                        searchDataGrid.UpdateLayout();
                        searchDataGrid.ScrollIntoView(searchDataGrid.Items[0]);
                        row = (DataGridRow) searchDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                    }

                    row?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
                case Key.Enter:
                    // Перевод фокуса ввода на следующий визуальный элемент после [DataGrid] поиска сущности
                    var nextControlAfterDataGrid =
                        searchDataGrid.PredictFocus(FocusNavigationDirection.Down) as Control;
                    if (nextControlAfterDataGrid == null)
                    {
                        return;
                    }

                    eventArgs.Handled = true;
                    nextControlAfterDataGrid.Focus();
                    break;
            }
        }

        private void SearchDataGrid_OnPreviewKeyUp(object senderIsDataGrid, KeyEventArgs eventArgs)
        {
            const int startOfListIndex = 0;
            // DataGrid поиска сущности
            var searchDataGrid = senderIsDataGrid as DataGrid;
            if (searchDataGrid == null)
            {
                return;
            }

            // Grid-обёртка DataGrid и TextBox поиска
            var searchWrapperGrid = VisualTreeHelper.GetParent(searchDataGrid) as Grid;
            // TextBox поиска/добавления
            var searchTextBox = searchWrapperGrid?.Children.OfType<TextBox>().FirstOrDefault();
            if (searchTextBox == null)
            {
                return;
            }

            // Если фокус ввода на первой записи DataGrid и нажата [Up] - перевод клавиатурного фокуса ввода к TextBox
            if (startOfListIndex == searchDataGrid.SelectedIndex && eventArgs.Key == Key.Up)
            {
                searchTextBox.Focus();
            }

            // Если записей не 0 и нажат [Enter] - заносим текст объекта в TextBox и переводим фокус к след. контролу
            else if (searchDataGrid.Items.Count > 0 && eventArgs.Key == Key.Enter)
            {
                // Выбранная строка (объект) DataGrid поиска сущности
                var rawSelectedItem = searchDataGrid.SelectedItem;
                if (rawSelectedItem == null)
                {
                    return;
                }

                var selectedItem = rawSelectedItem.GetType();
                if (selectedItem.BaseType == typeof(Product))
                {
                    var item = (Product) rawSelectedItem;
                    var viewModel = (AddReleaseViewModel) DataContext;

                    viewModel.SelectedProduct = viewModel.ProductsList.FirstOrDefault(i => i.Id == item.Id);
                }
            }
        }

        private void SearchDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Enter)
            {
                eventArgs.Handled = true;
            }
        }

        private void SearchDataGrid_OnPreviewMouseDown(object senderIsDataGrid, MouseButtonEventArgs eventArgs)
        {
            // DataGrid поиска сущности
            var searchDataGrid = senderIsDataGrid as DataGrid;
            if (searchDataGrid == null)
            {
                return;
            }

            // Grid-обёртка DataGrid и TextBox поиска
            var searchWrapperGrid = VisualTreeHelper.GetParent(searchDataGrid) as Grid;
            // TextBox поиска/добавления
            var searchTextBox = searchWrapperGrid?.Children.OfType<TextBox>().FirstOrDefault();
            if (searchTextBox == null)
            {
                return;
            }

            // Выбранная строка (объект) DataGrid поиска сущности
            var rawSelectedItem = searchDataGrid.SelectedItem;
            if (rawSelectedItem == null)
            {
                return;
            }

            var selectedItem = rawSelectedItem.GetType();
            if (selectedItem.BaseType == typeof(Product))
            {
                var item = (Product) rawSelectedItem;
                var viewModel = (AddReleaseViewModel) DataContext;

                viewModel.SelectedProduct = viewModel.ProductsList.FirstOrDefault(i => i.Id == item.Id);
            }
        }

        /// <summary>
        /// Нажатие клавиши в контексном меню - исправление дефекта скрытия фильтра при переключении раскладки ввода
        /// </summary>
        private void PopupFilterContextMenu_OnKeyDown(object senderIsMenuItem, KeyEventArgs eventArgs)
        {
            var key = eventArgs.Key;
            eventArgs.Handled = key == Key.System || key == Key.LeftAlt || key == Key.RightAlt;
        }

        /// <summary>
        /// Подстановка имени столбца при открытии контексного меню фильтрации DataGrid, 
        /// установка Tag и перемещение фокуса ввода в поле
        /// </summary>
        private void PageDataGrid_OnContextMenuOpening(object senderIsDataGrid, ContextMenuEventArgs eventArgs)
        {
            UserControlUtil.Service_PageDataGridWithFilterContextMenuOpening(senderIsDataGrid);
        }

        private void CompositionsDataGrid_OnPreviewKeyUp(object sender, KeyEventArgs eventArgs)
        {
            var viewModel = (AddReleaseViewModel)DataContext;

            switch (eventArgs.Key)
            {
                case Key.NumPad0:
                    var selectedCompositionProduct = viewModel.SelectedCompositionProduct;
                    if (selectedCompositionProduct != null)
                    {
                        selectedCompositionProduct.IsSelectedInRelease =
                            !selectedCompositionProduct.IsSelectedInRelease;
                    }
                    eventArgs.Handled = true;
                    break;
                case Key.PageDown:
                    if (ReplacementsDataGrid.Items.Count > 0)
                    {
                        viewModel.SelectedReplacementDetail = (ReplacementDetail)ReplacementsDataGrid.Items[0];
                        UserControlUtil.SetFocusOnSelectedRowInDataGrid(ReplacementsDataGrid);
                    }
                    eventArgs.Handled = true;
                    break;            
                case Key.PageUp:
                    eventArgs.Handled = true;
                    break;
            }
        }

        private void CompositionsDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {        
            switch (eventArgs.Key)
            {
                case Key.PageDown:
                    eventArgs.Handled = true;
                    break;
                case Key.PageUp:
                    eventArgs.Handled = true;
                    break;
            }
        }

        private void ReplacementsDataGrid_OnPreviewKeyUp(object sender, KeyEventArgs eventArgs)
        {
            var viewModel = (AddReleaseViewModel)DataContext;
            if (viewModel.SelectedReplacementDetail == null)
            {
                return;
            }

            switch (eventArgs.Key)
            {
                case Key.RightCtrl:
                    viewModel.MaxSelectedReplacementCommand.Execute(true);
                    eventArgs.Handled = true;
                    break;
                case Key.PageDown:
                    if (CompositionsDataGrid.Items.Count > 0)
                    {
                        var rowReplacement = ReplacementsDataGrid.ItemContainerGenerator.ContainerFromItem(viewModel
                            .SelectedReplacementDetail) as DataGridRow;
                        if (rowReplacement != null)
                        {
                            rowReplacement.IsSelected = false;
                        }

                        UserControlUtil.SetFocusOnSelectedRowInDataGrid(CompositionsDataGrid);
                    }

                    eventArgs.Handled = true;
                    break;
                case Key.PageUp:
                    eventArgs.Handled = true;
                    break;
            }
        }

        private void ReplacementsDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            var viewModel = (AddReleaseViewModel)DataContext;

            switch (eventArgs.Key)
            {
                case Key.PageDown:
                    eventArgs.Handled = true;
                    break;
                case Key.PageUp:
                    eventArgs.Handled = true;
                    break;
                case Key.Down:
                    if (viewModel.SelectedReplacementDetail == ReplacementsDataGrid.Items[ReplacementsDataGrid.Items.Count-1])
                    {
                        eventArgs.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (viewModel.SelectedReplacementDetail == ReplacementsDataGrid.Items[0])
                    {
                        eventArgs.Handled = true;
                    }
                    break;
            }
        }
    }
}
