using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyNotesApplication.Data.Interfaces;
using MyNotesApplication.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyNotesApplication.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public AuthController(IRepository<User> userRepo)
        {
            _userRepository = userRepo;
        }

        /// <summary>
        /// Req: {"Email": "123", "Password": "112"}
        /// Res: {Bearer: jwtToken}
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async void Login()
        {
            try
            {
                AuthData? authData = await HttpContext.Request.ReadFromJsonAsync<AuthData>();

                string email = authData.Email;
                string password = authData.Password;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email && u.Password == password);

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
        public void Logout() 
        {

        }

        [HttpPost]
        [Route("Register")]
        public async void Register()
        {
            try
            {
                RegisterData? registerData = await HttpContext.Request.ReadFromJsonAsync<RegisterData>();

                string email = registerData.Email;
                string password = registerData.Password;
                string username = registerData.Username;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    User newUser = new User();
                    newUser.Email = email;
                    newUser.Password = password;
                    newUser.Username = username;
                    newUser.EmailConfirmed = false;

                    PasswordHasher<User> ph = new PasswordHasher<User>();
                    newUser.Password = ph.HashPassword(newUser, password);

                    User createdUser = _userRepository.Add(newUser);
                    await _userRepository.SaveChanges();

                    await HttpContext.Response.WriteAsJsonAsync(createdUser);
                }
                else
                {
                    await HttpContext.Response.WriteAsJsonAsync(new { message = "userAlreadyExists" });
                }

            }
            catch (Exception ex)
            {
                await HttpContext.Response.WriteAsJsonAsync(new { message = "error", exception = ex.Message });
            }
        }

        public record AuthData(string Email, string Password);
        public record RegisterData(string Email, string Username, string Password);
    }
}
