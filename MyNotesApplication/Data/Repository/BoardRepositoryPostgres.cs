using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

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
            return entity;
        }

        public bool Delete(Board entity)
        {
            _myDbContext.Boards.Remove(entity);
            return true;
        }

        public Board Get(int id) => _myDbContext.Boards.FirstOrDefault(x => x.Id == id);

        public IEnumerable<Board> GetAll() => _myDbContext.Boards.ToList();

        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();

        public Board Update(Board entity)
        {
            _myDbContext.Boards.Update(entity);
            return entity;
        }
    }
}
