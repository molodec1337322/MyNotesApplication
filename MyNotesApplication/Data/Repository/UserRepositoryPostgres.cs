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
            _myDBContext.Add(entity);
            return entity;
        }

        public bool Delete(int id)
        {
            _myDBContext.Remove(id);
            return true;
        }

        public User Get(int id) => _myDBContext.Users.Find(id);

        public List<User> GetAll()
        {
            return _myDBContext.Users.ToList();
        }

        public User Update(User entity)
        {
            _myDBContext.Update(entity);
            return entity;
        }
    }
}
