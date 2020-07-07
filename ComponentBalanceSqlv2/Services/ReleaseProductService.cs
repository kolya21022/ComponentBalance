using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Services
{
    public class ReleaseProductService
    {
        private readonly DomainContext _context;

        public ReleaseProductService()
        {
            _context = new DomainContext();
        }
        public ReleaseProductService(DomainContext context)
        {
            _context = context;
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Найти выпуск по номеру, цеху и месяцу году, если нет то добавить.
        /// </summary>
        public ReleaseProduct FindOrAddIfNotExists(long workGuildId,
            long productId, string factoryNumber, MonthYear monthYear)
        {
            var releaseProduct = new ReleaseProduct(workGuildId, productId, factoryNumber, monthYear);
            var result = _context.Set<ReleaseProduct>()
                .FindFirstOrAddIfNotExists(releaseProduct,
                    i => i.FactoryNumber == factoryNumber
                         && i.Month == monthYear.Month
                         && i.Year == monthYear.Year
                         && i.WorkGuildId == workGuildId);
            return result;
        }
    }
}
