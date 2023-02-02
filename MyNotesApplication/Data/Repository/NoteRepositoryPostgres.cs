using Microsoft.EntityFrameworkCore;
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

        public Note Add(Note entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Note Get(int id)
        {
            return _myDBContext.Notes.FirstOrDefault(n => n.Id == id);
        }

        public List<Note> GetAll()
        {
            throw new NotImplementedException();
        }

        public Note Update(Note entity)
        {
            throw new NotImplementedException();
        }
    }
}
