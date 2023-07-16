using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static MyNotesApplication.Controllers.NotesController;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Boards")]
    public class BoardsController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserBoardRole> _userBoardRoleRepository;
        private readonly IRepository<Board> _boardRepository;
        private readonly IRepository<Column> _columnRepository;
        private readonly ILogger<BoardsController> _logger;

        public BoardsController(
            IRepository<User> userRepository, 
            IRepository<UserBoardRole> userBoardRoleRepository, 
            IRepository<Board> boardRepository, 
            IRepository<Column> columnRepository, 
            ILogger<BoardsController> logger) 
        { 
            _userRepository = userRepository;
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
            UserBoardRole? ubr = _userBoardRoleRepository.GetAll().FirstOrDefault(u => u.UserId == user.Id && u.BoardId == board.Id);
            if (ubr == null) return false;
            return true;
        }

        private bool IsUserAllowedToInteractWithBoard(User user, Board board, UserBoardRoles role)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.GetAll().FirstOrDefault(u => u.UserId == user.Id && u.BoardId == board.Id && u.Role == role.ToString());
            if (ubr == null) return false;
            return true;
        }

        /// <summary>
        /// req {}
        /// res [{"id": 1, "name": "gfdsggf"}, {"id": 2, "name": "gffdgdfgf"}]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("AllOwned")]
        public async Task<IActionResult> GetBoards()
        {
            string username = GetUsernameFromJwtToken();
            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            List<UserBoardRole> ubrList = _userBoardRoleRepository.GetAll().Where(u => u.UserId == user.Id && u.Role == UserBoardRoles.OWNER.ToString()).ToList();
            List<Board> boardList = new List<Board>();//_boardRepository.GetAll().Where(b => ).ToList();
            foreach (var ubr in ubrList)
            {
                boardList.Add(_boardRepository.Get(ubr.BoardId));
            }

            return Ok(boardList);
        }

        [HttpGet]
        [Authorize]
        [Route("Get/{BoardId}")]
        public async Task<IActionResult> GetBoard(int BoardId)
        {
            string username = GetUsernameFromJwtToken();

            Board? board = _boardRepository.Get(BoardId);
            if (board == null) return NotFound();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            if (!IsUserAllowedToInteractWithBoard(user, board)) return Forbid();

            return Ok(board);
        }

        /// <summary>
        /// req {"Name": "fsdgdfg"}
        /// res {"id": , "name": "fsgfds"}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Create")]
        public async Task<IActionResult> CreateBoard()
        {
            string username = GetUsernameFromJwtToken();

            NewBoardData? boardData = await HttpContext.Request.ReadFromJsonAsync<NewBoardData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            Board newBoard = new Board();
            newBoard.Name = boardData.Name;

            Board board = _boardRepository.Add(newBoard);
            await _boardRepository.SaveChanges();

            UserBoardRole newUBR = new UserBoardRole();
            newUBR.Board = board;
            newUBR.UserId = user.Id;
            newUBR.Role = UserBoardRoles.OWNER.ToString();

            UserBoardRole ubr = _userBoardRoleRepository.Add(newUBR);
            await _boardRepository.SaveChanges();

            return Ok(board);
        }

        [HttpPost]
        [Authorize]
        [Route("AddGuest/{BoardId}")]
        public async Task<IActionResult> AddGuest()
        {
            return Ok();
        }

        [HttpPut]
        [Authorize]
        [Route("AddOwner/{BoardId}")]
        public async Task<IActionResult> AddOwner()
        {
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{BoardId}")]
        public async Task<IActionResult> DeleteBoard(int BoardId)
        {
            return Ok();
        }

        public record BoardData(int id, string Name);


        public record NewBoardData(string Name);
    }
}
