using Microsoft.Extensions.Configuration;
using Npgsql;

public interface INpgsqlConnectionFactory
{
    NpgsqlConnection Create();
}

public class NpgsqlConnectionFactory : INpgsqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NpgsqlConnection Create() => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
}