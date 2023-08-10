using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyNotesApplication.Services;
using Microsoft.AspNetCore.Authorization;
using MyNotesApplication.Services.Interfaces;
using MyNotesApplication.Services.RabbitMQBroker.Messages;
using MyNotesApplication.Services.Message;
using System.Text.Json;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Auth")]
    public class AuthController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ConfirmationToken> _confirmationTokenRepository;
        private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
        private readonly IConfiguration _appConfiguration;
        private readonly IMessageBroker _messageBroker;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IRepository<User> userRepo, IRepository<ConfirmationToken> confirmationTokenRepo, IRepository<PasswordResetToken> passwordResetTokenRepository, IConfiguration appConfiguration, IMessageBroker messageBroker, ILogger<AuthController> logger)
        {
            _userRepository = userRepo;
            _confirmationTokenRepository = confirmationTokenRepo;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _appConfiguration = appConfiguration;
            _messageBroker = messageBroker;
            _logger = logger;
        }

        private void SendEmail(string email, string subject, string body)
        {
            var payloadMessage = new SendEmailMessage(email, subject, body);
            var JSONPayloadMessage = JsonSerializer.Serialize(payloadMessage).ToString();
            var message = new MessageWithJSONPayload(_appConfiguration.GetValue<string>("BrokerEmailServiceName"), JSONPayloadMessage);
            _messageBroker.SendMessage(message);
        }

        /// <summary>
        /// Req: {"Email": "123", "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            AuthData? authData = await HttpContext.Request.ReadFromJsonAsync<AuthData>();

            string email = authData.Email;
            string password = authData.Password;

            User? user = _userRepository.Get(u => u.Email == email).FirstOrDefault();
            if (user == null) return NotFound();
            if (!user.EmailConfirmed) return Unauthorized(new { message = "account not activated", email = user.Email });

            PasswordHasher<User> ph = new PasswordHasher<User>();
            if (ph.VerifyHashedPassword(user, user.Password, password) != PasswordVerificationResult.Success) return NotFound();


            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromHours(12)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return Ok(new { Token = "Bearer " + new JwtSecurityTokenHandler().WriteToken(jwt) });
        }

        [HttpPost]
        [Authorize]
        [Route("Logout")]
        public async Task<IActionResult> Logout() 
        {
            try
            {
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        private ConfirmationToken GenerateNewRegistrationToken(User user)
        {
            ConfirmationToken token = new ConfirmationToken();
            token.User = user;
            token.CreatedDate = DateTime.UtcNow;
            token.ExpiredDate = DateTime.UtcNow.AddHours(2);
            token.ConfirmationGUID = Guid.NewGuid().ToString();

            return token;
        }

        /// <summary>
        /// Req: {"Email": "123", "Username": "Mol" "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Register()
        {
            RegisterData? registerData = await HttpContext.Request.ReadFromJsonAsync<RegisterData>();

            string email = registerData.Email;
            string password = registerData.Password;
            string username = registerData.Username;

            User? user = _userRepository.Get(u => u.Email == email || u.Username == username).FirstOrDefault();

            if (user != null) return Conflict();

            User newUser = new User();
            newUser.Email = email;
            newUser.Password = password;
            newUser.Username = username;
            newUser.EmailConfirmed = false;

            PasswordHasher<User> ph = new PasswordHasher<User>();
            newUser.Password = ph.HashPassword(newUser, password);

            User createdUser = _userRepository.Add(newUser);

            ConfirmationToken createdToken = _confirmationTokenRepository.Add(GenerateNewRegistrationToken(newUser));

            var confirmationUrl = Url.Action("EmailConfirm", "Auth", new { confirmationGuidUrl = createdToken.ConfirmationGUID }, protocol: HttpContext.Request.Scheme);
            SendEmail(newUser.Email, "Подтвердите свою почту", $"Подтвердите регистрацию, перейдя по ссылке: <a href='{confirmationUrl}'>Подтвердить</a>");

            return Ok(new { message = "confirm email" });
        }

        /// <summary>
        /// req{Email: "email@email.com"}
        /// res{}
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("NewActivationMail")]
        public async Task<IActionResult> NewActivationMail()
        {
            EmailData? emailData = await HttpContext.Request.ReadFromJsonAsync<EmailData>();

            string email = emailData.Email;

            User? user = _userRepository.Get(u => u.Email == email).FirstOrDefault();

            if (user == null) return BadRequest(new {message = "no user with such email"});
            if (user.EmailConfirmed) return BadRequest(new { message = "already activated" });

            ConfirmationToken? oldToken = _confirmationTokenRepository.Get(t => t.UserId == user.Id).FirstOrDefault();
            if(oldToken != null)
            {
                _confirmationTokenRepository.Delete(oldToken);
            }

            ConfirmationToken newToken = _confirmationTokenRepository.Add(GenerateNewRegistrationToken(user));

            var confirmationUrl = Url.Action("EmailConfirm", "Auth", new { confirmationGuidUrl = newToken.ConfirmationGUID }, protocol: HttpContext.Request.Scheme);
            SendEmail(user.Email, "Подтвердите свою почту", $"Подтвердите регистрацию, перейдя по ссылке: <a href='{confirmationUrl}'>Подтвердить</a>");

            return Ok();
        }

        [HttpGet]
        [Route("EmailConfirm/{confirmationGuidUrl}")]
        public async Task<IActionResult> EmailConfirm(string confirmationGuidUrl)
        {
            ConfirmationToken? token = _confirmationTokenRepository.Get(t => t.ConfirmationGUID == confirmationGuidUrl).FirstOrDefault();

            if (token == null) return BadRequest("no such token found");
            if (token.ExpiredDate < DateTime.UtcNow) return BadRequest("Token expired, request it again in registration form");

            User user = _userRepository.Get(token.UserId);
            user.EmailConfirmed = true;

            _confirmationTokenRepository.Delete(token);
            _userRepository.Update(user);

            return Redirect(_appConfiguration.GetValue<string>("FrontRedirectUrl"));
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword()
        {
            EmailData? emailData = await HttpContext.Request.ReadFromJsonAsync<EmailData>();

            User? user = _userRepository.Get(u => u.Email == emailData.Email).FirstOrDefault();
            if (user == null) return BadRequest("No such user");

            PasswordResetToken? resetToken = _passwordResetTokenRepository.Get(t => t.UserId == user.Id).FirstOrDefault();
            if (resetToken != null) _passwordResetTokenRepository.Delete(resetToken);

            resetToken = new PasswordResetToken();
            resetToken.UserId = user.Id;
            resetToken.CreatedDate = DateTime.UtcNow;
            resetToken.ExpiredDate = DateTime.UtcNow.AddHours(2);
            resetToken.ConfirmationGUID = Guid.NewGuid().ToString();

            PasswordResetToken createdToken = _passwordResetTokenRepository.Add(resetToken);

            var resetUrl = Url.Action("NewPassword", "Auth", new { resetToken = createdToken.ConfirmationGUID }, protocol: HttpContext.Request.Scheme);
            SendEmail(user.Email, "Смена пароля", $"Для смены пароля перейдите по ссылке: <a href='{resetUrl}'>Подтвердить</a>. <br/>Если вы не запрашивали смену пароля, то просто игнорируйте это письмо.");

            return Ok();
        }

        [HttpGet]
        [Route("NewPassword/{resetToken}")]
        public async Task<IActionResult> NewPassword(string resetToken)
        {
            var redirectUrl = $"{_appConfiguration.GetValue<string>("FrontRedirectUrl")}/PasswordReset/{resetToken}";
            return Redirect(redirectUrl);
        }

        [HttpPut]
        [Route("NewPasswordConfirm/{resetToken}")]
        public async Task<IActionResult> NewPasswordConfirm(string resetToken)
        {
            PasswordResetToken? token = _passwordResetTokenRepository.Get(t => t.ConfirmationGUID == resetToken).FirstOrDefault();

            NewPasswordData? passwordData = await HttpContext.Request.ReadFromJsonAsync<NewPasswordData>();

            if (token == null) return BadRequest("no such token found");
            if (token.ExpiredDate < DateTime.UtcNow) return BadRequest("Token expired, request it again in registration form");

            User user = _userRepository.Get(token.UserId);
            user.Password = passwordData.password;

            PasswordHasher<User> ph = new PasswordHasher<User>();
            user.Password = ph.HashPassword(user, passwordData.password);

            _passwordResetTokenRepository.Delete(token);
            _userRepository.Update(user);

            return Ok();
        }


        public record AuthData(string Email, string Password);
        public record EmailData(string Email);
        public record RegisterData(string Email, string Username, string Password);
        public record NewPasswordData(string password);
    }
}
