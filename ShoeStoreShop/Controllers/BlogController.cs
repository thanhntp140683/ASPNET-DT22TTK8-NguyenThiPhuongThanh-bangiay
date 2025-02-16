using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
