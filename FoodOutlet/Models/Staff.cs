namespace FoodOutlet.Models
{
    public class Staff
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public DateTime? birth_of_date { get; set; }
        public int role_id { get; set; } = 0;
        public string  phone_no { get; set; } ="";
        public string address { get; set; } = "";
        public string password { get; set; } = "";
        public string status { get; set; } = "";

    }


    public class Role
    {
        public int id { get; set; } = 0;
        public string role_name { get; set; } = "";
    }

    public class Resign
    {
        public int id { get; set; } = 0;
        public int registration_id { get; set; } = 0;
        public string reason { get; set; } = "";
        public DateTime resign {  get; set; }
    }

    public class Message
    {
        public string message { get; set; } = "";
    }
}
