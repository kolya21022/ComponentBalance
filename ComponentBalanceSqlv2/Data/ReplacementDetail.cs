using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;
using ComponentBalanceSqlv2.Utils;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Взаимозаменяймые детали]
    /// </summary>
    public class ReplacementDetail : BaseNotificationClass, IEnoughDetail
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Короткий код изделия которому разрешена данная взаимозамена детали.
        /// </summary>
        public int ShortProductCode { get; set; }

        // Не использую class Detail потому что замены не привязываются к определенному Measure который нужен для Detail.

        /// <summary>
        /// Заводской код заменяймой детали.
        /// </summary>
        public long DetailStartCode { get; set; }

        /// <summary>
        /// Название заменяймой детали.
        /// </summary>
        public string DetailStartName { get; set; }

        /// <summary>
        /// Обозначение заменяймой детали.
        /// </summary>
        public string DetailStartDesignation { get; set; }

        /// <summary>
        /// ГОСТ заменяймой детали.
        /// </summary>
        public string DetailStartGost { get; set; }

        /// <summary>
        /// Заводской код замены детали.
        /// </summary>
        public long DetailEndCode { get; set; }

        /// <summary>
        /// Название замены детали.
        /// </summary>
        public string DetailEndName { get; set; }

        /// <summary>
        /// Обозначение замены детали.
        /// </summary>
        public string DetailEndDesignation { get; set; }

        /// <summary>
        /// ГОСТ замены детали.
        /// </summary>
        public string DetailEndGost { get; set; }

        /// <summary>
        /// Причина разрешения на замену.
        /// </summary>
        public string Cause { get; set; }

        private decimal _count;
        /// <summary>
        /// Выбранное кол-во замены.
        /// </summary>
        [NotMapped]
        public decimal Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }

        public ReplacementDetail() { }

        public ReplacementDetail(int shortProductCode, Detail detailStart, Detail detailEnd, string cause)
        {
            ShortProductCode = shortProductCode;
            DetailStartCode = detailStart.Code;
            DetailStartName = detailStart.Name;
            DetailStartDesignation = detailStart.Designation;
            DetailStartGost = detailStart.Gost;

            DetailEndCode = detailEnd.Code;
            DetailEndName = detailEnd.Name;
            DetailEndDesignation = detailEnd.Designation;
            DetailEndGost = detailEnd.Gost;

            Cause = cause;
        }

        public ReplacementDetail(int shortProductCode, 
            long detailStartCode, string detailStartName, string detailStartDesignation, string detailStartGost,
            long detailEndCode, string detailEndName, string detailEndDesignation, string detailEndGost,
            string cause)
        {
            ShortProductCode = shortProductCode;
            DetailStartCode = detailStartCode;
            DetailStartName = detailStartName;
            DetailStartDesignation = detailStartDesignation;
            DetailStartGost = detailStartGost;

            DetailEndCode = detailEndCode;
            DetailEndName = detailEndName;
            DetailEndDesignation = detailEndDesignation;
            DetailEndGost = detailEndGost;

            Cause = cause;
        }

        private Enough _enough = Enough.Unknown;
        /// <summary>
        /// Указывает достаточно ли детали [DetailEndCode] на балансе для замены.
        /// </summary>
        [NotMapped]
        public Enough Enough
        {
            get { return _enough; }
            set
            {
                _enough = value;
                OnPropertyChanged();
            }
        }
    }
}