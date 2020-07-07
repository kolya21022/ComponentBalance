using System.Windows.Controls;
using System.Windows.Input;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.Utils.Styles;

namespace ComponentBalanceSqlv2.View.UserControls
{
    public partial class EditCompositionProductUserControl
    {
        public EditCompositionProductUserControl()
        {
            InitializeComponent();
            FontSize = Visual.FontSize;
        }

        /// <summary>
        /// Подстановка имени столбца при открытии контексного меню фильтрации DataGrid, 
        /// установка Tag и перемещение фокуса ввода в поле
        /// </summary>
        private void PageDataGrid_OnContextMenuOpening(object senderIsDataGrid, ContextMenuEventArgs eventArgs)
        {
            UserControlUtil.Service_PageDataGridWithFilterContextMenuOpening(senderIsDataGrid);
        }

        /// <summary>
        /// Нажатие клавиши в контексном меню - исправление дефекта скрытия фильтра при переключении раскладки ввода
        /// </summary>
        private void PopupFilterContextMenu_OnKeyDown(object senderIsMenuItem, KeyEventArgs eventArgs)
        {
            var key = eventArgs.Key;
            eventArgs.Handled = key == Key.System || key == Key.LeftAlt || key == Key.RightAlt;
        }
    }
}
