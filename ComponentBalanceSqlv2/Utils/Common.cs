using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Utils
{
    /// <summary>
    /// Общие утилитарные методы бизнес логики
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Проверка оканчивается ли указанная строка на указанную подстроку
        /// </summary>
        public static bool LineEndOn(string line, string ended)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }
            const StringComparison compareIgnoreCase = StringComparison.OrdinalIgnoreCase;
            return line.Length > ended.Length &&
                   string.Equals(ended, line.Substring(line.Length - ended.Length), compareIgnoreCase);
        }

        /// <summary>
        /// Чтение 1 байта по указанному смещению из указанного файла
        /// </summary>
        public static byte ReadOneByteFromFile(string file, long offset)
        {
            var buffer = new byte[1];
            using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(buffer, 0, 1);
                return buffer[0];
            }
        }

        /// <summary>
        /// Получения объекта класса цвета визуальных элементов, по hex-коду цвета
        /// </summary>
        public static SolidColorBrush BrushHex(string hexColor)
        {
            var solidColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
            if (solidColorBrush != null && !solidColorBrush.IsFrozen)
            {
                // NOTE: Делаем цвет нередактируемым, так как без этого сильно просаживается производительность.
                // Подробнее (Freezable Objects): https://msdn.microsoft.com/en-us/library/aa970683(v=vs.85).aspx
                solidColorBrush.Freeze();
            }
            return solidColorBrush;
        }

        /// <summary>
        /// Получить все месяца и их полные имена
        /// </summary>
        public static Dictionary<int, string> MonthsFullNames()
        {
            const int offset = 1;
            // ReSharper disable once PossibleNullReferenceException
            var arrayMonths = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentCulture).MonthNames;
            var months = new Dictionary<int, string>();
            var i = 0;
            foreach (var month in arrayMonths)
            {
                var number = i + offset;
                months.Add(number, month);
                if (++i >= arrayMonths.Length - offset) // Тринадцатый пустой месяц
                {
                    break;
                }
            }
            return months;
        }

        /// <summary>
        /// Формирование пути к RDLC-файлу отчёта
        /// </summary>
        public static string GetReportFilePath(string reportFileName)
        {
            var appFolder = ApplicationFolder();
            const string dataFolder = Constants.AppDataFolder;
            const string reportFolder = Constants.ReportFolder;
            return Path.Combine(appFolder, dataFolder, reportFolder, reportFileName);
        }

        /// <summary>
        /// Получение каталога exe-файла приложения
        /// </summary>
        private static string ApplicationFolder()
        {
            return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// Отображение детальной информации со стектрейсом о выброшеном исключении в отдельном окне.
        /// Для StorageException в отдельном поле, отображается 'возможная причина'.
        /// </summary>
        public static string ShowDetailExceptionMessage(Exception ex)
        {
            var messages = ExceptionMessages(ex);        // Получение и отображение сообщений/возможных причин
            var messagesToWrite = string.Empty;
            foreach (var message in messages)
            {
                if (messagesToWrite.Length > 0)
                {
                    messagesToWrite += "\n" + message;
                }
                else
                {
                    messagesToWrite += message;
                }
            }

            messagesToWrite += "\n\nСтектрейс\n";
            messagesToWrite += ex.ToString();

            return messagesToWrite;
        }

        /// <summary>
        /// Получение списка сообщений/возможных причин возникновения исключения, включая все вложенные исключения
        /// </summary>
        private static List<string> ExceptionMessages(Exception ex)
        {
            var messages = new List<string>();
            var currentEx = ex;
            do
            {
                var currentExStorage = currentEx as StorageException;
                if (currentExStorage != null && !string.IsNullOrWhiteSpace(currentExStorage.ProbableCause))
                {
                    var probableCause = currentExStorage.ProbableCause;
                    if (!messages.Contains(probableCause))
                    {
                        messages.Add(probableCause);
                    }
                }
                if (!string.IsNullOrWhiteSpace(currentEx.Message))
                {
                    var message = currentEx.Message;
                    if (!messages.Contains(message))
                    {
                        messages.Add(message);
                    }
                }
                currentEx = currentEx.InnerException; // следующее вложенное исключение
            } while (currentEx != null);
            return messages;
        }

        /// <summary>
        /// Вычисление точного количества месяцев между датами.
        /// <para>Время не учитывается.</para>
        /// <para>Конечная дата должна быть больше или равна начальной.</para>
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <returns>Количество месяцев</returns>
        public static int TotalMonths(DateTime startDate, DateTime endDate)
        {
            DateTime dt1 = startDate.Date, dt2 = endDate.Date;
            if (dt1 > dt2) return -1;
            if (dt1 == dt2) return 0;

            var m = ((dt2.Year - dt1.Year) * 12)
                    + dt2.Month - dt1.Month
                    + (dt2.Day >= dt1.Day - 1 ? 0 : -1)//поправка на числа
                    + ((dt1.Day == 1 && DateTime.DaysInMonth(dt2.Year, dt2.Month) == dt2.Day) ? 1 : 0);//если начальная дата - 1-е число меяца, а конечная - последнее число, добавляется 1 месяц
            return m;
        }

        /// <summary>
        /// Получение заголовка главного окна приложения.
        /// В заголовке название, версия, дата/время сборки, текущий пользователь и т.д.
        /// </summary>
        public static string GetApplicationTitle(Assembly assembly)
        {
            var currentIdentity = WindowsIdentity.GetCurrent();                           // текущий пользователь
                                                                                          // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var user = currentIdentity != null ? currentIdentity.Name : string.Empty;   // имя пользователя
            var versionInfo = GetFileVersionInfo(assembly);
            var appName = versionInfo.Comments;
            var productVersion = versionInfo.ProductVersion;
            var buildDate = GetBuildDateTime(assembly);
            var titlePattern = GenerateTitlePattern(buildDate != null);
            return string.Format(titlePattern, appName, productVersion, buildDate != null
                    ? string.Format(Constants.BuildDateTimePattern, buildDate)
                    : string.Empty, user);
        }

        /// <summary>
        /// Формирование шаблона заголовка окна приложения, в зависимости от наличия DateTime сборки приложения
        /// </summary>
        private static string GenerateTitlePattern(bool isbuildDateExist)
        {
            const string titlePatternPart1 = "{0}, {1}";
            const string titlePatternPart2WithDate = " [Build: {2}]";
            const string titlePatternPart2WithouDate = "{2}";
            const string titlePatternPart3 = " [{3}]";
            return titlePatternPart1 + (isbuildDateExist
                       ? titlePatternPart2WithDate
                       : titlePatternPart2WithouDate) + titlePatternPart3;
        }

        /// <summary>
        /// Получение объекта класса FileVersionInfo из указанной Assembly
        /// (Требуется для чтения аттрибутов сборки приложения: название, версия и т.д.)
        /// </summary>
        private static FileVersionInfo GetFileVersionInfo(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location);
        }

        /// <summary>
        /// Метод получения даты/времени компиляции указанной сборки
        /// </summary>
        private static DateTime? GetBuildDateTime(Assembly assembly)
        {
            var current = DateTime.Now;
            var universal = current.ToUniversalTime();
            var gmtOffset = (current - universal).TotalHours;
            try
            {
                var file = assembly.Location;
                const int headerOffset = 60;
                const int linkerTimestampOffset = 8;
                var buffer = new byte[2048];
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    stream.Read(buffer, 0, 2048);
                }
                var offset = BitConverter.ToInt32(buffer, headerOffset);
                var startIdnex = offset + linkerTimestampOffset;
                var secondsSince1970 = BitConverter.ToInt32(buffer, startIdnex);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
                return linkTimeUtc.AddHours(gmtOffset);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Русская локаль приложения и десятичный разделитель 'точка' по-умолчанию
        /// </summary>
        public static void SetRussianLocaleAndDecimalSeparatorDot()
        {
            const string decimalSeparator = ".";
            const string russianLocale = "ru-RU";
            var russianCultureInfo = new CultureInfo(russianLocale)
            {
                NumberFormat = { NumberDecimalSeparator = decimalSeparator }
            };
            Thread.CurrentThread.CurrentCulture = russianCultureInfo;
            Thread.CurrentThread.CurrentUICulture = russianCultureInfo;
            var cultureIetfLanguageTag = CultureInfo.CurrentCulture.IetfLanguageTag;
            var xmlLanguage = XmlLanguage.GetLanguage(cultureIetfLanguageTag);
            var frameworkPropertyMetadata = new FrameworkPropertyMetadata(xmlLanguage);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), frameworkPropertyMetadata);
        }

        /// <summary>
        /// Получение Mutex'а приложения: состоит из названия, версии и guid продукта
        /// </summary>
        public static Mutex GetApplicationMutex(out bool isCreatedNew)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var product = ProductName();
            var version = ProductVersion();
            var guid = ((GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;
            return new Mutex(true, $"[{product} : {version} : {guid}]", out isCreatedNew);
        }

        /// <summary>
        /// Получение названия приложения из текущей сборки
        /// </summary>
        private static string ProductName()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = GetFileVersionInfo(assembly);
            return versionInfo.ProductName;
        }

        /// <summary>
        /// Получение версии приложения из текущей сборки
        /// </summary>
        private static string ProductVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = GetFileVersionInfo(assembly);
            return versionInfo.ProductVersion;
        }

        /// <summary>
        /// Переключение языка ввода на язык текущей локали (русский)
        /// </summary>
        public static void SetKeyboardInputFromCurrentLocale()
        {
            InputLanguageManager.Current.CurrentInputLanguage = Thread.CurrentThread.CurrentCulture;
        }

        /// <summary>
        /// Получение главного окна приложения, для установки, в качестве родительского, дочерним окнам.
        /// Требуется из-за того, что нельзя установить окно дочерним, если оно ещё не было отображено.
        /// </summary>
        public static Window GetOwnerWindow()
        {
            var mainWindow = Application.Current.MainWindow;
            return mainWindow != null && mainWindow.IsVisible ? mainWindow : null;
        }

        /// <summary>
        /// Отображение сообщения об уже запущеной копии этого приложения в операционной системе
        /// </summary>
        public static void ShowMessageForMutexAlreadyRunning()
        {
            var isRunPattern = "В этой операционной системе уже запущен один экземпляр приложения:" +
                               Environment.NewLine + "{0} : {1}";
            var product = ProductName();
            var version = ProductVersion();
            var alreadyRunMessage = string.Format(isRunPattern, product, version);
            const MessageBoxResult defaultButton = MessageBoxResult.OK;
            const MessageBoxButton messageBoxButtons = MessageBoxButton.OK;
            const MessageBoxImage messageBoxType = MessageBoxImage.Information;
            MessageBox.Show(alreadyRunMessage, product, messageBoxButtons, messageBoxType, defaultButton);
        }

        /// <summary>
        /// Число символов целой части Decimal
        /// </summary>
        public static int IntegerPartDigitCount(decimal value)
        {
            value = Math.Abs(value);
            var count = 1;
            while ((value /= 10M) > 1M)
            {
                ++count;
            }
            return count;
        }

        /// <summary>
        /// Число символов части после запятой в Decimal
        /// </summary>
        public static int FractionalPartDigitCount(decimal value)
        {
            value = Math.Abs(value);
            var count = 0;
            while (value % 1M != 0M)
            {
                ++count;
                value *= 10M;
            }
            return count;
        }
    }
}