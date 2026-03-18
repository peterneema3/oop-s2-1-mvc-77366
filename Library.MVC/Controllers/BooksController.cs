using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
