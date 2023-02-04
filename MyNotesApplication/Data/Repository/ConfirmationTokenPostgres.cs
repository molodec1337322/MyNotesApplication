using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data.Repository
{
    public class ConfirmationTokenPostgres : IRepository<ConfirmationToken>
    {

        private readonly MyDBContext _myDBContext;

        public ConfirmationTokenPostgres(MyDBContext myDBContext)
        {
            _myDBContext = myDBContext;
        }

        public ConfirmationToken Add(ConfirmationToken entity)
        {
            _myDBContext.Add(entity);
            return entity;
        }

        public bool Delete(ConfirmationToken entity)
        {
            _myDBContext.Remove(entity);
            return true;
        }

        public ConfirmationToken Get(int id) => _myDBContext.ConfirmationTokens.Find(id);

        public IEnumerable<ConfirmationToken> GetAll() => _myDBContext.ConfirmationTokens.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public ConfirmationToken Update(ConfirmationToken entity)
        {
            _myDBContext.ConfirmationTokens.Update(entity);
            return entity;
        }
    }
}
