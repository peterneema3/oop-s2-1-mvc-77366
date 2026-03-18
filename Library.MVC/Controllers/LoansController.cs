using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
