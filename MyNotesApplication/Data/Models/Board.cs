namespace MyNotesApplication.Data.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<Column>? Columns { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<UserBoardRole> UserBoardRoles { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<Note> Notes { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<InvitationToken> InvitationTokens { get; set;}
    }
}
