using System.ComponentModel.DataAnnotations;

namespace FashionShopDemo.Models
{
    public class Brand
    {
        public int Id { get; set; }
        [Required, StringLength(50)] public string Name { get; set; }

       // public List<Product>? Products { get; set; } // tắt tham chiếu 2 chiều 14/12/2024

       // public List<Category>? Categories { get; set; }
    }
}
