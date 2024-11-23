using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using System.Security.Claims;

namespace FashionShopDemo.Controllers
{
    [Route("api/Orders")]
    [ApiController]
    [Authorize]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API lấy danh sách đơn hàng của người dùng
        [HttpGet("user-orders")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("Không tìm thấy đơn hàng nào.");
            }

            return Ok(orders.Select(order => new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                order.ShippingAddress,
                order.Status,
                OrderDetails = order.OrderDetails.Select(od => new
                {
                    od.Product.Name,
                    od.Product.Price,
                    od.Quantity
                })
            }));
        }

        // API tạo đơn hàng mới
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không tìm thấy thông tin người dùng.");
            }

            if (request.OrderDetails == null || !request.OrderDetails.Any())
            {
                return BadRequest("Đơn hàng phải có ít nhất một sản phẩm.");
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShippingAddress = request.ShippingAddress,
                TotalPrice = request.TotalPrice,
                Status = "Đang xử lý" // Trạng thái mặc định khi tạo đơn
            };

            // Thêm đơn hàng vào cơ sở dữ liệu
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng (OrderDetails)
            foreach (var orderDetailRequest in request.OrderDetails)
            {
                var product = await _context.Products.FindAsync(orderDetailRequest.ProductId);
                if (product == null)
                {
                    return NotFound($"Sản phẩm với ID {orderDetailRequest.ProductId} không tồn tại.");
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = orderDetailRequest.ProductId,
                    Quantity = orderDetailRequest.Quantity,
                    Price = product.Price // Giá sản phẩm tại thời điểm đặt hàng
                };

                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserOrders), new { id = order.Id }, order);
        }
    }

    // Định nghĩa lớp yêu cầu tạo đơn hàng (CreateOrderRequest)
    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderDetailRequest> OrderDetails { get; set; }
    }

    // Định nghĩa lớp chi tiết sản phẩm trong đơn hàng
    public class OrderDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
