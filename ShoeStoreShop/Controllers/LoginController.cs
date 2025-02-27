using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using BCrypt.Net;
using ShoeStore.Models;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        public LoginController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        [HttpGet]
        [Route("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction( "Index", "Login");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
            
        {
            Console.WriteLine("Email: " + email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại!";
                return RedirectToAction("ForgotPassword");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            Console.WriteLine("OTP: " + otp);
            user.ResetPasswordToken = otp;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(email, "Mã xác nhận", $"Mã OTP của bạn: {otp}");

            TempData["Success"] = "OTP đã được gửi!";
            return RedirectToAction("VerifyOTP", new { email });
        }
        [HttpGet("VerifyOTP")]
        public IActionResult VerifyOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View();
        }
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP(string email, string otp)
        {
            Console.WriteLine("Email: " + email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại!";
                return RedirectToAction("ForgotPassword");
            }

            if (user.ResetPasswordToken != otp || user.ResetTokenExpires < DateTime.UtcNow)
            {
                TempData["Error"] = "Mã OTP không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("VerifyOTP", new { email });
            }

            TempData["Success"] = "OTP hợp lệ, vui lòng nhập mật khẩu mới.";
            return RedirectToAction("ResetPassword", new { email, otp });
        }
        [HttpGet]
        public IActionResult ResetPassword(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            ViewBag.OTP = otp;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại!";
                return RedirectToAction("ForgotPassword");
            }

            if (user.ResetPasswordToken != otp || user.ResetTokenExpires < DateTime.UtcNow)
            {
                TempData["Error"] = "Mã OTP không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("VerifyOTP", new { email });
            }

            // Mã hóa mật khẩu mới
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Mật khẩu đã được cập nhật!";
            return RedirectToAction("Login");
        }

    }
}
