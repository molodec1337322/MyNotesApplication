using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ConfirmationToken> ConfirmationTokens { get; set; }
    }
}
