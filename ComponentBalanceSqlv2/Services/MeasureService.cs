using ComponentBalanceSqlv2.Data;
using ComponentBalanceSqlv2.Model;

namespace ComponentBalanceSqlv2.Services
{
    public class MeasureService
    {
        private readonly DomainContext _context;
        public MeasureService()
        {
            _context = new DomainContext();
        }
        public MeasureService(DomainContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Найти ед изм, если нет то добавить
        /// </summary>
        public Measure FindOrAddIfNotExists(int oldDbCode, string shortName)
        {
            var measure = new Measure(oldDbCode, shortName);
            var result = _context.Set<Measure>().FindFirstOrAddIfNotExists(measure, i => i.OldDbCode == oldDbCode);
            return result;
        }
    }
}
