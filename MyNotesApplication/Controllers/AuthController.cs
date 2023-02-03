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

namespace MyNotesApplication.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ConfirmationToken> _confirmationTokenRepository;

        public AuthController(IRepository<User> userRepo, IRepository<ConfirmationToken> confirmationTokenRepo)
        {
            _userRepository = userRepo;
            _confirmationTokenRepository = confirmationTokenRepo;

        }

        /// <summary>
        /// Req: {"Email": "123", "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async Task Login()
        {
            try
            {
                AuthData? authData = await HttpContext.Request.ReadFromJsonAsync<AuthData>();

                string email = authData.Email;
                string password = authData.Password;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    PasswordHasher<User> ph = new PasswordHasher<User>();
                    if(ph.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };
                        var jwt = new JwtSecurityToken(
                                issuer: AuthOptions.ISSUER,
                                audience: AuthOptions.AUDIENCE,
                                claims: claims,
                                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(12)),
                                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));


                        await HttpContext.Response.WriteAsJsonAsync(new { Bearer = ": " + new JwtSecurityTokenHandler().WriteToken(jwt) });
                    }
                    else
                    {
                        await HttpContext.Response.WriteAsJsonAsync(new { message = "userNotFound" });
                    }
                     
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "userNotFound"});
                }
                
            }
            catch (Exception ex)
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message="error", exception=ex.Message});
            }
        }

        [HttpPost]
        [Route("Logout")]
        public async Task Logout() 
        {

        }

        /// <summary>
        /// Req: {"Email": "123", "Username": "Mol" "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Registration")]
        public async Task Register()
        {
            try
            {
                RegisterData? registerData = await HttpContext.Request.ReadFromJsonAsync<RegisterData>();

                string email = registerData.Email;
                string password = registerData.Password;
                string username = registerData.Username;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email || u.Username == username);

                if (user == null)
                {
                    User newUser = new User();
                    newUser.Email = email;
                    newUser.Password = password;
                    newUser.Username = username;
                    newUser.EmailConfirmed = false;

                    ConfirmationToken newToken= new ConfirmationToken();
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

                    var emailService = new EmailService();
                    await emailService.SendEmailAsync(newUser.Email, "Подтвердите свою почту", "Подтвердите регистрацию, перейдя по ссылке: <a href='EmailConfirm/" + createdToken.ConfirmationGUID + "' >Подтвердить</a>");

                    await HttpContext.Response.WriteAsJsonAsync(new { message = "userConfirmEmail" });
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "userAlreadyExists" });
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                await HttpContext.Response.WriteAsJsonAsync(new { message = "error", exception = ex.Message });
            }
        }

        [HttpGet]
        [Route("EmailConfirm/{confirmationGuidUrl}")]
        public async Task EmailConfirm(string confirmationGuidUrl)
        {
            ConfirmationToken? token = _confirmationTokenRepository.GetAll().FirstOrDefault(token => token.ConfirmationGUID == confirmationGuidUrl && token.ExpiredDate > DateTime.Now);
            if(token != null)
            {
                User user = _userRepository.Get(token.UserId);
                user.EmailConfirmed = true;

                _confirmationTokenRepository.Delete(token);
                await _confirmationTokenRepository.SaveChanges();
                _userRepository.Update(user);
                await _userRepository.SaveChanges();

                await HttpContext.Response.WriteAsync("confirmed");
            }
            else
            {
                await HttpContext.Response.WriteAsync("confirmError");
            }
        }

        public record AuthData(string Email, string Password);
        public record RegisterData(string Email, string Username, string Password);
    }
}
