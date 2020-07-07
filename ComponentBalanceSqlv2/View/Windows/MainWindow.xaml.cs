using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.Utils.Styles;
using ComponentBalanceSqlv2.ViewModel;

namespace ComponentBalanceSqlv2.View.Windows
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            VisualInitializeComponent();


            Title = Common.GetApplicationTitle(Assembly.GetExecutingAssembly());
        }

        private void VisualInitializeComponent()
        {
            Background = Visual.BackColor2_Botticelli;
            Foreground = Visual.ForeColor2_PapayaWhip;
            WorkGuildTextBlock.Background = Visual.BackColor3_SanJuan;
            WorkGuildTextBlock.Foreground = Visual.ForeColor2_PapayaWhip;
            MonthAndYearTextBlock.Background = Visual.BackColor3_SanJuan;
            MonthAndYearTextBlock.Foreground = Visual.ForeColor2_PapayaWhip;
            FontSize = Visual.FontSize;
            if (Properties.Settings.Default.IsRunInFullscreen)
            {
                WindowState = WindowState.Maximized;
            }

            // Панель хоткеев
            HotkeysDockPanel.Background = Visual.BackColor4_BlueBayoux;
            var helpLabels = HotkeysDockPanel.Children.OfType<Label>();
            foreach (var helpLabel in helpLabels)
            {
                helpLabel.Foreground = Visual.ForeColor2_PapayaWhip;
            }

            var expanders = WrapperStackPanel.Children.OfType<Expander>();
            foreach (var expander in expanders)
            {
                expander.Background = Visual.BackColor4_BlueBayoux;
                expander.BorderBrush = Visual.LineBorderColor2_Nepal;
                expander.Foreground = Visual.ForeColor2_PapayaWhip;
                expander.FontSize = Visual.FontSize;

                var stackPanel = expander.Content as StackPanel;
                if (stackPanel == null)
                {
                    continue;
                }

                stackPanel.Background = Visual.BackColor4_BlueBayoux;

                var buttons = new List<Button>();
                buttons.AddRange(stackPanel.Children.OfType<Button>());
                foreach (var button in buttons)
                {
                    button.Foreground = Visual.ForeColor1_BigStone;
                }
            }

            WindowMenu.FontSize = Visual.FontSize;
            WindowMenu.Background = Visual.BackColor3_SanJuan;
            foreach (var menuItem in WindowMenu.Items)
            {
                var menuItemControl = menuItem as MenuItem;
                if (menuItemControl == null)
                {
                    continue;
                }

                menuItemControl.Background = Visual.BackColor4_BlueBayoux;
                menuItemControl.Foreground = Visual.ForeColor2_PapayaWhip;
            }

        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// В случае совпадения логина и пароля, передача новых цеха и участка
        /// </summary>
        private void AuthenticationViewModelOnOnAuthorize(object sender, AuthenticationEventArgs e)
        {
            if (e.IsAuthorized)
            {
                DataContext = new MainViewModel(e.User);
                _authenticationWindow.Close();
            }
        }

        private AuthenticationWindow _authenticationWindow;

        private void MenuItemChangeLogin_OnClick(object sender, RoutedEventArgs e)
        {
            var authenticationViewModel = new AuthenticationViewModel();
            _authenticationWindow = new AuthenticationWindow()
            {
                DataContext = authenticationViewModel
            };
            authenticationViewModel.OnAuthorize += AuthenticationViewModelOnOnAuthorize;
            _authenticationWindow.Show();
        }
    }
}