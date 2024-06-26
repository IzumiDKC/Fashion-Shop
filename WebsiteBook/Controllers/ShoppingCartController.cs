using FashionShopDemo.Extensions;
using FashionShopDemo.Models;
using FashionShopDemo.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FashionShopDemo.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MoMoPaymentService _moMoPaymentService;
        private readonly ILogger<ShoppingCartController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PayOSPaymentService _payOSPaymentService;


        public ShoppingCartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository,
            MoMoPaymentService moMoPaymentService,
            ILogger<ShoppingCartController> logger,
            IConfiguration configuration,
            PayOSPaymentService payOSPaymentService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _moMoPaymentService = moMoPaymentService;
            _logger = logger;
            _configuration = configuration;
            _payOSPaymentService = payOSPaymentService;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.TotalPrice = cart.Items.Sum(i => (i.Price - (i.PromotionPrice.HasValue ? (i.Price * i.PromotionPrice.Value / 100) : 0)) * i.Quantity);
            return View(cart);
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                TotalPrice = cart.Items.Sum(i => (i.Price - (i.PromotionPrice.HasValue ? (i.Price * i.PromotionPrice.Value / 100) : 0)) * i.Quantity)
            };

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string paymentMethod)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => (i.Price - (i.PromotionPrice.HasValue ? (i.Price * i.PromotionPrice.Value / 100) : 0)) * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            switch (paymentMethod)
            {
                case "payos":
                    try
                    {
                        var responseUrl = await _payOSPaymentService.CreatePaymentRequest(order);
                        _logger.LogInformation("Redirecting to PayOS Payment URL: {0}", responseUrl);
                        return Redirect(responseUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating PayOS payment request");
                        ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tạo yêu cầu thanh toán. Vui lòng thử lại sau.";
                        return View(order);
                    }


                case "momo":
                    try
                    {
                        var paymentService = new MoMoPaymentService(_configuration);
                        var response = await paymentService.CreatePaymentRequest(order);
                        return Redirect(response);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating MoMo payment request");
                        ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tạo yêu cầu thanh toán. Vui lòng thử lại sau.";
                        return View(order);
                    }

                case "cash":
                    return RedirectToAction("CashPayment", new { orderId = order.Id });

                default:
                    ViewBag.ErrorMessage = "Phương thức thanh toán không hợp lệ.";
                    return View(order);
            }
        }


        public IActionResult CashPayment(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Thanh toán bằng tiền mặt";
            _context.SaveChanges();

            return RedirectToAction("OrderCompleted", new { orderId = orderId });
        }
        public IActionResult OrderCompleted(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return RedirectToAction("Index");
            }
            return View(order);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                PromotionPrice = product.PromotionPrice,
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

    }
}