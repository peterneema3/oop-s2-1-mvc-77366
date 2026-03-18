using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{
    public class MembersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
