using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace ComponentBalanceSqlv2.Model
{
    public static class DbSetExtensions
    {
        /// <summary>
        /// Найти или добавить если отсутствует.
        /// </summary>
        public static T FindFirstOrAddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            if (predicate == null)
            {
                return null;
            }
            return dbSet.Any(predicate) ? dbSet.First(predicate) : dbSet.Add(entity);
        }
    }
}
