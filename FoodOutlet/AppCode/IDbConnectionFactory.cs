using MySql.Data.MySqlClient;

namespace FoodOutlet.AppCode
{
    public interface IDbConnectionFactory
    {
        MySqlConnection CreateConnection();
    }
}
