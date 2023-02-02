using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data.Repository
{
    public class NoteRepositoryPostgres : IRepository<Note>
    {
        private readonly MyDBContext _myDBContext;

        public NoteRepositoryPostgres(MyDBContext myDBContext)
        {
            _myDBContext = myDBContext;
        }

        public Task<Note> Add(Note entity)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Note> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Note>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Note> Update(Note entity)
        {
            throw new NotImplementedException();
        }
    }
}
