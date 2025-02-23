using Microsoft.AspNetCore.Mvc;
using ShoeStore.Data;
using ShoeStore.Extensions;
using ShoeStore.Models;

namespace ShoeStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSesionKey = "cart";
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            return View(cart);
        }
        public IActionResult AddToCart(int id, string name, decimal price, string imageUrl, int qty)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            var item = cart.FirstOrDefault(c => c.Id == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    Id = id,
                    Name = name,
                    Price = price,
                    ImageUrl = imageUrl,
                    Quantity = qty
                });
            }
            else
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("cart", cart);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int Id, string action)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            var item = cart.FirstOrDefault(c => c.Id == Id);

            if (item != null)
            {
                if (action == "increase")
                {
                    item.Quantity++;
                }
                else if (action == "decrease" && item.Quantity > 1)
                {
                    item.Quantity--;
                }
            }

            HttpContext.Session.SetObjectAsJson("cart", cart);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            var itemToRemove = cart.FirstOrDefault(c => c.Id == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            HttpContext.Session.SetObjectAsJson("cart", cart);

            return RedirectToAction("Index");
        }
    }
}
