namespace FoodOutlet.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public string recipe_name { get; set; }
        public int category_id { get; set; }
        public string recipe_img { get; set; }
        public string description { get; set; } 
        public decimal price { get; set; }
    }
}