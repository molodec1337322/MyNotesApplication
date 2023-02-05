using Microsoft.AspNetCore.Mvc;

namespace MyNotesApplication.Controllers
{
    
    public class HomeController : Controller
    {
        [Route("")]
        [Route("/Home")]
        [Route("/Home/Index")]
        public IActionResult Index()
        {
            return Ok("Started");
        }
    }
}
