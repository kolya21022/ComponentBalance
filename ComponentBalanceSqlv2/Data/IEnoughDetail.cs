using ComponentBalanceSqlv2.Enums;

namespace ComponentBalanceSqlv2.Data
{
    /// <summary>
    /// Интерфейс показывающий достаточно ли деталей на балансе цеха.
    /// </summary>
    public interface IEnoughDetail
    {
        Enough Enough { get; set; }
    }
}
