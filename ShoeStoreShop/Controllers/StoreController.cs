using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;

namespace ShoeStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StoreController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? brand, int? category, int? color, string keyword, int page = 1, int pageSize = 6)
        {
            ViewBag.Categories = _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    ProductCount = _context.Shoes.Count(p => p.Category_Id == c.Id)
                })
                .ToList();

            ViewBag.Brands = _context.Brands
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    ProductCount = _context.Shoes.Count(p => p.Brand_Id == b.Id)
                })
                .ToList();

            ViewBag.Colors = _context.Colors
                .Select(cl => new
                {
                    cl.Id,
                    cl.Name,
                    ProductCount = _context.Shoes.Count(p => p.ColorId == cl.Id)
                })
                .ToList();


            // Truy vấn sản phẩm
            var filler = _context.Shoes
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Color)
                .Include(p => p.ShoeImages)
                .AsQueryable();

            if (category.HasValue)
            {
                filler = filler.Where(p => p.Category_Id == category.Value);
            }

            if (brand.HasValue)
            {
                filler = filler.Where(p => p.Brand_Id == brand.Value);
            }

            if (color.HasValue)
            {
                filler = filler.Where(p => p.ColorId == color.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                filler = filler.Where(p => p.Name.Contains(keyword));
            }

            // Tổng số sản phẩm sau khi lọc
            int totalItems = filler.Count();

            // Phân trang
            var products = filler.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Truyền thông tin phân trang và bộ lọc đã chọn sang View
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedColor = color;
            ViewBag.Keyword = keyword;

            return View(products);
        }


    }
}
