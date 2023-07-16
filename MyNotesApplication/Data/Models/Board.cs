namespace MyNotesApplication.Data.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public int UserBoardId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public UserBoardRole UserBoard { get; set; }
    }
}
