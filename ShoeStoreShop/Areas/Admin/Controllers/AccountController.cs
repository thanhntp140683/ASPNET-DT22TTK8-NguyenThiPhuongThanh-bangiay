using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Account")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var users = await _context.Users
            .Select(x => new UserViewModel
            {
                Id = x.Id,
                UserName = x.Username,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber ?? "Trống",
                Role = x.Role,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

            return View(users);
        }
        [HttpGet("create")]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(User user)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            _context.Add(user);
            await _context.SaveChangesAsync();
            if (user.FullName == null || user.Email == null || user.Password == null)
            {
                TempData["error"] = "Vui lòng nhập đầy đủ thông tin";
                return View(user);
            }
            TempData["success"] = "Thêm tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var user = _context.Users.Find(id);
            return View(user);
        }
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, User user)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                var existingAccount = await _context.Users.FindAsync(id);
                if (existingAccount == null)
                {
                    return NotFound();
                }

                existingAccount.FullName = user.FullName;
                existingAccount.PhoneNumber = user.PhoneNumber;
                existingAccount.Status = user.Status;
                existingAccount.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingAccount.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["success"] = "Cập nhật tài khoản thành công";
            return RedirectToAction("Index");
        }
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa tài khoản thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}
