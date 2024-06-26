namespace FashionShopDemo.Models
{
    public class CartItem
    {
        public int ImageUrl { get; set; }
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public decimal? PromotionPrice { get; set; } // Thêm thuộc tính PromotionPrice
        public static CartItem FromProduct(Product product)
        {
            return new CartItem
            {
                PromotionPrice = product.PromotionPrice
            };
        }
        public decimal FinalPrice
        {
            get
            {
                decimal finalPrice;
                if (Product != null)
                {
                    return finalPrice = Product.FinalPrice; // Sử dụng FinalPrice từ Product
                }
                else
                {
                    return finalPrice = Price; // Sử dụng giá gốc nếu không có Product
                }
            }
        }
    }
}
