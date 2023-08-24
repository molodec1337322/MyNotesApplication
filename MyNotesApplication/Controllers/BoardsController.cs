using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using MyNotesApplication.Services;
using MyNotesApplication.Services.Interfaces;
using MyNotesApplication.Services.Message;
using MyNotesApplication.Services.RabbitMQBroker.Messages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using static MyNotesApplication.Controllers.NotesController;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Boards")]
    public class BoardsController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserBoardRole> _userBoardRoleRepository;
        private readonly IRepository<Board> _boardRepository;
        private readonly IRepository<InvitationToken> _invitationTokenRepository;
        private readonly IConfiguration _appConfiguration;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<BoardsController> _logger;

        public BoardsController(
            IRepository<User> userRepository, 
            IRepository<UserBoardRole> userBoardRoleRepository, 
            IRepository<Board> boardRepository, 
            IRepository<Column> columnRepository, 
            IRepository<InvitationToken> invitationTokenRepository,
            IConfiguration configuration,
            IMessageBroker messageBroker,
            ILogger<BoardsController> logger) 
        { 
            _userRepository = userRepository;
            _userBoardRoleRepository = userBoardRoleRepository;
            _boardRepository = boardRepository;
            _invitationTokenRepository = invitationTokenRepository;
            _appConfiguration = configuration;
            _messageBroker = messageBroker;
            _logger = logger;
        }

        private string GetUsernameFromJwtToken()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            token = token.ToString().Split(" ")[1];
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
        }

        private void SendEmail(string email, string subject, string body)
        {
            var payloadMessage = new SendEmailMessage(email, subject, body);
            var JSONPayloadMessage = JsonSerializer.Serialize(payloadMessage).ToString();
            var message = new MessageWithJSONPayload(_appConfiguration.GetValue<string>("BrokerEmailServiceName"), JSONPayloadMessage);
            _messageBroker.SendMessage(message);
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
        /// req {}
        /// res [{"id": 1, "name": "gfdsggf"}, {"id": 2, "name": "gffdgdfgf"}]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("AllOwned")]
        public async Task<IActionResult> GetOwnedBoards()
        {
            string username = GetUsernameFromJwtToken();
            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            List<UserBoardRole> ubrList = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.Role == UserBoardRoles.OWNER.ToString()).ToList();
            List<Board> boardList = new List<Board>();//_boardRepository.GetAll().Where(b => ).ToList();
            foreach (var ubr in ubrList)
            {
                boardList.Add(_boardRepository.Get(ubr.BoardId));
            }

            return Ok(boardList);
        }

        /// <summary>
        /// req {}
        /// res [{"id": 1, "name": "gfdsggf"}, {"id": 2, "name": "gffdgdfgf"}]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("AllGuest")]
        public async Task<IActionResult> GetAllGuestBoards()
        {
            string username = GetUsernameFromJwtToken();
            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            List<UserBoardRole> ubrList = _userBoardRoleRepository.Get(u => u.UserId == user.Id && u.Role == UserBoardRoles.GUEST.ToString()).ToList();
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
            if (board is null) return NotFound();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();
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

            var boardData = HttpContext.Request.ReadFromJsonAsync<NewBoardData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Board newBoard = new Board();

            NewBoardData? newBoardData = await boardData;
            if (newBoardData is null) return BadRequest();

            newBoard.Name = newBoardData.Name;

            Board board = _boardRepository.Add(newBoard);

            UserBoardRole newUBR = new UserBoardRole();
            newUBR.Board = board;
            newUBR.UserId = user.Id;
            newUBR.Role = UserBoardRoles.OWNER.ToString();

            UserBoardRole ubr = _userBoardRoleRepository.Add(newUBR);

            return Ok(board);
        }

        /// <summary>
        /// req {"Email": "comcom@com.com"}
        /// res {}
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("AddUserAsGuest/{BoardId}")]
        public async Task<IActionResult> AddGuest(int boardId)
        {
            string username = GetUsernameFromJwtToken();

            var inviteData = HttpContext.Request.ReadFromJsonAsync<UserToInviteData>();

            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            UserToInviteData? userToInviteData = await inviteData;
            if (userToInviteData is null) return BadRequest();
            if (user.Email == userToInviteData.Email) return Conflict();

            User? userToInvite = _userRepository.Get(u => u.Email == userToInviteData.Email).FirstOrDefault();
            if(userToInvite is null) return BadRequest();

            Board? board = _boardRepository.Get(boardId);

            if (board is null) return BadRequest();
            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            InvitationToken createdToken = new InvitationToken();
            createdToken.UserId = userToInvite.Id;
            createdToken.User = userToInvite;
            createdToken.BoardId = boardId;
            createdToken.Board = board;
            createdToken.CreatedDate = DateTime.UtcNow;
            createdToken.ExpirationTime = DateTime.UtcNow.AddHours(72);
            createdToken.InvitationGUID = Guid.NewGuid().ToString();

            _invitationTokenRepository.Update(createdToken);

            var invitationUrl = Url.Action("ConfirmInvitation", "Boards", new { invitationGUID = createdToken.InvitationGUID }, protocol: HttpContext.Request.Scheme);
            SendEmail(userToInvite.Email, "Приглашение на доску в качестве участника", $"Вы были приглашашены на доску в качестве участника пользователем {user.Username}. Перейдите по ссылке, чтобы стать участником: <a href='{invitationUrl}'>Подтвердить</a>");

            return Ok();
        }

        [HttpGet]
        [Route("ConfirmInvitation/{invitationGUID}")]
        public async Task<IActionResult> ConfirmInvitation(string invitationGUID)
        {
            InvitationToken? token = _invitationTokenRepository.Get(i => i.InvitationGUID == invitationGUID).FirstOrDefault();

            if (token is null) return BadRequest("no such token found");
            if (token.ExpirationTime < DateTime.UtcNow) return BadRequest("Token expired, request it again in registration form");

            UserBoardRole newUbr = new UserBoardRole();
            newUbr.BoardId = token.BoardId;
            newUbr.UserId = token.UserId;
            newUbr.User = token.User;
            newUbr.Board = token.Board;
            newUbr.Role = UserBoardRoles.GUEST.ToString();

            _userBoardRoleRepository.Add(newUbr);

            List<InvitationToken> tokensToDelete = _invitationTokenRepository.Get(t => t.UserId == token.UserId).ToList();
            foreach(var tokenToDelete in tokensToDelete)
            {
                _invitationTokenRepository.Delete(tokenToDelete);
            }

            return Redirect(_appConfiguration.GetValue<string>("FrontRedirectUrl"));
        }

        /// <summary>
        /// req URL
        /// res [{Name: "fsdgsd", ...}]
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("Owners/{boardId}")]
        public async Task<IActionResult> GetOwnersOfBoard(int boardId)
        {
            List<UserBoardRole> ubrlist = _userBoardRoleRepository.Get(u => u.BoardId == boardId && u.Role == UserBoardRoles.OWNER.ToString()).ToList();
            List<User> users = new List<User>();
            User? user = null;

            foreach(var ubr in ubrlist)
            {
                user = _userRepository.Get(u => u.Id == ubr.UserId).FirstOrDefault();
                users.Add(user);
            }

            return Ok(users);
        }

        /// <summary>
        /// req URL
        /// res [{Name: "fsdgsd", ...}]
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("Guests/{boardId}")]
        public async Task<IActionResult> GetGuestsOfBoard(int boardId)
        {
            List<UserBoardRole> ubrlist = _userBoardRoleRepository.Get(u => u.BoardId == boardId && u.Role == UserBoardRoles.GUEST.ToString()).ToList();

            List<User> users = new List<User>();
            User? user = null;

            foreach (var ubr in ubrlist)
            {
                user = _userRepository.Get(u => u.Id == ubr.UserId).FirstOrDefault();
                users.Add(user);
            }

            return Ok(users);
        }

        [HttpPut]
        [Authorize]
        [Route("ChangeRole/{BoardId}")]
        public async Task<IActionResult> AddOwner()
        {
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("Delete/{BoardId}")]
        public async Task<IActionResult> DeleteBoard(int BoardId)
        {
            var username = GetUsernameFromJwtToken();
            User? user = _userRepository.Get(u => u.Username == username).FirstOrDefault();

            Board? board = _boardRepository.Get(BoardId);
            if (board is null) return BadRequest();

            if (!IsUserAllowedToInteractWithBoard(user, board, UserBoardRoles.OWNER)) return Forbid();

            _boardRepository.Delete(board);

            return Ok(new { message = "deleted", boardId = BoardId });
        }

        public record BoardData(int id, string Name);
        public record NewBoardData(string Name);
        public record UserToInviteData(string Email);
        public record UserChangeRoleData(int userId, string role);

    }
}
