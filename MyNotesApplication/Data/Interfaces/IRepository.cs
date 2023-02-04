namespace MyNotesApplication.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        T Add(T entity);
        T Update(T entity);
        bool Delete(T entity);
        Task<int> SaveChanges();
    }
}
