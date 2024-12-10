using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using System.Security.Claims;

namespace FashionShopDemo.Controllers
{
    [Route("api/Orders")]
    [ApiController]
    
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user-orders/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId không hợp lệ.");
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
                order.Notes,
                OrderDetails = order.OrderDetails.Select(od => new
                {
                    od.Product.Name,
                    OriginalPrice = od.Price,
                    FinalPrice = od.FinalPrice,
                    od.Quantity
                })
            }));
        }



        [HttpPost("create")]
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

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShippingAddress = request.ShippingAddress,
                TotalPrice = request.TotalPrice,
                Status = "Đang xử lý",
                Notes = request.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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
                    Price = product.FinalPrice
                };

                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserOrders), new { id = order.Id }, order);
        }

    }

    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; } 

        public List<OrderDetailRequest> OrderDetails { get; set; }
    }

    public class OrderDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
