namespace MyNotesApplication.Data.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrderPlace { get; set; }
        public IEnumerable<Note>? Notes { get; set; }
        public int BoardId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Board Board { get; set; }
    }
}
