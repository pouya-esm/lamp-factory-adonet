using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LampFactory.Infrastructure.Persistence
{
    public class ApplicationDbContext
    {
        private readonly string _connectionString;
        public ApplicationDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
