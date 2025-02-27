using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("Admin/Login")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("Admin/Login")]
        public IActionResult LoginAdmin(string username, string password)
        {
            var user = _context.Users
                .Where(a => a.Username == username)
                .Select(a => new
                {
                    a.Id,
                    a.Username,
                    a.Email,
                    a.Password,
                    a.FullName,
                    a.Role,
                    a.Status
                })
                .SingleOrDefault();
            if (user == null)
            {
                TempData["Error"] = "Vui lòng nhập tài khoản và mật khẩu!";
                return View("Index");
            }
            HttpContext.Session.SetString("Name", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return View("Index");
        }
        [HttpGet("Admin/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Name");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Index");
        }
    }
}
