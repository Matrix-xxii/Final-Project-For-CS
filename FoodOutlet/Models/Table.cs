namespace FoodOutlet.Models
{
    public class Table
    {
        public int id { get; set; }
        public int table_number { get; set; }
        public string qr_code { get; set; } = string.Empty;
        public DateTime created_at { get; set; }
    }
}