using System.Windows;
using ComponentBalanceSqlv2.Utils.Styles;

namespace ComponentBalanceSqlv2.View.Windows
{
    public partial class AuthenticationWindow
    {
        public AuthenticationWindow()
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
