using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq;
using System.Linq.Expressions;

namespace MyNotesApplication.Data.Repository
{
    public class UserRepositoryPostgres : IRepository<User>, IDisposable
    {
        private readonly MyDBContext _myDBContext;
        private bool _disposed;

        public UserRepositoryPostgres(MyDBContext myDBContext)
        {
            _myDBContext = myDBContext;
            _disposed = false;
        }

        public User Add(User entity)
        {
            _myDBContext.Users.Add(entity);
            _myDBContext.SaveChanges();
            _myDBContext.DisposeDbset<User>();
            return entity;
        }

        public bool Delete(User entity)
        {
            _myDBContext.Users.Remove(entity);
            _myDBContext.SaveChanges();
            _myDBContext.DisposeDbset<User>();
            return true;
        }

        public User Get(int id) => _myDBContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == id); 

        public IEnumerable<User> Get(Func<User, bool> predicate) => _myDBContext.Users.AsNoTracking().Where(predicate).ToList();

        public IEnumerable<User> GetWithInclude(Func<User, bool> predicate, params Expression<Func<User, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsNoTracking().Where(predicate).ToList();
        }

        private IQueryable<User> Include(params Expression<Func<User, object>>[] includeProperties)
        {
            IQueryable<User> query = _myDBContext.Users.AsNoTracking();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<User> GetAll() => _myDBContext.Users.AsNoTracking().ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public User Update(User entity)
        {
            _myDBContext.Users.Update(entity);
            _myDBContext.SaveChanges();
            _myDBContext.DisposeDbset<User>();
            return entity;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            try
            {
                if(_myDBContext != null) _myDBContext.Dispose();
            }
            catch(Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
