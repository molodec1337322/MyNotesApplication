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

        [HttpPost]
        [Route("Login")]
        public async void Login()
        {
            try
            {
                AuthData? authData = await HttpContext.Request.ReadFromJsonAsync<AuthData>();

                string email = authData.Email;
                string password = authData.Paasword;

                User? user = _userRepository.GetAll().FirstOrDefault(u => u.Email == email && u.Password == password);

                if(user != null)
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
        public ActionResult Logout() 
        {
            return View();
        }

        [HttpPost]
        [Route("Register")]
        public ActionResult Register()
        {
            return View();
        }

        public record AuthData(string Email, string Paasword);
    }
}
