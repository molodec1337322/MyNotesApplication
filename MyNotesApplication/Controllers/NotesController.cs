
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static MyNotesApplication.Controllers.NotesController;
using Microsoft.AspNetCore.Identity;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Notes")]
    public class NotesController : Controller
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserBoardRole> _userBoardRoleRepository;
        private readonly IRepository<Board> _boardRepository;
        private readonly IRepository<Column> _columnRepository;
        private readonly IConfiguration _appConfiguration;
        private readonly ILogger<NotesController> _logger;

        public NotesController(
            IRepository<Note> noteRepo, 
            IRepository<User> userRepo, 
            IRepository<UserBoardRole> userBoardRoleRepository,
            IRepository<Board> boardRepository,
            IRepository<Column> columnRepository,
            IConfiguration appConfiguration, 
            ILogger<NotesController> logger
            )
        {
            _noteRepository = noteRepo;
            _userRepository = userRepo;
            _appConfiguration = appConfiguration;
            _userBoardRoleRepository = userBoardRoleRepository;
            _boardRepository = boardRepository;
            _columnRepository = columnRepository;
            _logger = logger;
        }

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }

        private bool IsUserAllowedToInteractWithBoard(User user, Board board)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == board.Id).FirstOrDefault();
            if (ubr == null) return false;
            return true;
        }

        private bool IsUserAllowedToInteractWithBoard(User user, Board board, UserBoardRoles role)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == board.Id && u.Role == role.ToString()).FirstOrDefault();
            if (ubr == null) return false;
            return true;
        }

        private bool IsUserAllowedToInteractWithNote(User user, Note note)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == note.BoardId).FirstOrDefault();
            if(ubr == null) return false;
            return true;
        }

        private bool IsUserAllowedToInteractWithNote(User user, Note note, UserBoardRoles role)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == note.BoardId && u.Role == UserBoardRoles.OWNER.ToString()).FirstOrDefault();
            if (ubr == null) return false;
            return true;
        }

        private bool IsColumnExists(int columnId, int boardId)
        {
            Column? column = _columnRepository.Get(c => c.BoardId == boardId && c.Id == columnId).FirstOrDefault();
            if(column == null) return false;
            return true;
        }


        [HttpGet]
        [Route("GetLimit")]
        public async Task<IActionResult> GetNotesLimit()
        {
            return Ok(new { limit = _appConfiguration.GetValue<int>("NotesPerBoard") });
        }

        /// <summary>
        /// Req {"Name": "Note1", "Text": "Note Text", "PathToFile": null, "BoardId": 1, "ColumnId": 1, "OrderPlace": 1}
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Add")]
        public async Task<IActionResult> AddNote()
        {
            string username = GetUsernameFromJwtToken();
            NewNoteData? newNoteData = await HttpContext.Request.ReadFromJsonAsync<NewNoteData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Note newNote = new Note();
            newNote.BoardId = newNoteData.BoardId;
            newNote.ColumnId = newNoteData.ColumnId;

            if (!IsUserAllowedToInteractWithNote(user, newNote, UserBoardRoles.OWNER)) return Forbid();
            if (!IsColumnExists(newNote.ColumnId, newNote.BoardId)) return BadRequest();

            int notesCount = _noteRepository.Get(n => n.BoardId == newNoteData.BoardId).Count();
            if (notesCount > _appConfiguration.GetValue<int>("NotesLimitPerBoard")) return Forbid();

            newNote.Text = newNoteData.Text;
            newNote.Name = newNoteData.Name;
            newNote.CreatedDate = DateTime.UtcNow;
            newNote.OrderPlace = newNoteData.OrderPlace;

            Note note = _noteRepository.Add(newNote);

            return Ok(note);
        }

        [HttpPut]
        [Authorize]
        [Route("Update/{NoteId}")]
        public async Task<IActionResult> UpdateNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault(u => u.Username == username);

            Note? note = _noteRepository.Get(NoteId);
            if(note == null) return BadRequest();

            note.ColumnId = updatedNoteData.ColumnId;

            if (!IsUserAllowedToInteractWithNote(user, note, UserBoardRoles.OWNER)) return Forbid();
            if (!IsColumnExists(note.ColumnId, note.BoardId)) return BadRequest();

            note.OrderPlace = updatedNoteData.OrderPlace;
            note.Name = updatedNoteData.Name;
            note.Text = updatedNoteData.Text;
            note.ChangedDate = DateTime.UtcNow;

            Note updatedNote = _noteRepository.Update(note);
            await _noteRepository.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Req {"BoardId": 1, "notes":[{"id": 1, "columnId": 1, "OrderPlace": 1}, {"id": 2, "columnId": 2, "OrderPlace": 2}]}
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [Route("UpdatePlacement")]
        public async Task<IActionResult> UpdateNotes()
        {
            string username = GetUsernameFromJwtToken();
            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);


            NoteOrderAndColumnUpdateDataList? notesData = await HttpContext.Request.ReadFromJsonAsync<NoteOrderAndColumnUpdateDataList>();

            Board? board = _boardRepository.Get(notesData.BoardId);

            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            List<NoteOrderAndColumnUpdateData> notesDataList = notesData.notes;
            List<Note> notesToUpdate = new List<Note>();
            List<Column> columns = new List<Column>();
            foreach (var noteData in notesDataList)
            {
                Note? note = _noteRepository.Get(n => n.Id == noteData.id && n.BoardId == board.Id).FirstOrDefault();

                Column? column = columns.FirstOrDefault(c => c.Id == noteData.ColumnId && c.BoardId == board.Id);
                if (column == null)
                {
                    column = _columnRepository.Get(c => c.Id == noteData.ColumnId && c.BoardId == board.Id).FirstOrDefault();
                    if (column == null) return BadRequest();
                    columns.Add(column);
                }
                if (note == null) return BadRequest();

                notesToUpdate.Add(note);
            }

            for(int i = 0; i < notesToUpdate.Count; i++)
            {
                notesToUpdate[i].ColumnId = notesDataList[i].ColumnId;
                notesToUpdate[i].Column = columns.FirstOrDefault(c => c.Id == notesDataList[i].ColumnId);
                notesToUpdate[i].OrderPlace = notesDataList[i].OrderPlace;

                _noteRepository.Update(notesToUpdate[i]);
            }

            return Ok(notesToUpdate);
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{NoteId}")]
        public async Task<IActionResult> DeleteNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();
            Note? note = _noteRepository.Get(NoteId);

            if (note == null) return BadRequest();
            if (!IsUserAllowedToInteractWithNote(user, note, UserBoardRoles.OWNER)) return Forbid();

            _noteRepository.Delete(note);

            return Ok(new { message = "deleted", NoteId = NoteId });
        }
        
        public record NoteData(int id, string Name, string Text, string PathToFile, int ColumnId, int OrderPlace);
        public record NotesData(IEnumerable<NoteData> notesList); 

        public record NoteOrderAndColumnUpdateData(int id, int ColumnId, int OrderPlace);
        public record NoteOrderAndColumnUpdateDataList(int BoardId, List<NoteOrderAndColumnUpdateData> notes);

        public record NewNoteData(string Name, string Text, string PathToFile, int BoardId, int ColumnId, int OrderPlace);
    }
}
