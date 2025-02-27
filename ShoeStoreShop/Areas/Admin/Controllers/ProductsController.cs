using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Models;

namespace ShoeStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Admin/Products")]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var shoes = _context.Shoes
                .Include(s => s.Category)
                .Include(s => s.Brand)
                .Include(s => s.Color)
                .ToList();
            return View(shoes);
        }
        [HttpGet("Admin/Products/Create")]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Category = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name");
            ViewBag.Colors = new SelectList(_context.Colors, "Id", "Name");
            return View();
        }
        [HttpGet("Admin/Products/Custom")]
        public IActionResult Custom()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var categories = _context.Categories.ToList();
            var brands = _context.Brands.ToList();
            var colors = _context.Colors.ToList();

            ViewData["Categories"] = categories;
            ViewData["Brands"] = brands;
            ViewData["Colors"] = colors;

            return View();
        }
        [HttpGet("Admin/Products/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var shoe = _context.Shoes
                    .Include(s => s.Category)
                    .Include(s => s.Brand)
                    .Include(s => s.Color)
                    .Include(s => s.ShoeImages) // Lấy danh sách hình ảnh
                    .FirstOrDefault(s => s.Id == id);

                if (shoe == null)
                {
                    return NotFound("Không tìm thấy sản phẩm!");
                }

                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", shoe.Category_Id);
                ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", shoe.Brand_Id);
                ViewBag.Colors = new SelectList(_context.Colors, "Id", "Name", shoe.ColorId);

                return View(shoe);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }
        [HttpPost("Admin/Products/Create")]
        public async Task<IActionResult> Create(Shoe shoe, List<IFormFile> images)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.Category = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name");
            ViewBag.Colors = new SelectList(_context.Colors, "Id", "Name");

            _context.Shoes.Add(shoe);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine("wwwroot/uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var shoeImage = new ShoeImage
                        {
                            Shoe_Id = shoe.Id,
                            Image_Url = "/uploads/" + fileName
                        };

                        _context.ShoeImages.Add(shoeImage);
                    }
                }
                await _context.SaveChangesAsync();
            }
            return View(shoe);
        }
        [HttpPost("Admin/Products/Edit/{id}")]
        public async Task<IActionResult> Edit(int id, Shoe shoe, List<IFormFile> images)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var existingShoe = _context.Shoes
                .Include(s => s.ShoeImages)
                .FirstOrDefault(s => s.Id == id);

            if (existingShoe == null)
                return NotFound();

            // Cập nhật thông tin sản phẩm
            existingShoe.Name = shoe.Name;
            existingShoe.Description = shoe.Description;
            existingShoe.Price = shoe.Price;
            existingShoe.Category_Id = shoe.Category_Id;
            existingShoe.Brand_Id = shoe.Brand_Id;
            existingShoe.ColorId = shoe.ColorId;
            existingShoe.Gender = shoe.Gender;
            existingShoe.Updated_At = DateTime.Now;

            // **Chỉ xóa ảnh cũ nếu có ảnh mới được tải lên**
            if (images != null && images.Count > 0)
            {
                foreach (var oldImage in existingShoe.ShoeImages)
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", oldImage.Image_Url);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _context.ShoeImages.RemoveRange(existingShoe.ShoeImages);

                // **Thêm ảnh mới**
                foreach (var image in images)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var newImage = new ShoeImage
                    {
                        Shoe_Id = id,
                        Image_Url = "/uploads/" + fileName,
                        Created_At = DateTime.Now
                    };

                    _context.ShoeImages.Add(newImage);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost("Admin/Products/CustomProduct")]
        public IActionResult CreateCustom(string select, string name)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên không được để trống!";
                return RedirectToAction("Index");
            }

            if (select == "category")
            {
                var category = new Category { Name = name };
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm danh mục mới!";
            }
            else if (select == "brand")
            {
                var brand = new Brand { Name = name };
                _context.Brands.Add(brand);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm thương hiệu mới!";
            }
            else if (select == "color")
            {
                var color = new Color { Name = name };
                _context.Colors.Add(color);
                _context.SaveChanges();
                TempData["Success"] = "Đã thêm màu sắc mới!";
            }
            else
            {
                TempData["Error"] = "Lựa chọn không hợp lệ!";
            }

            return RedirectToAction("Index");
        }


       
        [HttpPost("Admin/Products/CustomCategory")]
        [ActionName("EditCategory")]
        public IActionResult EditCategory(Category category, string Name)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            if (Name == null)
            {
                TempData["Error"] = "Không được để trống";
                return RedirectToAction("Index");
            }

            var existingCategory = _context.Categories.Find(category.Id);
            if (existingCategory != null)
            {
                existingCategory.Name = category.Name;
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục thành công!";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost("Admin/Products/CustomBrand")]
        [ActionName("EditBrand")]
        public IActionResult EditBrand(Brand brand, string Name)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            if (Name == null)
            {
                TempData["Error"] = "Không được để trống";
                return RedirectToAction("Index");
            }

            var existingBrand = _context.Brands.Find(brand.Id);

            if (existingBrand != null)
            {
                existingBrand.Name = brand.Name;
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật thương hiệu thành công!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost("Admin/Products/CustomColor")]
        [ActionName("EditColor")]
        public IActionResult EditColor(Color color, string Name)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            if (Name == null)
            {
                TempData["Error"] = "Không được để trống";
                return RedirectToAction("Index");
            }

            var existingColor = _context.Colors.Find(color.Id);

            if (existingColor != null)
            {
                existingColor.Name = color.Name;
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật màu sắc thành công!";
            }
            return RedirectToAction("Index");
        }
        [HttpGet("Admin/Products/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }
            var shoe = _context.Shoes.Find(id);
            if (shoe == null)
            {
                return NotFound();
            }
            _context.Shoes.Remove(shoe);
            _context.SaveChanges();

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("Index");
        }
    }
}
