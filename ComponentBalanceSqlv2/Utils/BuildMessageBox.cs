using System.Windows;

namespace ComponentBalanceSqlv2.Utils
{
    public static class BuildMessageBox
    {
        // Заголовки сообщений MessageBox
        private const string HeaderConfirm = "Запрос подтверждения";
        private const string HeaderLogicError = "Ошибка логики приложения";
        private const string HeaderCriticalError = "Критическая ошибка приложения";
        private const string HeaderValidation = "Сообщение проверки корректности данных";
        private const string HeaderInformationOrWarning = "Информационное сообщение / предупреждение";

        /// <summary>
        /// Вызвать окно ошибки приложения.
        /// </summary>
        /// <param name="message"> Сообщение ошибки. </param>
        public static void GetCriticalErrorMessageBox(string message)
        {
            MessageBox.Show(message, HeaderCriticalError,
                MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        /// <summary>
        /// Вызвать окно подтверждения запроса.
        /// </summary>
        /// <param name="message"> Сообщение. </param>
        /// <returns> MessageBoxResult.Yes; MessageBoxResult.No </returns>
        public static MessageBoxResult GetConfirmMessageBox(string message)
        {
            return MessageBox.Show(message, HeaderConfirm, MessageBoxButton.YesNo,
                MessageBoxImage.Question);
        }

        /// <summary>
        /// Вызвать окно информации.
        /// </summary>
        /// <param name="message"> Сообщение. </param>
        public static void GetInformationMessageBox(string message)
        {
            MessageBox.Show(message, HeaderInformationOrWarning, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Вызвать окно логической ошибки.
        /// </summary>
        /// <param name="message"> Сообщение. </param>
        public static void GetLogicErrorMessageBox(string message)
        {
            MessageBox.Show(message, HeaderLogicError, MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
