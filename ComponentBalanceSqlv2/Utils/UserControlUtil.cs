using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ComponentBalanceSqlv2.Utils
{
    /// <summary>
    /// Утилитарные методы UserControl
    /// </summary>
    public class UserControlUtil
    {
        /// <summary>
        /// Попытка установки клавиатурного фокуса ввода ассинхронно в отдельном потоке.
        /// Требуется в некоторых случаях, когда обычный вызов Focus не срабатывает.
        /// Источник: https://stackoverflow.com/a/3771441
        /// </summary>
        private static void SetElementFocusInOtherThread(UIElement element)
        {
            if (!element.Focus())
            {
                element.Dispatcher.BeginInvoke(DispatcherPriority.Input,
                    new ThreadStart(delegate { element.Focus(); }));
            }
        }

        /// <summary>
		/// Сервисный метод фильтрующего главного DataGrid страницы, при открытии контексного меню фильтрации.
		/// Подставляет название столбца в контекстное меню, устанавливает Tag и перемещение фокуса ввода в поле ввода.
		/// Метод работает только с указанной компоновкой элементов, не привязываясь к конкретным объктам страниц:
		/// DataGrid -> ContextMenu -> MenuItem -> MenuItem.Header -> Grid -> [TextBlock + TextBox + Button]
		/// </summary>
		public static void Service_PageDataGridWithFilterContextMenuOpening(object senderIsDataGrid)
        {
            var dataGrid = senderIsDataGrid as DataGrid;   // Главный DataGrid страницы
            if (dataGrid == null)
            {
                return;
            }
            var contextMenu = dataGrid.ContextMenu;        // Контекстное меню этого DataGrid
            if (contextMenu == null)
            {
                return;
            }
            if (dataGrid.Items.Count == 0)
            {
                contextMenu.Visibility = Visibility.Collapsed;
                return;
            }

            // Для получения текущего столбца требуется переключить режим выделения DataGrid со строки на ячейку
            dataGrid.SelectionUnit = DataGridSelectionUnit.Cell;
            var currentColumn = dataGrid.CurrentColumn;
            dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;  // Переключение режима выделения обратно на строку

            if (currentColumn == null || currentColumn.Header == null)
            {
                contextMenu.Visibility = Visibility.Collapsed;
                return;
            }
            var columnName = currentColumn.SortMemberPath;                 // Название столбца
            if (string.IsNullOrWhiteSpace(columnName))
            {
                contextMenu.Visibility = Visibility.Collapsed;
                return;
            }
            contextMenu.Visibility = Visibility.Visible;
            var сontextMenuItems = contextMenu.Items;
            if (сontextMenuItems.Count != 1)
            {
                return;
            }
            var сontextMenuItem = сontextMenuItems[0] as MenuItem;         // MenuItem контекстного меню
            if (сontextMenuItem == null)
            {
                return;
            }
            var filterFieldWrapperGrid = сontextMenuItem.Header as Grid;   // Grid-обёртка полей контекстного меню
            if (filterFieldWrapperGrid == null)
            {
                return;
            }
            var textBlocks = filterFieldWrapperGrid.Children.OfType<TextBlock>().ToArray();
            var textBoxes = filterFieldWrapperGrid.Children.OfType<TextBox>().ToArray();
            if (textBlocks.Length != 1 || textBoxes.Length != 1)
            {
                return;
            }
            var popupFilterName = textBlocks[0];                    // Контрол для надписи
            var popupFilterValue = textBoxes[0];                    // Поле ввода
            popupFilterName.Text = currentColumn.Header.ToString(); // Отображение названия столбца пользователю
            popupFilterValue.Tag = columnName;                      // Установка столбца в Tag поля ввода
            popupFilterValue.Text = string.Empty;
            SetElementFocusInOtherThread(popupFilterValue);         // Установка клавиатурного фокуса в поле ввода
        }

        /// <summary>
        /// Установка клавиатурного фокуса ввода на выбраную запись DataGrid (Вызов Focus не работает как нужно)
        /// </summary>
        public static void SetFocusOnSelectedRowInDataGrid(object senderIsDatagrid)
        {
            var dataGrid = senderIsDatagrid as DataGrid;
            if (dataGrid == null)
            {
                return;
            }
            if (dataGrid.Items.Count == 0 || dataGrid.SelectedItem == null)
            {
                Keyboard.Focus(dataGrid);
                return;
            }
            var selected = dataGrid.SelectedItem;

            // NOTE: эта копипаста ниже не случайна, нужный функционал срабатывает только со второго раза.
            // Решение в указаном ответе: https://stackoverflow.com/a/27792628 Работает, не трогай
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(selected);
            if (row == null)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(selected);
            }
            if (row == null)
            {
                return;
            }
            dataGrid.UpdateLayout();
            dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
