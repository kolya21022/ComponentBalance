using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Детали].
    /// </summary>
    public class Detail
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Заводской код детали.
        /// </summary>
        public long Code { get; set; }
        
        /// <summary>
        /// Название детали.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Обозначение детали.
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// ГОСТ детали.
        /// </summary>
        public string Gost { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Еденицы измерения], 
        /// указывающий ед. измерения детали.
        /// </summary>
        public long MeasureId { get; set; }  
        public virtual Measure Measure { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Типов ТМЦ], 
        /// указывающий какой тип ТМЦ детали.
        /// </summary>
        public long TmcTypeId { get; set; }
        public virtual TmcType TmcType { get; set; }

        /// <summary>
        /// Склад хранения детали.
        /// </summary>
        public int Warehouse { get; set; }

        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<CompositionProduct> CompositionProducts { get; set; }

        public virtual ICollection<ReleaseReplacementDetail> ReleaseReplacementDetails { get; set; }

        /// <summary>
        /// Краткая информация о детали.
        /// </summary>
        public string SelectedDisplay =>
            $"Код: [{Code}]; Наименование: [{Name}]; Обозначение: [{Designation}]; ГОСТ: [{Gost}]";

        public Detail()
        {
            Balances = new HashSet<Balance>();
            CompositionProducts = new HashSet<CompositionProduct>();

            ReleaseReplacementDetails = new HashSet<ReleaseReplacementDetail>();
        }

        public Detail(long code, string name, string designation, string gost) : this()
        {
            Code = code;
            Name = name;
            Designation = designation;
            Gost = gost;
        }

        public Detail(long code, string name, string designation, string gost, 
            Measure measure, TmcType tmcType, int warehouse) : this(code, name, designation, gost)
        {           
            Measure = measure;
            TmcType = tmcType;
            Warehouse = warehouse;
        }
    }
}