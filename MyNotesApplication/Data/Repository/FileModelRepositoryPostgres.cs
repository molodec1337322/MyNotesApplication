using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

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
            _dbContext.SaveChanges();
            return entity;
        }

        public bool Delete(FileModel entity)
        {
            _dbContext.FileModels.Remove(entity);
            _dbContext.SaveChanges();
            return true;
        }

        public FileModel Get(int id) => _dbContext.FileModels.FirstOrDefault(f => f.Id == id);

        public IEnumerable<FileModel> Get(Func<FileModel, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileModel> GetAll() => _dbContext.FileModels.ToList();

        public IEnumerable<FileModel> GetWithInclude(Func<FileModel, bool> predicate, params Expression<Func<FileModel, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public async Task<int> SaveChanges() => await _dbContext.SaveChangesAsync();

        public FileModel Update(FileModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
