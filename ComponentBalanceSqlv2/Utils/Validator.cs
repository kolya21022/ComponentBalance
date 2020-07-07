using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace ComponentBalanceSqlv2.Utils
{
    /// <summary>
    /// Утилитарный класс с методами валидации (проверки корректности данных)
    /// </summary>
    public static class Validator
    {
        private const string ErrorSizePattern = "Кол-во символов поля [{0}] должно быть не больше [{1}] символов";
        private const string ErrorEmptyOrAbsentPattern = "Значение поля [{0}] не указано/не выбрано";
        private const string ErrorDecimalNegativePattern = "Значение поля [{0}] не может быть отрицательным";
        private const string ErrorDecimalZeroPattern = "Значение поля [{0}] не может быть равно 0";
        private const string ErrorMessageNotMatchDate = "Значение поля [{0}] не может отличаться от текущего рабочего месяца и года";
        private const string ErrorMessageNotMoreValue = "Значение поля [{0}] не может привышать [{1}]";

        /// <summary>
        /// Валидация не пустой строки, с числом символов не больше указанного.
        /// В случае несоответствия уловиям (false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsLineNotEmptyAndSizeNoMore(string value, string fieldName, int maxLength,
            StringBuilder errorMessages)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var messageEmptyValue = string.Format(ErrorEmptyOrAbsentPattern, fieldName);
                errorMessages.AppendLine(messageEmptyValue);
                return false;
            }
            if (maxLength >= value.Trim().Length)
            {
                return true;
            }
            var messageSizeError = string.Format(ErrorSizePattern, fieldName, maxLength);
            errorMessages.AppendLine(messageSizeError);
            return false;
        }

        /// <summary>
        /// Валидация строки, которая может быть пустой, но с числом символов не больше указанного.
        /// В случае несоответствия уловиям (false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsLineMightEmptyAndSizeNoMore(string value, string fieldName, int maxLength,
            StringBuilder errorMessages)
        {
            if (string.IsNullOrWhiteSpace(value) || maxLength >= value.Trim().Length)
            {
                return true;
            }
            var message = string.Format(ErrorSizePattern, fieldName, maxLength);
            errorMessages.AppendLine(message);
            return false;
        }

        /// <summary>
        /// Валидация объекта на null.
        /// В случае если объект null (возвращается false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsNotNullSelectedObject(object value, string fieldName, StringBuilder errorMessages)
        {
            if (value != null)
            {
                return true;
            }
            var message = string.Format(ErrorEmptyOrAbsentPattern, fieldName);
            errorMessages.AppendLine(message);
            return false;
        }

        /// <summary>
        /// Валидация объекта на -1.
        /// В случае если объект равен -1 (возвращается false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsNotSelectedObject(decimal value, string fieldName, StringBuilder errorMessages)
        {
            if (value != -1)
            {
                return true;
            }
            var message = string.Format(ErrorEmptyOrAbsentPattern, fieldName);
            errorMessages.AppendLine(message);
            return false;
        }

        /// <summary>
        /// Проверка состоит ли строка только из символов кириллицы и пробелов, и запрос подтверждения продолжения у 
        /// пользователя в противном случае. Для пустой строки или состоящей только из пробелов, возвращается true.
        /// </summary>
        /// <returns>Возвращает true в случае, если поле состоит только из кириллицы или пользователь подтвердил, 
        /// что он и хотел некириллические символы</returns>
        public static bool IsCyrillicWithUserConfirmOtherwise(string value, string fieldName)
        {
            var headerConfirm = "Запрос подтверждения";
            var newLine = Environment.NewLine;
            var confirmPattern = "Поле [{0}] содержит некириллические символы, цифры или иные нестандартные символы." +
                                 newLine + "Вы действительно хотите оставить такое значение?" + newLine + "「{1}」";
            const string cyrillicRegexFormatPattern = "^[\\p{{IsCyrillic}}\\s\\p{{P}}]{{{0},{1}}}$";
            const int maxLength = int.MaxValue;
            const int minLength = 0;

            var cyrillicRegexPattern = string.Format(cyrillicRegexFormatPattern, minLength, maxLength);
            var regex = new Regex(cyrillicRegexPattern);
            var match = regex.Match(value.Trim());
            if (match.Success)
            {
                return true;
            }
            const MessageBoxImage messageType = MessageBoxImage.Asterisk;
            const MessageBoxButton messageButtons = MessageBoxButton.OKCancel;
            var confirmMessage = string.Format(confirmPattern, fieldName, value);
            var result = MessageBox.Show(confirmMessage, headerConfirm, messageButtons, messageType);
            return result == MessageBoxResult.OK;
        }

        /// <summary>
        /// Валидация не отрицательного не null decimal без десятичной точки (целого), 
        /// с числом символов не больше указанного.
        /// В случае несоответствия уловиям (false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsPositiveNotNullDecimalWithoutPointAndSizeNoMore(decimal? nullableValue, string fieldName,
            int maxDigitsIntCount, StringBuilder errorMessages)
        {
            const string errorDecimalOnlyIntergerPattern = "Значение поля [{0}] должно быть целым";
            if (nullableValue == null)
            {
                var messageEmptyValue = string.Format(ErrorEmptyOrAbsentPattern, fieldName);
                errorMessages.AppendLine(messageEmptyValue);
                return false;
            }
            var value = (decimal)nullableValue;
            if (value == 0)
            {
                var messageNegativeValue = string.Format(ErrorDecimalZeroPattern, fieldName);
                errorMessages.AppendLine(messageNegativeValue);
                return false;
            }
            if (value < 0)
            {
                var messageNegativeValue = string.Format(ErrorDecimalNegativePattern, fieldName);
                errorMessages.AppendLine(messageNegativeValue);
                return false;
            }
            if (Common.FractionalPartDigitCount(value) != 0)
            {
                var messageNegativeValue = string.Format(errorDecimalOnlyIntergerPattern, fieldName);
                errorMessages.AppendLine(messageNegativeValue);
                return false;
            }
            if (Common.IntegerPartDigitCount(value) <= maxDigitsIntCount)
            {
                return true;
            }
            var messageSizeError = string.Format(ErrorSizePattern, fieldName, maxDigitsIntCount);
            errorMessages.AppendLine(messageSizeError);
            return false;
        }

        /// <summary>
        /// Валидация не отрицательного не null decimal с десятичной точкой (вещественного), с числом символов не 
        /// больше указанного и с не числом значимых символов после десятичной точки.
        /// В случае несоответствия уловиям (false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsPositiveNotNullDecimalWithPointAndSizeNoMore(decimal? nullableValue, string fieldName,
            int maxIntDigitsCount, int maxSignsAfterPoint, StringBuilder errorMessages)
        {
            const string errorIntegerSizePattern = "Кол-во символов поля [{0}] перед точкой " +
                                                   "должно быть не больше [{1}] символов";
            const string errorFractionSizePattern = "Кол-во символов поля [{0}] после точки " +
                                                    "должно быть не больше [{1}] символов";
            if (nullableValue == null)
            {
                var messageEmptyValue = string.Format(ErrorEmptyOrAbsentPattern, fieldName);
                errorMessages.AppendLine(messageEmptyValue);
                return false;
            }
            var value = (decimal)nullableValue;
            if (value == 0)
            {
                var messageNegativeValue = string.Format(ErrorDecimalZeroPattern, fieldName);
                errorMessages.AppendLine(messageNegativeValue);
                return false;
            }
            if (value < 0)
            {
                var messageNegative = string.Format(ErrorDecimalNegativePattern, fieldName);
                errorMessages.AppendLine(messageNegative);
                return false;
            }
            //Проверка числа символов перед точкой
            if (Common.IntegerPartDigitCount(value) > maxIntDigitsCount)
            {
                var messageIntegerLength = string.Format(errorIntegerSizePattern, fieldName, maxIntDigitsCount);
                errorMessages.AppendLine(messageIntegerLength);
                return false;
            }
            //Проверка числа символов после точки
            if (Common.FractionalPartDigitCount(value) <= maxSignsAfterPoint)
            {
                return true;
            }
            var messageFractionLength = string.Format(errorFractionSizePattern, fieldName, maxSignsAfterPoint);
            errorMessages.AppendLine(messageFractionLength);
            return false;
        }

        /// <summary>
        /// Проверка совпадения указанных месяца и года с текущими(рабочими).
        /// В случае если месяц и год не совпадают (возвращается false), в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsSelectedMonthAndYearMatchTodaysMonthAndYear(int selectedMonth, int selectedYear,
            decimal todayMonth, decimal todayYear, string fieldName, StringBuilder errorMessages)
        {
            if (selectedMonth == todayMonth && selectedYear == todayMonth)
            {
                return true;
            }
            var messageEmptyValue = string.Format(ErrorMessageNotMatchDate, fieldName);
            errorMessages.AppendLine(messageEmptyValue);
            return false;
        }

        /// <summary>
        /// Проверка не привышает ли decimal значение value -> value2, в errorMessages заносится сообщение об ошибке.
        /// </summary>
        public static bool IsNotMoreValue(decimal value, decimal value2, string fieldName, StringBuilder errorMessages)
        {
            if (value <= value2)
            {
                return true;
            }
            var message = string.Format(ErrorMessageNotMoreValue, fieldName, value2);
            errorMessages.AppendLine(message);
            return false;
        }
    }
}
