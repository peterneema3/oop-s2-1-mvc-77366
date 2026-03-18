using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Books"); // redirect to Books list
        }
    }
}
