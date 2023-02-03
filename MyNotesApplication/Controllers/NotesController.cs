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
        public string Test()
        {
            var res = _noteRepository.Get(0);
            return res == null? "ok" : res.ToString();
        }

        [HttpGet]
        [Authorize]
        public async void AllNotes()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var username);
            username.ToString();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            if(user != null)
            {
                await HttpContext.Response.WriteAsJsonAsync( new {message = "ok" });
            }
            else
            {
                await HttpContext.Response.WriteAsJsonAsync(new { error = "notFound" });
            }
        }
        
    }
}
