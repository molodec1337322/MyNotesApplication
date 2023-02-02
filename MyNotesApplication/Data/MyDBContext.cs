using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
