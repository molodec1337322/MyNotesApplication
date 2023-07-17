namespace MyNotesApplication.Data.Models
{
    public class InvitationToken
    {
        public int Id { get; set; }
        public string InvitationGUID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int BoardId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Board Board { get; set; }
        public int UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
    }
}
