namespace FoodOutlet.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public string recipe_name { get; set; }
        public int category_id { get; set; }
        public decimal price { get; set; }
    }
}