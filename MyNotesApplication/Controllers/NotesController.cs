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
    [Route("api/Notes")]
    public class NotesController : Controller
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _appConfiguration;

        public NotesController(IRepository<Note> noteRepo, IRepository<User> userRepo, IConfiguration appConfiguration)
        {
            _noteRepository = noteRepo;
            _userRepository = userRepo;
            _appConfiguration = appConfiguration;
        }

        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task AllNotes()
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            await HttpContext.Response.WriteAsJsonAsync(_noteRepository.GetAll().Where(n => n.UserId == user.Id));
        }

        [HttpGet]
        [Authorize]
        [Route("Get/{NoteId}")]
        public async Task GetNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);
            if(note != null)
            {
                if(note.UserId == user.Id)
                {
                    await HttpContext.Response.WriteAsJsonAsync(_noteRepository.Get(NoteId));
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
                }
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
            }
        }

        /// <summary>
        /// Req {"Name" = "Note1", "Text" = "Note Text", "PathToFile": "pathToFile/Null"}
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [Route("Add")]
        public async Task AddNote()
        {
            string username = GetUsernameFromJwtToken();
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            if(noteData != null)
            {       
                Note newNote = new Note();
                newNote.Text = noteData.Text;
                newNote.Name = noteData.Name;
                newNote.CreatedDate = DateTime.UtcNow;
                newNote.UserId = user.Id;

                Note note = _noteRepository.Add(newNote);
                await _noteRepository.SaveChanges();

                await HttpContext.Response.WriteAsJsonAsync(newNote);
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "error"});
            }
        }

        [HttpPost]
        [Authorize]
        [Route("Update/{NoteId}")]
        public async Task UpdateNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);
            if (note != null)
            {
                if (note.UserId == user.Id)
                {
                    note.Name = updatedNoteData.Name;
                    note.Text = updatedNoteData.Text;
                    note.ChangedDate = DateTime.UtcNow;

                    Note updatedNote = _noteRepository.Update(note);
                    await _noteRepository.SaveChanges();

                    await HttpContext.Response.WriteAsJsonAsync(updatedNote);
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
                }
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("Done/{NoteId}")]
        public async Task MarkNoteDone(int NoteId)
        {
            string username = GetUsernameFromJwtToken();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);
            if (note != null)
            {
                if (note.UserId == user.Id)
                {
                    note.IsDone = true;
                    note.DateDone = DateTime.UtcNow;

                    Note doneNote = _noteRepository.Update(note);
                    await _noteRepository.SaveChanges();

                    await HttpContext.Response.WriteAsJsonAsync(doneNote);
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
                }
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{NoteId}")]
        public async Task DeleteNote(int NoteId)
        {
            string username = GetUsernameFromJwtToken();
            NoteData? updatedNoteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            Note? note = _noteRepository.Get(NoteId);
            if (note != null)
            {
                if (note.UserId == user.Id)
                {
                    _noteRepository.Delete(note);
                    await _noteRepository.SaveChanges();

                    await HttpContext.Response.WriteAsJsonAsync(new { message = "deleted"});
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
                }
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "notFound" });
            }
        }
        
        public record NoteData(string Name, string Text, string PathToFile);

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }
    }
}
