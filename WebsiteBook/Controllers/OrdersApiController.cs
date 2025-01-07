using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using NuGet.Common;
using Vonage.Messages.Webhooks;

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
            string userId = null;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jsonToken = HttpContext.Items["JsonToken"] as JwtSecurityToken;
            if (jsonToken != null)
            {
                userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                Console.WriteLine($"User ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không tìm thấy thông tin người dùng.");
                }
            }
            else
            {
                return Unauthorized("Token không hợp lệ.");
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
                Status = request.Status,
                Notes = request.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderDetails = new List<OrderDetail>();

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
                orderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // Nạp danh sách chi tiết vào đơn hàng để trả về
            var response = new
            {
                order.Id,
                order.UserId,
                order.OrderDate,
                order.TotalPrice,
                order.ShippingAddress,
                order.Status,
                order.Notes,
                OrderDetails = orderDetails.Select(od => new
                {
                    od.ProductId,
                    od.Quantity,
                    od.Price
                }).ToList()
            };

            return CreatedAtAction(nameof(GetUserOrders), new { id = order.Id }, response);
        }


    }

    // DTO Classes
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }

    public class OrderDetailDto
    {
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int Quantity { get; set; }
    }



    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public List<OrderDetailRequest> OrderDetails { get; set; }
    }

    public class OrderDetailRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
/* goc
 * using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionShopDemo.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
            string userId = null;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jsonToken = HttpContext.Items["JsonToken"] as JwtSecurityToken;
            if (jsonToken != null)
            {
                // Lấy userId từ jsonToken
                userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                Console.WriteLine($"User ID: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không tìm thấy thông tin người dùng.");
                }
            }
            else
            {
                return Unauthorized("Token không hợp lệ.");
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


            return CreatedAtAction(nameof(GetUserOrders), new { id = order.Id }, new { order.Id });
        }

    }

    // DTO Classes
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }

    public class OrderDetailDto
    {
        public string ProductName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int Quantity { get; set; }
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
*/