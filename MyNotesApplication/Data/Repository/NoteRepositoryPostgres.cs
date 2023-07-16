using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.Linq.Expressions;

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
            _myDBContext.SaveChanges();
            return entity;
        }

        public bool Delete(Note entity)
        {
            _myDBContext.Notes.Remove(entity);
            _myDBContext.SaveChanges();
            return true;
        }

        public Note Get(int id) => _myDBContext.Notes.FirstOrDefault(n => n.Id == id);

        public IEnumerable<Note> Get(Func<Note, bool> predicate) => _myDBContext.Notes.Where(predicate).ToList();

        public IEnumerable<Note> GetWithInclude(Func<Note, bool> predicate, params Expression<Func<Note, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.Where(predicate).ToList();
        }

        private IQueryable<Note> Include(params Expression<Func<Note, object>>[] includeProperties)
        {
            IQueryable<Note> query = _myDBContext.Notes;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IEnumerable<Note> GetAll() => _myDBContext.Notes.ToList();

        public async Task<int> SaveChanges() => await _myDBContext.SaveChangesAsync();

        public Note Update(Note entity)
        {
            _myDBContext.Notes.Update(entity);
            _myDBContext.SaveChanges();
            return entity;
        }
    }
}
