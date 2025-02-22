﻿using System.ComponentModel.DataAnnotations;

namespace FashionShopDemo.Models
{
    public class Brand
    {
        public int Id { get; set; }
        [Required, StringLength(50)] public string Name { get; set; }
        public List<Product>? Products { get; set; }

   //     public List<Category>? Categories { get; set; }
    }
}
