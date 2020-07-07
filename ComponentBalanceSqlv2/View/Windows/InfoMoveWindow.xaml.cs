using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComponentBalanceSqlv2.Utils.Styles;

namespace ComponentBalanceSqlv2.View.Windows
{
    public partial class InfoMoveWindow
    {
        public InfoMoveWindow()
        {
            InitializeComponent();
            FontSize = Visual.FontSize;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
