using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FashionShopDemo.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public int? ParentId { get; set; } // ID của danh mục cha (nullable)
            
        [ForeignKey("ParentId")] [JsonIgnore]
        [ValidateNever]

        public virtual Category Parent { get; set; }

        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    }
}
