namespace MyNotesApplication.Data.Models
{
    public class Note
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsDone { get; set; }

        public DateTime DateDone { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
