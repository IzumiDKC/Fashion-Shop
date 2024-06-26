using FashionShop.Models;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Drawing2D;

namespace FashionShopDemo.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm"), StringLength(100)]
        public string? Name { get; set; }
        [Required, Range(1, 10000)]    
        public decimal Price { get; set; }
        public decimal? PromotionPrice { get; set; } = 0;
        public decimal FinalPrice
        {
            get
            {
                if (PromotionPrice.HasValue && PromotionPrice.Value > 0)
                {
                    decimal discount = Price * (PromotionPrice.Value / 100);
                    return Price - discount;
                }
                else
                {
                    return Price;
                }
            }
        }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<Images>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool Status { get; set; } = true;
        public string? SeoTitle { get; set; }
        public int? Quantity { get; set; }
        public bool IsHot { get; set; } = false;
        public DateTime? HotStartDate { get; set; }
        public DateTime? HotEndDate { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string? MetaKeyword { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public int? ProductCommentId { get; set; }
        public ProductComment? ProductComment { get; set; }

    }
}
