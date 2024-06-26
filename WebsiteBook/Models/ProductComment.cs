using FashionShopDemo.Models;
using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models
{
    public class ProductComment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung!")]
        [StringLength(100)] public required string Content { get; set; }
        public bool Status { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public List<Product>? Products { get; set; }

    }
}
