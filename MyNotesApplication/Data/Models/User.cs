using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace MyNotesApplication.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public IEnumerable<UserBoardRole> UserBoards { get; set; }

        public ConfirmationToken ConfirmationToken { get; set; }
        public IEnumerable<InvitationToken> InvitationTokens { get; set;}
    }
}
