using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class PostgreGenericRepository<T> : IRepository<T> where T : class
    {
        private readonly MyDBContext _context;
        private DbSet<T> _dbSet { get; set; }

        public PostgreGenericRepository(MyDBContext context) 
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public T Add(T entity)
        {
            _dbSet.Add(entity); 
            _context.SaveChanges();
            return entity;
        }

        public bool Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public T Get(int id) => _dbSet.Find(id);

        public IEnumerable<T> Get(Func<T, bool> predicate) => _dbSet.AsNoTracking().Where(predicate);

        public IEnumerable<T> GetAll() => _dbSet.AsNoTracking();

        public IEnumerable<T> GetWithInclude(Func<T, bool> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsNoTracking().Where(predicate);
        }

        private IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public async Task<int> SaveChanges() => await _context.SaveChangesAsync();


        public T Update(T entity)
        {
            _dbSet.Update(entity);
            _context.SaveChanges();
            return entity;
        }
    }
}
