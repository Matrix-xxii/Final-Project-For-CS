namespace FoodOutlet.Models
{
    public class Recipe
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int qty { get; set; }
        public string qty_status { get; set; }
    }
}