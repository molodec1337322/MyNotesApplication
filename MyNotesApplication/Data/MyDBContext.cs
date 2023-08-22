using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data
{
    public class MyDBContext : DbContext, IDisposable
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
            Console.WriteLine("\nDBContext Created!!!\n");
        }

        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ConfirmationToken> ConfirmationTokens { get; set; }
        public DbSet<FileModel> FileModels { get; set; }
        public DbSet<UserBoardRole> UserBoardRoles { get; set; }
        public DbSet<InvitationToken> InvitationTokens { get; set;}
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            Console.WriteLine("\nDBContext Disposed!!!\n");
        }
    }
}
