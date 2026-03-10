using Microsoft.AspNetCore.Mvc;

namespace Library.MVC.Controllers
{
    public class InvoicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
