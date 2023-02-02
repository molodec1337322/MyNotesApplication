using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace MyNotesApplication.Controllers
{
    [Route("api/[controller]/[action]")]
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
        public string Test()
        {
            var res = _noteRepository.Get(0);
            return res == null? "ok" : res.ToString();
        }
    }
}
