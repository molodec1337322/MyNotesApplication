
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data.Repository
{
    public class ColumnRepositoryPostgres : IRepository<Column>
    {
        private readonly MyDBContext _myDbContext;

        public ColumnRepositoryPostgres(MyDBContext myDbContext)
        {
            _myDbContext = myDbContext;
        }

        public Column Add(Column entity)
        {
            _myDbContext.Columns.Add(entity);
            return entity;
        }

        public bool Delete(Column entity)
        {
            _myDbContext.Columns.Remove(entity);
            return true;
        }

        public Column Get(int id) => _myDbContext.Columns.FirstOrDefault(x => x.Id == id);

        public IEnumerable<Column> GetAll() => _myDbContext.Columns.ToList();
            
        public async Task<int> SaveChanges() => await _myDbContext.SaveChangesAsync();


        public Column Update(Column entity)
        {
            _myDbContext.Columns.Add(entity);
            return entity;
        }
    }
}
