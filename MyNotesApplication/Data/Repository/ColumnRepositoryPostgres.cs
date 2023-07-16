
using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class ColumnRepositoryPostgres : IRepository<Column>
    {
        private readonly MyDBContext _myDbContext;

        public ColumnRepositoryPostgres(MyDBContext myDbContext)
        {
            _myDbContext = myDbContext;
        }

        public Column Add(Column entity)
        {
            _myDbContext.Columns.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }

        public bool Delete(Column entity)
        {
            _myDbContext.Columns.Remove(entity);
            _myDbContext.SaveChanges();
            return true;
        }

        public Column Get(int id) => _myDbContext.Columns.FirstOrDefault(x => x.Id == id);

        public IEnumerable<Column> GetAll() => _myDbContext.Columns.ToList();

        public IEnumerable<Column> Get(Func<Column, bool> predicate) => _myDbContext.Columns.Where(predicate).ToList();

        public IEnumerable<Column> GetWithInclude(Func<Column, bool> predicate, params Expression<Func<Column, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.Where(predicate).ToList();
        }

        private IQueryable<Column> Include(params Expression<Func<Column, object>>[] includeProperties)
        {
            IQueryable<Column> query = _myDbContext.Columns;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();


        public Column Update(Column entity)
        {
            _myDbContext.Columns.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }

        
    }
}
