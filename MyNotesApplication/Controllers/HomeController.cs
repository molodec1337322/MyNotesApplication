using Microsoft.AspNetCore.Mvc;

namespace MyNotesApplication.Controllers
{
    
    public class HomeController : Controller
    {
        [Route("")]
        [Route("/Home")]
        [Route("/Home/Index")]
        public string Index()
        {
            return "<a></a>";
        }
    }
}
