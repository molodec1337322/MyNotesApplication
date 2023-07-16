using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Columns")]
    public class ColumnsController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserBoardRole> _userBoardRoleRepository;
        private readonly IRepository<Board> _boardRepository;
        private readonly IRepository<Column> _columnRepository;
        private readonly ILogger<ColumnsController> _logger;

        public ColumnsController(
            IRepository<User> userRepository,
            IRepository<UserBoardRole> userBoardRoleRepository,
            IRepository<Board> boardRepository,
            IRepository<Column> columnRepository,
            ILogger<ColumnsController> logger)
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
        /// req url://.../AllColumnsFromBoard/{BoardId}
        /// res [{"id": 1, "name": "fsdf", "orderPlace": 1, "notes": null, "boardId": 1}, {"id": 1, "name": "fsdf", "orderPlace": 1, "notes": null, "boardId": 1}]
        /// </summary>
        /// <param name="BoardId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("AllColumnsFromBoard/{BoardId}")]
        public async Task<IActionResult> GetColumnsOfBoard(int BoardId)
        {
            string username = GetUsernameFromJwtToken();

            Board? board = _boardRepository.Get(BoardId);
            if (board == null) return BadRequest();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            if(!IsUserAllowedToInteractWithBoard(user, board)) return Forbid();

            List<Column> columns = _columnRepository.GetAll().Where(c => c.BoardId == BoardId).ToList();

            return Ok(columns);
        }

        /// <summary>
        /// req {"Name": "sdf", "OrderPlace": 1, "BoardId": 1}
        /// res {"id": 1, "name": "fsdf", "orderPlace": 1, "notes": null, "boardId": 1}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            string username = GetUsernameFromJwtToken();
            NewColumnData? newColumnData = await HttpContext.Request.ReadFromJsonAsync<NewColumnData>();

            User? user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);

            Board? board = _boardRepository.Get(newColumnData.BoardId);
            if (board == null) return BadRequest();
            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            Column newColumn = new Column();
            newColumn.BoardId = newColumn.BoardId;
            newColumn.Board = board;
            newColumn.OrderPlace = newColumnData.OrderPlace;
            newColumn.Name = newColumnData.Name;

            Column column = _columnRepository.Add(newColumn);
            await _columnRepository.SaveChanges();

            return Ok(column);
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{ColumnId}")]
        public async Task<IActionResult> Delete()
        {
            return Ok();
        }

        [HttpPut]
        [Authorize]
        [Route("UpdateColumns")]
        public async Task<IActionResult> Update()
        {
            return Ok();
        }

        public record NewColumnData (string Name, int OrderPlace, int BoardId);
    }
}
