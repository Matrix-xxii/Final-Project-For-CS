using MySql.Data.MySqlClient;

namespace FoodOutlet.AppCode
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
