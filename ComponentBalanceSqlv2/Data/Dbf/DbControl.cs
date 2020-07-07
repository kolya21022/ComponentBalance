using System;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Data.Dbf
{
    /// <summary>
    /// Класс управление соединением с БД FoxPro
    /// </summary>
    public static class DbControl
    {
        /// <summary>
        /// Получение соединения с каталогом базы данных FoxPro
        /// </summary>
        public static OleDbConnection GetConnection(string path)
        {
            var connectRow = FoxProConnectRow(path);
            return new OleDbConnection(connectRow);
        }

        /// <summary>
        /// Попытка открытия соединения с каталогом базы данных FoxPro
        /// </summary>
        public static void TryConnectOpen(this OleDbConnection connect)
        {
            var foxProCatalog = connect.DataSource;
            try
            {
                connect.Open();
            }
            catch (DbException ex)
            {
                throw HandlingFoxProConnectOpenPathError(foxProCatalog, ex);
            }
        }

        /// <summary>
        /// Проверка кодировки указанного dbf в указанном DataSource соединения.
        /// В случае, если кодировка не выставлена - выбрасывается StorageException
        /// </summary>
        public static void VerifyInstalledEncoding(this OleDbConnection connection, string dbfFileName)
        {
            const string dbfExtension = ".dBf";
            if (string.IsNullOrWhiteSpace(dbfFileName))
            {
                const string fileParameterNotFound = "В параметрах не указано имя файла";
                throw new ArgumentException(fileParameterNotFound);
            }
            // Подстановка к имени файла расширения, если оно не указано
            var trimmedDbfFileName = dbfFileName.Trim();
            var appendixExtension = Common.LineEndOn(trimmedDbfFileName, dbfExtension) ? string.Empty : dbfExtension;
            var filename = trimmedDbfFileName + appendixExtension;

            // Получение полного пути к файлу
            var datasource = connection.DataSource;
            // ReSharper disable once AssignNullToNotNullAttribute
            var filepath = Path.Combine(datasource, filename);

            // Получение байта кодировки DBF файла
            const long encodingOffset = 29L;
            byte encodingByte;
            try
            {
                encodingByte = Common.ReadOneByteFromFile(filepath, encodingOffset);
            }
            catch (IOException)
            {
                // NOTE: исключение скрывается умышленно, так как при попытке чтения открытого на чтение файла 
                // бросается исключение, хотя запись и не запрещена драйвером
                return;
            }
            if (encodingByte != 0)
            {
                return;
            }

            const string absentEncodingPattern = "Не указана кодировка файла [{0}]. Обратитесь к программистам.";
            var message = string.Format(absentEncodingPattern, filepath);
            throw new StorageException(message);
        }

        /// <summary>
        /// Обработка типовых исключений открытия соединения с каталогом базы данных FoxPro, с формированием 
        /// сообщения о возможной причине.
        /// </summary>
        private static StorageException HandlingFoxProConnectOpenPathError(string foxProCatalog, Exception ex)
        {
            const string errorUncertain = "Непредвиденная ошибка соединения с каталогом базы данных [{0}]";
            const string errorCausePathAccess = "Каталог [{0}] не найден, используется, или вы не имеете прав доступа";
            const string errorCausePath = "Путь [{0}] до базы данных не доступен";

            const int errorNumberCausePathAccess = -2147217865;
            const int errorNumberCausePath = -2147217887;

            string probableCause; // вероятная причина
            var oleDbException = ex as OleDbException;
            if (oleDbException != null)
            {
                var exceptionNumber = oleDbException.ErrorCode;
                switch (exceptionNumber)
                {
                    case errorNumberCausePath:        // недоступен путь
                        probableCause = string.Format(errorCausePath, foxProCatalog);
                        break;
                    case errorNumberCausePathAccess:  // недоступен каталог
                        probableCause = string.Format(errorCausePathAccess, foxProCatalog);
                        break;
                    default:                          // неопределённая ошибка открытия соединения
                        probableCause = string.Format(errorUncertain, foxProCatalog);
                        break;
                }
            }
            else // неопределённая ошибка открытия соединения
            {
                probableCause = string.Format(errorUncertain, foxProCatalog);
            }
            return new StorageException(ex.Message, probableCause, ex);
        }

        /// <summary>
        /// Получение строки соединения с каталогом базы данных foxpro
        /// </summary>
        private static string FoxProConnectRow(string path)
        {
            const string connectionPattern = "Provider={0};Data Source={1};";
            const string provider = "VFPOLEDB.1";
            return string.Format(connectionPattern, provider, path);
        }

        /// <summary>
        /// Обработка типовых исключений работы с БД FoxPro, с формированием сообщения о возможной причине.
        /// В случае, если исходное исключение не StorageException или не OleDbException - пробрасывается исходное.
        /// </summary>
        public static Exception HandleKnownDbFoxProExceptions(DbException ex)
        {
            const int foxproWrongQueryErrorCode = -2147467259;
            const string foxProWrongQueryMessage = "Ошибка в SQL-запросе к базе данных FoxPro: несовпадение типов, " +
                "неверное имя столбца, отсутсвие столбца или что-то иное.";
            const int foxproFileAccessErrorCode = -2147217865;
            const string foxproFileAccessMessage = "Нет доступа к файлу (или отсутвует файл) базы " +
                "данных FoxPro для выполнения запроса.";

            var isDbException = ex is StorageException || ex is OleDbException;
            if (!isDbException)
            {
                return ex; // Исключение является не StorageException, не OleDbException
            }
            var probableCause = string.Empty; // предполагаемая ошибка
            var currentInnerException = ex;
            do
            {
                var oleDbException = currentInnerException as OleDbException;
                if (oleDbException != null)
                {
                    var oleDbErrorCode = oleDbException.ErrorCode;
                    if (oleDbErrorCode == foxproWrongQueryErrorCode)      // Причина - ошибочный запрос к FoxPro БД
                    {
                        probableCause = foxProWrongQueryMessage;
                        break;
                    }
                    if (oleDbErrorCode == foxproFileAccessErrorCode)      // Причина - недоступен файл FoxPro БД
                    {
                        probableCause = foxproFileAccessMessage;
                        break;
                    }
                }
                // следующее вложенное исключение
                currentInnerException = currentInnerException.InnerException as DbException;
            } while (currentInnerException != null);

            // Если неизвестная причина SqlException или OleDbException
            if (string.Empty.Equals(probableCause))
            {
                return new StorageException(ex.Message, ex);
            }

            // Если SqlException или OleDbException с "возможной причиной"
            throw new StorageException(ex.Message, probableCause, ex);
        }
    }
}