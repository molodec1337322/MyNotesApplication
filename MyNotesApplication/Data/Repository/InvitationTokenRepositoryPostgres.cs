using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class InvitationTokenRepositoryPostgres : IRepository<InvitationToken>
    {

        private readonly MyDBContext _myDbContext;

        public InvitationTokenRepositoryPostgres(MyDBContext dbContext)
        {
            _myDbContext = dbContext;
        }

        public InvitationToken Add(InvitationToken entity)
        {
            _myDbContext.InvitationTokens.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }

        public bool Delete(InvitationToken entity)
        {
            _myDbContext.InvitationTokens.Remove(entity);
            _myDbContext.SaveChanges();
            return true;
        }

        public InvitationToken Get(int id) => _myDbContext.InvitationTokens.AsNoTracking().FirstOrDefault(i => i.Id == id);

        public IEnumerable<InvitationToken> Get(Func<InvitationToken, bool> predicate) => _myDbContext.InvitationTokens.AsNoTracking().Where(predicate).ToList();

        public IEnumerable<InvitationToken> GetAll() => _myDbContext.InvitationTokens.AsNoTracking().ToList();

        public IEnumerable<InvitationToken> GetWithInclude(Func<InvitationToken, bool> predicate, params Expression<Func<InvitationToken, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsNoTracking().Where(predicate).ToList();
        }

        private IQueryable<InvitationToken> Include(params Expression<Func<InvitationToken, object>>[] includeProperties)
        {
            IQueryable<InvitationToken> query = _myDbContext.InvitationTokens.AsNoTracking();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();

        public InvitationToken Update(InvitationToken entity)
        {
            _myDbContext.InvitationTokens.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }
    }
}
