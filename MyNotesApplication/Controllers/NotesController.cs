using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyNotesApplication.Middlewares;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Notes")]
    public class NotesController : Controller
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _appConfiguration;
        private readonly ILogger<AuthController> _logger;

        public NotesController(IRepository<Note> noteRepo, IRepository<User> userRepo, IConfiguration appConfiguration, ILogger<AuthController> logger)
        {
            _noteRepository = noteRepo;
            _userRepository = userRepo;
            _appConfiguration = appConfiguration;
        }

        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task<IActionResult> AllNotes()
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            if (user == null) return Forbid();

            return Ok(_noteRepository.GetAll().Where(n => n.UserId == user.Id));
            
        }

        [HttpGet]
        [Route("GetLimit")]
        public async Task<IActionResult> GetNotesLimit()
        {
            return Ok(new {limit = _appConfiguration.GetValue<int>("NotesPerUser") });
        }

        [HttpGet]
        [Authorize]
        [Route("Get/{NoteId}")]
        public async Task<IActionResult> GetNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);

            if(note == null) return NotFound();

            if (note.UserId != user.Id) return Forbid();

            return Ok(_noteRepository.Get(NoteId));
        }

        /// <summary>
        /// Req {"Name" = "Note1", "Text" = "Note Text", "PathToFile": "pathToFile/Null", Type: "ToDo", "order": 1}
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
            newNote.Type = noteData.Type;
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

            if (note.UserId != user.Id) return Forbid();

            note.Name = updatedNoteData.Name;
            note.Text = updatedNoteData.Text;
            note.ChangedDate = DateTime.UtcNow;

            Note updatedNote = _noteRepository.Update(note);
            await _noteRepository.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Req {"Name" = "Note1", "Text" = "Note Text", "PathToFile": "pathToFile/Null", "order": 1}
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

            foreach(var note in notes){
                if(note.UserId != user.Id)
                {
                    return Forbid();
                }
            }

            foreach(var note in notes)
            {
                var newNote = notesData.notesList.First(item => note.Id == item.id);
                note.OrderPlace = newNote.order;
                note.Type = newNote.Type;
                _noteRepository.Update(note);
            }

            await _noteRepository.SaveChanges();

            return Ok();
        }

        [HttpPut]
        [Authorize]
        [Route("Done/{NoteId}")]
        public async Task<IActionResult> MarkNoteDone(int NoteId)
        {
            string username = GetUsernameFromJwtToken();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);

            if(note == null) return NoContent();

            if(note.UserId != user.Id) return Forbid();

            note.IsDone = true;
            note.DateDone = DateTime.UtcNow;

            Note doneNote = _noteRepository.Update(note);
            await _noteRepository.SaveChanges();

            return Ok(doneNote);
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
        
        public record NoteData(int id, string Name, string Text, string PathToFile, string Type, int order);
        public record NotesData(IEnumerable<NoteData> notesList);

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }
    }
}
