using System;
using System.Data.Common;

namespace ComponentBalanceSqlv2.Model
{
    /// <summary>
    /// Исключение слоя работы с базой данных.
    /// В некоторых случаях указывается возможная `человекочитаемая` причина возникновения, 
    /// для информирования конечных пользователей, в специальном окне приложения.
    /// </summary>
    /// <inheritdoc />
    public class StorageException : DbException
    {
        /// <summary>
        /// Возможная причина
        /// </summary>
        public string ProbableCause { get; }

        public StorageException(string message) : base(message) { }

        public StorageException(string message, string probableCause, Exception innerException)
            : base(message, innerException)
        {
            ProbableCause = probableCause;
        }

        public StorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
