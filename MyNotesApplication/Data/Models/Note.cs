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
        public int OrderPlace { get; set; }

        public DateTime DateDone { get; set; }
        
        public int BoardId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Board Board { get; set; }
        public int ColumnId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Column Column { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IEnumerable<FileModel> Files { get; set; }
    }
}
