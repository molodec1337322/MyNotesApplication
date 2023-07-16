namespace MyNotesApplication.Data.Models
{
    public class ConfirmationToken
    {
        public int Id { get; set; }
        public string ConfirmationGUID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int UserId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
    }
}
