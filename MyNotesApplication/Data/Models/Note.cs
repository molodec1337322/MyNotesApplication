namespace MyNotesApplication.Data.Models
{
    public class Note
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ChangedDate { get; set; }

        public bool IsDone { get; set; }

        public string Type { get; set; }
        public int OrderPlace { get; set; }

        public DateTime DateDone { get; set; }
        public int UserId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }
        public IEnumerable<FileModel> Files { get; set; }
    }

    public enum NoteType
    {
        ToDo,
        GotInWork,
        InProgress,
        Done
    }
}
