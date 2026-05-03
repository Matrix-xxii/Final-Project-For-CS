namespace FoodOutlet.Models
{
    public class Order
    {
        public int id { get; set; }
        public int? payment_id { get; set; }
        public int table_id { get; set; }
        public int? recipe_id { get; set; }
        public int? order_detail_id { get; set; }
        public string status { get; set; } = "Pending";
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class OrderDetail
    {
        public int id { get; set; }
        public int table_id { get; set; }
        public int recipe_id { get; set; }
        public int qty { get; set; }
        public int? status_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class CreateOrderRequest
    {
        public int table_number { get; set; }
        public List<OrderItem> items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int recipe_id { get; set; }
        public int qty { get; set; }
    }
}
