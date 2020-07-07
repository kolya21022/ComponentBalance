using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Services
{
    public class BalanceService
    {
        private readonly DomainContext _context;
        
        public BalanceService()
        {
            _context = new DomainContext();
        }
        public BalanceService(DomainContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Найти баланс, если нет то добавить
        /// </summary>
        public Balance FindOrAddIfNotExists(long workGuildId, long detailId, MonthYear monthYear)
        {
            var balance = new Balance(workGuildId, detailId, monthYear);
            var result = _context.Set<Balance>().FindFirstOrAddIfNotExists(balance, i => i.WorkGuildId == workGuildId
                                                                                        && i.Month ==
                                                                                        monthYear.Month
                                                                                        && i.Year == monthYear.Year
                                                                                        && i.DetailId == detailId);
            return result;
        }

        /// <summary>
        /// Получить детали на балансе по их id или их коду.
        /// </summary>
        public IEnumerable<Balance> GetBalancesByIdDetailsAndCodeDetails(
            long workGuildId, MonthYear monthYear,
            IEnumerable<long> detailsId,
            IEnumerable<long> detailsCode)
        {
            var balances = _context.Balances
                .Where(i => i.WorkGuildId == workGuildId
                            && i.Month == monthYear.Month
                            && i.Year == monthYear.Year

                            && (detailsId.Contains(i.DetailId) || detailsCode.Contains(i.Detail.Code)))
                .Include(i => i.Detail)
                .AsNoTracking();
            return balances;
        }
    }
}
