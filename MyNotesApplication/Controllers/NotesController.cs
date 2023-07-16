
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        private readonly ILogger<AuthController> _logger;

        public NotesController(
            IRepository<Note> noteRepo, 
            IRepository<User> userRepo, 
            IRepository<UserBoardRole> userBoardRoleRepository,
            IRepository<Board> boardRepository,
            IRepository<Column> columnRepository,
            IConfiguration appConfiguration, 
            ILogger<AuthController> logger
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


        //потом затестить оба метода и чекнуть, что быстрее
        private bool IsUserAllowedToInteractWithNote(User user, Note note, UserBoardRoles role)
        {
            UserBoardRole? userBoardRole = _userBoardRoleRepository.GetAll().FirstOrDefault(ubr => ubr.UserId == user.Id && ubr.BoardId == 
                _boardRepository.GetAll().FirstOrDefault(b => b.Id == 
                _columnRepository.GetAll().FirstOrDefault(c => c.Id == note.ColumnId).BoardId).UserBoardId);

            if(userBoardRole != null && userBoardRole.Role == role.ToString())
            {
                return true;
            }
            return false;
        }

        private bool IsUserAllowedToInteractWithNotes(User user, List<Note> notes, UserBoardRoles role)
        {
            Note firstNote = notes[0];
            Board board = _boardRepository.Get(_columnRepository.Get(firstNote.ColumnId).BoardId);
            UserBoardRole? ubr = _userBoardRoleRepository.GetAll().FirstOrDefault(u => u.BoardId == board.UserBoardId && u.UserId == user.Id && u.Role == role.ToString());

            if(ubr == null) return false;

            List<Column> columns = _columnRepository.GetAll().Where(c => c.BoardId == board.Id).ToList();
            List<Note> allNotesInBoard = new List<Note>();
            foreach (var col in columns)
            {
                List<Note> tmpNotes = _noteRepository.GetAll().Where(n => n.ColumnId == col.Id).ToList();
                foreach(var n in tmpNotes)
                {
                    allNotesInBoard.Add(n);
                }
                
            }

            if (allNotesInBoard.Count != notes.Count) return false;

            foreach(var trustedNote in allNotesInBoard)
            {
                if(!notes.Contains(trustedNote)) return false;
            }

            return true;
        }

        private bool IsNoteInBoard(Board board, Note note)
        {
            Column? col = _columnRepository.GetAll().FirstOrDefault(c => c.BoardId == board.Id && note.ColumnId == c.Id);

            if (col == null) return false;
            return true;
        }

        [HttpGet]
        [Route("GetLimit")]
        public async Task<IActionResult> GetNotesLimit()
        {
            return Ok(new { limit = _appConfiguration.GetValue<int>("NotesPerBoard") });
        }

        /// <summary>
        /// Req {"Name" = "Note1", "Text" = "Note Text", "PathToFile": "pathToFile/Null", ColumnId: 1, "order": 1}
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Add")]
        public async Task<IActionResult> AddNote()
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            if(noteData == null) return NoContent();

            if (_noteRepository.GetAll().Where(note => note.UserId == user.Id).ToList().Count >= _appConfiguration.GetValue<int>("NotesLimitPerUser")) return Forbid();

            Note newNote = new Note();
            newNote.Text = noteData.Text;
            newNote.Name = noteData.Name;
            newNote.CreatedDate = DateTime.UtcNow;
            newNote.ColumnId = noteData.ColumnId;
            newNote.OrderPlace = noteData.order;
            newNote.UserId = user.Id;

            Note note = _noteRepository.Add(newNote);
            await _noteRepository.SaveChanges();

            return Ok(newNote);
        }

        [HttpPut]
        [Authorize]
        [Route("Update/{NoteId}")]
        public async Task<IActionResult> UpdateNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);

            if(note == null) return NoContent();

            if(updatedNoteData == null) return NotFound();

            if(note.UserId != user.Id) return Forbid();

            note.Name = updatedNoteData.Name;
            note.Text = updatedNoteData.Text;
            note.ChangedDate = DateTime.UtcNow;

            Note updatedNote = _noteRepository.Update(note);
            await _noteRepository.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Req [{"Name" = "Note1", "Text" = "Note Text", "PathToFile": "pathToFile/Null", "columnId": 1}]
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [Route("Update")]
        public async Task<IActionResult> UpdateNotes()
        {
            string username = GetUsernameFromJwtToken();
            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            NotesData? notesData = await HttpContext.Request.ReadFromJsonAsync<NotesData>();
            List<int> noteIds = new List<int>();
            int noteId;

            if (notesData == null) return NoContent();

            foreach(var item in notesData.notesList)
            {
                noteIds.Add(item.id);
            }

            List<Note> notes = _noteRepository.GetAll().Where(item => noteIds.Contains(item.Id)).ToList();

            if (!IsUserAllowedToInteractWithNotes(user, notes, UserBoardRoles.OWNER)) return Forbid();
            /*
            foreach(var note in notes){
                if(IsUserAllowedToInteractWithNote(user, note, UserBoardRoles.GUEST))
                {
                    return Forbid();
                }
            }
            */

            foreach(var note in notes)
            {
                var newNote = notesData.notesList.FirstOrDefault(item => note.Id == item.id);
                note.OrderPlace = newNote.order;
                note.ColumnId = newNote.ColumnId;
                _noteRepository.Update(note);
            }

            await _noteRepository.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{NoteId}")]
        public async Task<IActionResult> DeleteNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);

            if(note == null) return NoContent();

            if (note.UserId != user.Id) return Forbid();

            _noteRepository.Delete(note);
            await _noteRepository.SaveChanges();

            return Ok(new { message = "deleted", NoteId = NoteId });
        }
        
        public record NoteData(int id, string Name, string Text, string PathToFile, int ColumnId, int order);
        public record NotesData(IEnumerable<NoteData> notesList);

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }
    }
}
