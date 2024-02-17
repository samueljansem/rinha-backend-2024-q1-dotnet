using Microsoft.Extensions.Configuration;
using Npgsql;

public interface INpgsqlConnectionFactory
{
    NpgsqlConnection Create();
}

public class NpgsqlConnectionFactory(IConfiguration _configuration) : INpgsqlConnectionFactory
{
    public NpgsqlConnection Create() => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
}