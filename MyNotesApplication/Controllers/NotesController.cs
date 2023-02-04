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

        public NotesController(IRepository<Note> noteRepo, IRepository<User> userRepo)
        {
            _noteRepository = noteRepo;
            _userRepository = userRepo;
        }

        [HttpGet]
        [Authorize]
        [Route("")]
        public async Task AllNotes()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var username);
            username.ToString();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            await HttpContext.Response.WriteAsJsonAsync(_noteRepository.GetAll().Where(n => n.UserId == user.Id));
        }

        [HttpGet]
        [Authorize]
        [Route("Get/{NoteId}")]
        public async Task GetNote(int NoteId)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var username);
            username.ToString();

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
        /// Req {"Name" = "Note1", "Text" = "Note Text"}
        /// Res {"message" = "ok"}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Add")]
        public async Task AddNote()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            NoteData? noteData = await HttpContext.Request.ReadFromJsonAsync<NoteData>();
            token = token.ToString().Split(" ")[1];
            string username = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;

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
        
        public record NoteData(string Name, string Text);
    }
}
