using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
