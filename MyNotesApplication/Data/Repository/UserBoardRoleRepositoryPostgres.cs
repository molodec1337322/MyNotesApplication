using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

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
            return entity;
        }

        public bool Delete(UserBoardRole entity)
        {
            _myDbContext.UserBoardRoles.Remove(entity);
            return true;
        }

        public UserBoardRole Get(int id) => _myDbContext.UserBoardRoles.FirstOrDefault(x => x.Id == id);

        public IEnumerable<UserBoardRole> GetAll() => _myDbContext.UserBoardRoles.ToList();

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();

        public UserBoardRole Update(UserBoardRole entity)
        {
            _myDbContext.UserBoardRoles.Update(entity);
            return entity;
        }
    }
}
