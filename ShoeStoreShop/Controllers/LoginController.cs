using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using BCrypt.Net;
using ShoeStore.Models;

namespace ShoeStore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("/Register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost("/Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
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
                                        .SingleOrDefaultAsync();
            if (user == null)
            {
                TempData["Error"] = "Vui lòng nhập tài khoản và mật khẩu!";
                return View("Index");
            }
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    TempData["Error"] = "Sai tài khoản hoặc mật khẩu!";
                    return View("Index");
                }

                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("Index", "Home");
                }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost("/Register")]
        public async Task<IActionResult> Register(string username, string fullname, string password, string email)
        {
            var existingUser = await _context.Users
                                              .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Tên tài khoản đã tồn tại.");
                return View();
            }

            var existingEmail = await _context.Users
                                               .FirstOrDefaultAsync(u => u.Email == email);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email đã được đăng ký.");
                return View();
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newAccount = new User
            {
                Username = username,
                FullName = fullname,
                Password = passwordHash,
                Email = email,
                CreatedAt = DateTime.Now,
                Status = "Active",
                Role = "User"
            };

            _context.Users.Add(newAccount);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            //if (user == null)
            //{
            //    return Json(new { success = false, message = "Email không tồn tại!" });
            //}

            //user.ResetPasswordToken = Guid.NewGuid().ToString();
            //user.ResetTokenExpires = DateTime.Now.AddHours(1);
            //await _context.SaveChangesAsync();

            //string resetLink = Url.Action("ResetPassword", "Account", new { token = user.ResetPasswordToken }, Request.Scheme);

            //await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", $"Click vào link để đặt lại mật khẩu: {resetLink}");

            return Json(new { success = true, message = "Hãy kiểm tra email của bạn để đặt lại mật khẩu." });
        }
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            //var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token && u.ResetTokenExpires > DateTime.Now);

            //if (user == null)
            //{
            //    return View("Error");
            //}

            return View(new ResetPasswordViewModel { Token = token });
        }
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    var user = await _context.Users
            //        .FirstOrDefaultAsync(u => u.ResetPasswordToken == model.Token);

            //    if (user != null)
            //    {
            //        user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            //        user.ResetPasswordToken = null;
            //        await _context.SaveChangesAsync();

            //        return RedirectToAction("Login", "Account");
            //    }
            //    else
            //    {
            //        ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
            //    }
            //}

            return View(model);
        }


        [HttpGet]
        [Route("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction( "Index", "Login");
        }
    }
}
