using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data.Repository
{
    public class FileModelRepositoryPostgres : IRepository<FileModel>
    {
        private readonly MyDBContext _dbContext;

        public FileModelRepositoryPostgres(MyDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public FileModel Add(FileModel entity)
        {
            _dbContext.FileModels.Add(entity);
            return entity;
        }

        public bool Delete(FileModel entity)
        {
            _dbContext.FileModels.Remove(entity);
            return true;
        }

        public FileModel Get(int id) => _dbContext.FileModels.FirstOrDefault(f => f.Id == id);

        public IEnumerable<FileModel> GetAll() => _dbContext.FileModels.ToList();

        public async Task<int> SaveChanges() => await _dbContext.SaveChangesAsync();

        public FileModel Update(FileModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
