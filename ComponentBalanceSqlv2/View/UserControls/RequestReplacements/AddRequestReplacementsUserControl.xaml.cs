using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.ViewModel.RequestReplacements;

namespace ComponentBalanceSqlv2.View.UserControls.RequestReplacements
{
    public partial class AddRequestReplacementsUserControl
    {
        public AddRequestReplacementsUserControl()
        {
            InitializeComponent();
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
                    var row = (DataGridRow)searchDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                    if (row == null)
                    {
                        searchDataGrid.UpdateLayout();
                        searchDataGrid.ScrollIntoView(searchDataGrid.Items[0]);
                        row = (DataGridRow)searchDataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                    }

                    row?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
                case Key.Enter:
                    // Перевод фокуса ввода на следующий визуальный элемент после [DataGrid] поиска сущности
                    var nextControlAfterDataGrid = searchDataGrid.PredictFocus(FocusNavigationDirection.Down) as Control;
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
                if (selectedItem.BaseType == typeof(Detail))
                {
                    var item = (Detail)rawSelectedItem;
                    var viewModel = (AddRequestReplacementsViewModel)DataContext;

                    if (searchDataGrid.Name == "FirstSearchDataGrid")
                    {
                        viewModel.SelectedDetailFirst = viewModel.DetailsList.FirstOrDefault(i => i.Id == item.Id);
                    }
                    else
                    {
                        viewModel.SelectedDetailSecond = viewModel.DetailsList.FirstOrDefault(i => i.Id == item.Id);
                    }
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
            if (selectedItem.BaseType == typeof(Detail))
            {
                var item = (Detail)rawSelectedItem;
                var viewModel = (AddRequestReplacementsViewModel)DataContext;

                if (searchDataGrid.Name == "FirstSearchDataGrid")
                {
                    viewModel.SelectedDetailFirst = viewModel.DetailsList.FirstOrDefault(i => i.Id == item.Id);
                }
                else
                {
                    viewModel.SelectedDetailSecond = viewModel.DetailsList.FirstOrDefault(i => i.Id == item.Id);
                }
            }
        }

    }
}
