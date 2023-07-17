using Microsoft.EntityFrameworkCore;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Data
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
            
        }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ConfirmationToken> ConfirmationTokens { get; set; }
        public DbSet<FileModel> FileModels { get; set; }
        public DbSet<UserBoardRole> UserBoardRoles { get; set; }
        public DbSet<InvitationToken> InvitationTokens { get; set;}
        
    }
}
