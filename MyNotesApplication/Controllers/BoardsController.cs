using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Boards")]
    public class BoardsController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserBoardRole> _userBoardRepository;
        private readonly IRepository<Board> _boardRepository;
        private readonly IRepository<Column> _columnRepository;
        private readonly ILogger _logger;

        public BoardsController(
            IRepository<User> userRepository, 
            IRepository<UserBoardRole> userBoardRepository, 
            IRepository<Board> boardRepository, 
            IRepository<Column> columnRepository, 
            ILogger logger) 
        { 
            _userRepository = userRepository;
            _userBoardRepository = userBoardRepository;
            _boardRepository = boardRepository;
            _columnRepository = columnRepository;
            _logger = logger;
        }

    }
}
