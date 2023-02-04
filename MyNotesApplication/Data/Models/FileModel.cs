namespace MyNotesApplication.Data.Models
{
    //назван с суффиксом Model для того, чтобы не путать с системным классом File
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Format { get; set; }
        public int NoteId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Note Note { get; set; }
    }
}
