using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyNotesApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MyNotesApplication.Controllers
{
    [Route("api/v1/Auth")]
    public class AuthController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ConfirmationToken> _confirmationTokenRepository;
        private readonly IConfiguration _appConfiguration;
        private readonly ILogger<AuthController> _logger;
        private readonly IDistributedCache _cache;

        public AuthController(IRepository<User> userRepo, IRepository<ConfirmationToken> confirmationTokenRepo, IConfiguration appConfiguration, ILogger<AuthController> logger, IDistributedCache disturbedCache)
        {
            _userRepository = userRepo;
            _confirmationTokenRepository = confirmationTokenRepo;
            _appConfiguration = appConfiguration;
            _logger = logger;
            _cache = disturbedCache;
        }

        /// <summary>
        /// Req: {"Email": "123", "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            try
            {
                AuthData? authData = await HttpContext.Request.ReadFromJsonAsync<AuthData>();

                string email = authData.Email;
                string password = authData.Password;

                User? user = null;
                var userCachedStr = "";
                bool isCacheServerOnline = true;

                try
                {
                    userCachedStr = await _cache.GetStringAsync(email);
                }
                catch (RedisConnectionException redisEx)
                {
                    _logger.LogWarning(redisEx.ToString());
                    isCacheServerOnline = false;
                }

                if (userCachedStr != "") user = JsonSerializer.Deserialize<User>(userCachedStr);
                else
                {
                    user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email);

                    if (user == null || !user.EmailConfirmed) return NotFound();

                    if (isCacheServerOnline)
                    {
                        var userJSONstr = JsonSerializer.Serialize(user);
                        _logger.LogDebug("recived data from cahce");
                        await _cache.SetStringAsync(email, userJSONstr, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                        });
                    }
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
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

        /// <summary>
        /// Req: {"Email": "123", "Username": "Mol" "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Register()
        {
            try
            {
                RegisterData? registerData = await HttpContext.Request.ReadFromJsonAsync<RegisterData>();

                string email = registerData.Email;
                string password = registerData.Password;
                string username = registerData.Username;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email || u.Username == username);

                if (user != null) return StatusCode(409);

                User newUser = new User();
                newUser.Email = email;
                newUser.Password = password;
                newUser.Username = username;
                newUser.EmailConfirmed = false;

                ConfirmationToken newToken = new ConfirmationToken();
                newToken.User = newUser;
                newToken.CreatedDate = DateTime.UtcNow;
                newToken.ExpiredDate = DateTime.UtcNow.AddDays(1);
                newToken.ConfirmationGUID = Guid.NewGuid().ToString();

                PasswordHasher<User> ph = new PasswordHasher<User>();
                newUser.Password = ph.HashPassword(newUser, password);

                User createdUser = _userRepository.Add(newUser);
                await _userRepository.SaveChanges();
                ConfirmationToken createdToken = _confirmationTokenRepository.Add(newToken);
                await _confirmationTokenRepository.SaveChanges();

                try
                {
                    var tokenJSONStr = JsonSerializer.Serialize(newToken);
                    await _cache.SetStringAsync(newToken.ConfirmationGUID, tokenJSONStr, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                    });
                }
                catch (RedisConnectionException redisEx)
                {
                    _logger.LogWarning(redisEx.Message);
                }

                var emailService = new EmailService(_appConfiguration);
                var confirmationUrl = Url.Action("EmailConfirm", "Auth", new { confirmationGuidUrl = createdToken.ConfirmationGUID }, protocol: HttpContext.Request.Scheme);
                await emailService.SendEmailAsync(newUser.Email, "Подтвердите свою почту", $"Подтвердите регистрацию, перейдя по ссылке: <a href='{confirmationUrl}'>Подтвердить</a>");

                return Ok(new { message = "confirm email" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("EmailConfirm/{confirmationGuidUrl}")]
        public async Task<IActionResult> EmailConfirm(string confirmationGuidUrl)
        {
            ConfirmationToken? token = null;
            var tokenCachedStr = "";
            try
            {
                tokenCachedStr = await _cache.GetStringAsync(confirmationGuidUrl);
            }
            catch (RedisConnectionException redisEx)
            {
                _logger.LogWarning(redisEx.Message);
            }

            if(tokenCachedStr != "") token = JsonSerializer.Deserialize<ConfirmationToken>(tokenCachedStr);
            else token = _confirmationTokenRepository.GetAll().FirstOrDefault(token => token.ConfirmationGUID == confirmationGuidUrl && token.ExpiredDate > DateTime.Now);

            if (token == null) return BadRequest();

            User user = _userRepository.Get(token.UserId);
            user.EmailConfirmed = true;

            _confirmationTokenRepository.Delete(token);
            await _confirmationTokenRepository.SaveChanges();
            _userRepository.Update(user);
            await _userRepository.SaveChanges();

            return Redirect(_appConfiguration.GetValue<string>("FrontRedirectUrl"));
        }

        public record AuthData(string Email, string Password);
        public record RegisterData(string Email, string Username, string Password);
    }
}
