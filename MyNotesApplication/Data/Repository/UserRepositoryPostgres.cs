using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class UserRepositoryPostgres : IRepository<User>
    {
        private readonly MyDBContext _myDBContext;

        public UserRepositoryPostgres(MyDBContext myDBContext)
        {
            _myDBContext = myDBContext;
        }

        public User Add(User entity)
        {
            _myDBContext.Users.Add(entity);
            _myDBContext.SaveChanges();
            return entity;
        }

        public bool Delete(User entity)
        {
            _myDBContext.Users.Remove(entity);
            _myDBContext.SaveChanges();
            return true;
        }

        public User Get(int id) => _myDBContext.Users.Find(id);

        public IEnumerable<User> Get(Func<User, bool> predicate) => _myDBContext.Users.Where(predicate).ToList();

        public IEnumerable<User> GetWithInclude(Func<User, bool> predicate, params Expression<Func<User, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.Where(predicate).ToList();
        }

        private IQueryable<User> Include(params Expression<Func<User, object>>[] includeProperties)
        {
            IQueryable<User> query = _myDBContext.Users;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<User> GetAll() => _myDBContext.Users.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public User Update(User entity)
        {
            _myDBContext.Users.Update(entity);
            _myDBContext.SaveChanges(); 
            return entity;
        }
    }
}
