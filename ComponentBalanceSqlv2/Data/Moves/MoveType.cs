using System.ComponentModel.DataAnnotations;

namespace ComponentBalanceSqlv2.Data.Moves
{
    /// <summary>
    /// Запись таблицы [Вид движения].
    /// </summary>
    public class MoveType
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Флаг, указывающий может ли пользователь 
        /// сам провести движение данного типа.
        /// True - может сам провести.
        /// False - не может сам провести.
        /// </summary>
        public bool IsView { get; set; }
    }
}
