using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
