using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class UserBoardRoleRepositoryPostgres : IRepository<UserBoardRole>
    {
        private readonly MyDBContext _myDbContext;

        public UserBoardRoleRepositoryPostgres(MyDBContext dbContext)
        {
            _myDbContext = dbContext;
        }

        public UserBoardRole Add(UserBoardRole entity)
        {
            _myDbContext.UserBoardRoles.Add(entity);
            _myDbContext.SaveChanges();
            return entity;
        }

        public bool Delete(UserBoardRole entity)
        {
            _myDbContext.UserBoardRoles.Remove(entity);
            _myDbContext.SaveChanges();
            return true;
        }

        public UserBoardRole Get(int id) => _myDbContext.UserBoardRoles.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public IEnumerable<UserBoardRole> Get(Func<UserBoardRole, bool> predicate) => _myDbContext.UserBoardRoles.AsNoTracking().Where(predicate).ToList();

        public IEnumerable<UserBoardRole> GetWithInclude(Func<UserBoardRole, bool> predicate, params Expression<Func<UserBoardRole, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsNoTracking().Where(predicate).ToList();
        }

        private IQueryable<UserBoardRole> Include(params Expression<Func<UserBoardRole, object>>[] includeProperties)
        {
            IQueryable<UserBoardRole> query = _myDbContext.UserBoardRoles.AsNoTracking();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<UserBoardRole> GetAll() => _myDbContext.UserBoardRoles.AsNoTracking().ToList();

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();

        public UserBoardRole Update(UserBoardRole entity)
        {
            _myDbContext.UserBoardRoles.Update(entity);
            _myDbContext.SaveChanges();
            return entity;
        }
    }
}
