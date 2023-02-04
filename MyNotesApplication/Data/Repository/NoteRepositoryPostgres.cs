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
            _myDBContext.Notes.Add(entity);
            return entity;
        }

        public bool Delete(Note entity)
        {
            _myDBContext.Notes.Remove(entity);
            return true;
        }

        public Note Get(int id) => _myDBContext.Notes.FirstOrDefault(n => n.Id == id);

        public IEnumerable<Note> GetAll() => _myDBContext.Notes.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public Note Update(Note entity)
        {
            _myDBContext.Notes.Update(entity);
            return entity;
        }
    }
}
