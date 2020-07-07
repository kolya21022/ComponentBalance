using Visual = ComponentBalanceSqlv2.Utils.Styles.Visual;

namespace ComponentBalanceSqlv2.View.Windows
{
    /// <summary>
    /// Логика взаимодействия для UserConfigWindow.xaml
    /// </summary>
    public partial class UserConfigWindow
    {
        public UserConfigWindow()
        {
            InitializeComponent();
            FontSize = Visual.FontSize;
        }
    }
}
