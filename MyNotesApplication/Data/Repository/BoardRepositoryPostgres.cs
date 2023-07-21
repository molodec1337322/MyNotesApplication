using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class BoardRepositoryPostgres : IRepository<Board>
    {
        private readonly MyDBContext _myDbContext;

        public BoardRepositoryPostgres(MyDBContext myDbContext)
        {
            _myDbContext = myDbContext;
        }

        public Board Add(Board entity)
        {
            _myDbContext.Boards.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }

        public bool Delete(Board entity)
        {
            _myDbContext.Boards.Remove(entity);
            _myDbContext.SaveChanges();
            return true;
        }

        public Board Get(int id) => _myDbContext.Boards.AsNoTracking().FirstOrDefault(b => b.Id == id);

        public IEnumerable<Board> GetAll() => _myDbContext.Boards.AsNoTracking().ToList();

        public IEnumerable<Board> Get(Func<Board, bool> predicate) => _myDbContext.Boards.AsNoTracking().Where(predicate).ToList();

        public IEnumerable<Board> GetWithInclude(Func<Board, bool> predicate, params Expression<Func<Board, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsNoTracking().Where(predicate).ToList();
        }

        private IQueryable<Board> Include(params Expression<Func<Board, object>>[] includeProperties)
        {
            IQueryable<Board> query = _myDbContext.Boards;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();

        public Board Update(Board entity)
        {
            _myDbContext.Boards.Update(entity);
            _myDbContext.SaveChanges(); 
            return entity;
        }
    }
}
