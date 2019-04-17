namespace Devanooga.Slackbot.Website.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    
    [Route("")]
    public class HomeController : Controller
    {
        public Task<IActionResult> Index()
        {
            return Task.FromResult<IActionResult>(View());
        }
    }
}