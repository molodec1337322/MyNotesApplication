using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq;

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
            return entity;
        }

        public bool Delete(User entity)
        {
            _myDBContext.Users.Remove(entity);
            return true;
        }

        public User Get(int id) => _myDBContext.Users.Find(id);

        public IEnumerable<User> GetAll() => _myDBContext.Users.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public User Update(User entity)
        {
            _myDBContext.Users.Update(entity);
            return entity;
        }
    }
}
