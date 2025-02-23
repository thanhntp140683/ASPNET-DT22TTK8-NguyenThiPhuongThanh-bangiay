using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;

namespace ShoeStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Product/{id}")]
        public IActionResult Index(int id)
        {
            var product = _context.Shoes
          .Include(s => s.Brand)
          .Include(s => s.Category)
          .Include(s => s.ShoeImages) 
          .FirstOrDefault(s => s.Id == id);

            if (product == null) return NotFound();

            ViewBag.Image = product.ShoeImages?.ToList();
            ViewBag.Brand = _context.Brands.ToList();
            ViewBag.Category = _context.Categories.ToList();

            return View(product);
        }
    }
}
