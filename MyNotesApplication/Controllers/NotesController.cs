using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> AllNotes()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var username);
            username.ToString();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            await HttpContext.Response.WriteAsJsonAsync(_noteRepository.GetAll().Where(n => n.UserId == user.Id));

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("/{NoteId}")]
        public async Task<IActionResult> GetNote(int NoteId)
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
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }
        
    }
}
