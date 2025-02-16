using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
