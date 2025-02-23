using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Data;
using ShoeStore.Extensions;
using ShoeStore.Models.Vnpay;
using ShoeStore.Models;
using ShoeStore.Services.Momo;
using ShoeStore.Services.Vnpay;

namespace ShoeStore.Controllers
{
    
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _dbContext;
        private IMomoService _momoService;

        public PaymentController(IMomoService momoService, IVnPayService vnPayService, ApplicationDbContext dbContext)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderInfo model, PaymentInformationModel model1, string customerName, string address, string phone, decimal Amount, string paymentMethod)
        {
            HttpContext.Session.SetString("CustomerName", customerName);
            HttpContext.Session.SetString("Address", address);
            HttpContext.Session.SetString("Phone", phone);
            HttpContext.Session.SetDecimal("TotalAmount", Amount);

            switch (paymentMethod)
            {
                case "Momo":
                    var response = await _momoService.CreatePaymentAsync(model);
                    return Redirect(response.PayUrl);

                case "VnPay":
                    var url = _vnPayService.CreatePaymentUrl(model1, HttpContext);
                    return Redirect(url);

                case "COD":
                    var order = new Order
                    {
                        OrderId = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        CustomerName = customerName,
                        Address = address,
                        Phone = phone,
                        TotalAmount = Amount,
                        PaymentMethod = "COD",
                        PaymentStatus = "Success",
                        OrderStatus = "Processing",
                        OrderDate = DateTime.Now
                    };

                    _dbContext.Orders.Add(order);
                    await _dbContext.SaveChangesAsync();

                    var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");


                    if (cart == null || !cart.Any())
                    {
                        throw new ArgumentException("Cart không hợp lệ!");
                    }

                    var orderDetails = cart.Select(item => new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Id,
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Quantity * item.Price
                    }).ToList();


                    HttpContext.Session.Remove("cart");

                    ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Apple Store.";
                    TempData["PaymentSuccess"] = "Thanh toán thành công!";
                    return RedirectToAction("Index", "Home");

                default:
                    TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToAction("Index", "Cart");
            }

        }

        private List<OrderDetail> SaveOrderDetails(List<CartItem> cart, string orderId)
        {
            if (cart == null || !cart.Any())
            {
                throw new ArgumentException("Cart không hợp lệ!");
            }

            var orderDetails = cart.Select(item => new OrderDetail
            {
                OrderId = orderId,
                ProductId = item.Id,
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = item.Quantity * item.Price
            }).ToList();


            _dbContext.OrderDetails.AddRange(orderDetails);
            _dbContext.SaveChanges();

            HttpContext.Session.Remove("cart");
            TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            return orderDetails;
        }
        [Route("Payment/PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");
            if (response.VnPayResponseCode == "00")
            {

                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = response.OrderId,
                    PaymentStatus = "Success",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store.";
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = response.OrderId,
                    PaymentStatus = "Fail",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
            }

            return View("PaymentCallbackVnpay", response);

        }

        [HttpGet]
        [Route("Payment/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");

            if (response.IsSuccess)
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Success",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };

                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store.";
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Fail",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };
                _dbContext.Orders.Add(checkOrder);
                await _dbContext.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
                TempData["Error"] = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.!";
            }
            return View("PaymentCallback", response);
        }
    }
}
