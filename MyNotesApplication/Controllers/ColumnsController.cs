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
        private readonly IRepository<Note> _notesRepository;
        private readonly ILogger<ColumnsController> _logger;

        public ColumnsController(
            IRepository<User> userRepository,
            IRepository<UserBoardRole> userBoardRoleRepository,
            IRepository<Board> boardRepository,
            IRepository<Column> columnRepository,
            IRepository<Note> notesRepository,
            ILogger<ColumnsController> logger)
        {
            _userRepository = userRepository;
            _userBoardRoleRepository = userBoardRoleRepository;
            _boardRepository = boardRepository;
            _columnRepository = columnRepository;
            _notesRepository = notesRepository;
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
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == board.Id).FirstOrDefault();
            if (ubr == null) return false;
            return true;
        }

        private bool IsUserAllowedToInteractWithBoard(User user, Board board, UserBoardRoles role)
        {
            UserBoardRole? ubr = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.BoardId == board.Id && u.Role == role.ToString()).FirstOrDefault();
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
        [Route("FromBoard/{BoardId}")]
        public async Task<IActionResult> GetColumnsOfBoard(int BoardId)
        {
            string username = GetUsernameFromJwtToken();

            Board? board = _boardRepository.Get(BoardId);
            if (board == null) return BadRequest();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();
            if(!IsUserAllowedToInteractWithBoard(user, board)) return Forbid();

            List<Column> columns = _columnRepository.GetWithInclude(c => c.BoardId == BoardId).ToList();
            List<Note> notes = _notesRepository.Get(n => n.BoardId == BoardId).ToList();

            List<Note> notesInColumns = new List<Note>();

            foreach(var column in columns)
            {
                foreach(var note in notes)
                {
                    notesInColumns = notes.Where(n => n.ColumnId == column.Id).ToList();
                }
                column.Notes = notesInColumns;
            }

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

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Board? board = _boardRepository.Get(newColumnData.BoardId);
            if (board == null) return BadRequest();
            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            Column newColumn = new Column();
            newColumn.BoardId = newColumn.BoardId;
            newColumn.Board = board;
            newColumn.OrderPlace = newColumnData.OrderPlace;
            newColumn.Name = newColumnData.Name;

            Column column = _columnRepository.Add(newColumn);

            return Ok(column);
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{columnId}")]
        public async Task<IActionResult> Delete(int columnId)
        {
            string username = GetUsernameFromJwtToken();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Column? column = _columnRepository.Get(columnId);
            if (column == null) return BadRequest();

            Board? board = _boardRepository.Get(column.BoardId);
            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            _columnRepository.Delete(column);

            return Ok(new {message = "deleted", ColumnId = columnId});
        }

        [HttpPut]
        [Authorize]
        [Route("UpdateColumns")]
        public async Task<IActionResult> Update()
        {
            return Ok();
        }


        /// <summary>
        /// req {name: "newName", orderPlace: 1}
        /// res {"id": 1, "name": "newName", "orderPlace": 1, "notes": null, "boardId": 1}
        /// </summary>
        /// <param name="columnId"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [Route("Update/{columnId}")]
        public async Task<IActionResult> Update(int columnId)
        {
            string username = GetUsernameFromJwtToken();
            EditColumnData? columnData = await HttpContext.Request.ReadFromJsonAsync<EditColumnData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Column? column = _columnRepository.Get(columnId); 
            if (column == null) return BadRequest();

            Board? board = _boardRepository.Get(column.BoardId);
            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            column.Name = columnData.Name;
            column.OrderPlace = columnData.OrderPlace;

            _columnRepository.Update(column);

            return Ok();
        }

        public record NewColumnData (string Name, int OrderPlace, int BoardId);
        public record EditColumnData (string Name, int OrderPlace);
    }
}
