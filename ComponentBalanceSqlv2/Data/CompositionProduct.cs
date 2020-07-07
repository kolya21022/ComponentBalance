using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComponentBalanceSqlv2.Enums;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Запись таблицы [Состав изделий].
    /// </summary>
    public class CompositionProduct : BaseNotificationClass, IEnoughDetail
    {
        /// <summary>
        /// Уникальный ключ.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Цеха], 
        /// указывающий какого цеха деталь [Detail] входит в состав изделия [Product].
        /// </summary>
        public long WorkGuildId { get; set; }
        public virtual WorkGuild WorkGuild { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Изделия], 
        /// указывающий в состав какого изделия [Product] входит деталь [Detail].
        /// </summary>
        public long ProductId { get; set; }
        public virtual Product Product { get; set; }

        /// <summary>
        /// Вторичный ключ на таблицу [Деталей], 
        /// указывающий какая деталь [Detail] входит в состав изделия [Product].
        /// </summary>
        public long DetailId { get; set; }
        public virtual Detail Detail { get; set; }

        /// <summary>
        /// Кол-во деталей [Detail] входящей в состав одного изделия [Product].
        /// </summary>
        public decimal Count { get; set; }

        /// <summary>
        /// Флаг, указывающий что деталь [Detail] можно не включать в выпуск изделия [Product].
        /// TODO На данный момент разрешено у всех, в связи с беспорядками на заводе и не желанием вести правильную документацию конструкторами
        /// True - можно не включать деталь в выпуск изделия.
        /// False - обязательно должна присутствовать деталь в выпуске изделия.
        /// </summary>
        public bool IsCanDeleteInRelease { get; set; }

        /// <summary>
        /// Флаг, указывающий что можно использовать часть(которая имеется) нужных деталей [Detail] при выпуске изделия [Product], 
        /// вместо необходимого для выпуска изделия кол-ва [Count].
        /// True - можно использовать часть нужных деталей.
        /// False - нельзя использовать часть нужных деталей, обязательно использовать [Count] деталей.
        /// </summary>
        public bool IsCanUsePart { get; set; }

        
        private bool _isSelectedInRelease = true;
        /// <summary>
        /// Флаг, указывающий выбрана ли деталь [Detail] в проводимом выпуске изделия [Product].
        /// Стандартно выбранны все и только те у кого IsCanDeleteInRelease = true могут быть не выбраны. 
        /// True - деталь выбрана для текущего выпуска.
        /// False - деталь невыбрана для текущего выпуска.
        /// </summary>
        [NotMapped]
        public bool IsSelectedInRelease
        {
            get { return _isSelectedInRelease; }
            set
            {
                if (IsCanDeleteInRelease)
                {
                    _isSelectedInRelease = value;
                }
                OnPropertyChanged();
            }
        }

        public CompositionProduct() { }

        public CompositionProduct(WorkGuild workGuild, Product product, Detail detail, 
            decimal count, bool isCanDeleteInRelease, bool isCanUsePart)
        {
            WorkGuild = workGuild;
            Product = product;
            Detail = detail;
            Count = count;
            IsCanDeleteInRelease = isCanDeleteInRelease;
            IsCanUsePart = isCanUsePart;
        }

        private Enough _enough = Enough.Unknown;
        /// <summary>
        /// Указывает достаточно ли детали [Detail] на балансе для проводимого выпуска изделия [Product].
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