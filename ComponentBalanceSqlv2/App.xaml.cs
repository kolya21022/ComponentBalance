using System.Threading;
using System.Windows;
using ComponentBalanceSqlv2.View.Windows;
using ComponentBalanceSqlv2.Utils;
using ComponentBalanceSqlv2.ViewModel;

namespace ComponentBalanceSqlv2
{
    public partial class App
    {
        /// <summary>
        /// Маркер, сигнализирующий об захвате нового экземляра Mutex этого приложения
        /// </summary>
        private static bool _isCreatedNew;

        /// <summary>
        /// Примитив синхронизации OC, используемый для проверки запуска копий этого приложения в OC.
        /// NOTE: работа с Mutex базируется на этом решении: https://stackoverflow.com/a/5376828
        /// </summary>
        private static Mutex _mutex = Common.GetApplicationMutex(out _isCreatedNew);

        private AuthenticationWindow AuthenticationWindow { get; set; }
        public AuthenticationViewModel AuthenticationViewModel { get; set; }
        public static MainViewModel MainViewModel { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Если этот Mutex не первый созданный в текущий момент в операционной системе, сообщение и завершение
            if (!_isCreatedNew)
            {
                _mutex = null;
                Common.ShowMessageForMutexAlreadyRunning();
                Current.Shutdown();
                return;
            }

            // Установка русско-язычной локали и десятичного разделителя точки
            Common.SetRussianLocaleAndDecimalSeparatorDot();

            base.OnStartup(e);

            // Переключение языка ввода на русский
            Common.SetKeyboardInputFromCurrentLocale();

            AuthenticationViewModel = new AuthenticationViewModel();
            AuthenticationWindow = new AuthenticationWindow() { DataContext = AuthenticationViewModel };
            AuthenticationViewModel.OnAuthorize += AuthenticationViewModelOnAuthorize;
            AuthenticationWindow.Show();
        }

        /// <summary>
        /// Освобождение Mutex при завершении приложения
        /// </summary>
        /// <inheritdoc />
        protected override void OnExit(ExitEventArgs eventArgs)
        {
            _mutex?.ReleaseMutex();

            base.OnExit(eventArgs);
        }

        private void AuthenticationViewModelOnAuthorize(object sender, AuthenticationEventArgs e)
        {
            if (e.IsAuthorized)
            {
                MainViewModel = new MainViewModel(e.User);
                MainWindow = new MainWindow()
                {
                    DataContext = MainViewModel
                };
                MainWindow.Show();
                AuthenticationWindow.Close();
            }
        }
    }
}