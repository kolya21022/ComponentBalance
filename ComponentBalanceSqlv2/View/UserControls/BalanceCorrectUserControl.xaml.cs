using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.Utils.Styles;

namespace ComponentBalanceSqlv2.View.UserControls
{
    public partial class BalanceCorrectUserControl
    {
        public BalanceCorrectUserControl()
        {
            InitializeComponent();
            VisualInitializeComponent();
        }

        private void VisualInitializeComponent()
        {
            Background = Visual.BackColor2_Botticelli;
            Foreground = Visual.ForeColor2_PapayaWhip;
            FontSize = Visual.FontSize;
        }

        /// <summary>
        /// Подстановка имени столбца при открытии контексного меню фильтрации DataGrid, 
        /// установка Tag и перемещение фокуса ввода в поле
        /// </summary>
        private void PageDataGrid_OnContextMenuOpening(object senderIsDataGrid, ContextMenuEventArgs eventArgs)
        {
            UserControlUtil.Service_PageDataGridWithFilterContextMenuOpening(senderIsDataGrid);
            SetFocusOnSelectedRowInDataGrid(senderIsDataGrid);
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
        /// Выставление клавиатурного фокуса ввода на строку DataGrid
        /// </summary>
        private void PageDataGrid_OnLoaded(object senderIsDatagrid, RoutedEventArgs eventArgs)
        {
            SetFocusOnSelectedRowInDataGrid(senderIsDatagrid);
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

        /// <summary>
        /// После выбора item в ComboBox фокус снимается с ComboBox и ставится на DataGrid
        /// </summary>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFocusOnSelectedRowInDataGrid(PageDataGrid);
        }
    }
}
