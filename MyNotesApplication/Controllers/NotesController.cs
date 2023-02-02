using MyNotesApplication.Data;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Controllers
{
    public class NotesController
    {
        private readonly IRepository<Note> _noteRepository;
        private readonly IRepository<User> _userRepository;

        public NotesController(IRepository<Note> noteRepo, IRepository<User> userRepo)
        {
            _noteRepository = noteRepo;
            _userRepository = userRepo;
        }
    }
}
