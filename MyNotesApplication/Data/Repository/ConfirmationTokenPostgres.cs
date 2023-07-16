using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class ConfirmationTokenPostgres : IRepository<ConfirmationToken>
    {

        private readonly MyDBContext _myDBContext;

        public ConfirmationTokenPostgres(MyDBContext myDBContext)
        {
            _myDBContext = myDBContext;
        }

        public ConfirmationToken Add(ConfirmationToken entity)
        {
            _myDBContext.Add(entity);
            _myDBContext.SaveChanges();
            return entity;
        }

        public bool Delete(ConfirmationToken entity)
        {
            _myDBContext.Remove(entity);
            _myDBContext.SaveChanges();
            return true;
        }

        public ConfirmationToken Get(int id) => _myDBContext.ConfirmationTokens.Find(id);

        public IEnumerable<ConfirmationToken> Get(Func<ConfirmationToken, bool> predicate) => _myDBContext.ConfirmationTokens.Where(predicate).ToList();

        public IEnumerable<ConfirmationToken> GetWithInclude(Func<ConfirmationToken, bool> predicate, params Expression<Func<ConfirmationToken, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.Where(predicate).ToList();
        }

        private IQueryable<ConfirmationToken> Include(params Expression<Func<ConfirmationToken, object>>[] includeProperties)
        {
            IQueryable<ConfirmationToken> query = _myDBContext.ConfirmationTokens;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<ConfirmationToken> GetAll() => _myDBContext.ConfirmationTokens.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public ConfirmationToken Update(ConfirmationToken entity)
        {
            _myDBContext.ConfirmationTokens.Update(entity);
            _myDBContext.SaveChanges();
            return entity;
        }
    }
}
