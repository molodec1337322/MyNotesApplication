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
        [Route("Login/{username}")]
        public string Login(string username)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt).ToString();
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
    }
}
