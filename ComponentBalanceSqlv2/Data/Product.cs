using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ComponentBalanceSqlv2.Data.Moves;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись в таблице [Изделия].
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Короткий код изделия.
        /// </summary>
        public int ShortCode => (int)(Code / 100000000000);

        /// <summary>
        /// Заводской код изделия.
        /// </summary>
        public long Code { get; set; }

        /// <summary>
        /// Название изделия.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Обозначение изделия.
        /// </summary>
        public string Designation { get; set; }

        public virtual ICollection<ImportWarehouseMove> ImportWarehouseMoves { get; set; }
        public virtual ICollection<ReleaseProduct> ReleaseProducts { get; set; }
        public virtual ICollection<CompositionProduct> CompositionProducts { get; set; }

        public Product()
        {
            ImportWarehouseMoves = new HashSet<ImportWarehouseMove>();
            ReleaseProducts = new HashSet<ReleaseProduct>();
            CompositionProducts = new HashSet<CompositionProduct>();
        }

        public Product(long code, string name, string designation) : this()
        {
            Code = code;
            Name = name;
            Designation = designation;
        }
    }
}
