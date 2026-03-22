namespace FoodOutlet.Models
{
    public class Inventory
    {
        public int id { get; set; } = 0;
        public int recipe_id { get; set; } = 0;
        public int stock_qty { get; set; } = 0;
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? recipe_name { get; set; }
        public string? recipe_img { get; set; }
    }
}
