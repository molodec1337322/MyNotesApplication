namespace MyNotesApplication.Data.Models
{
    public class UserBoardRole
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
        public int BoardId {get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Board Board { get; set; }
    }

    public enum UserBoardRoles
    {
        OWNER,
        GUEST
    }
}
