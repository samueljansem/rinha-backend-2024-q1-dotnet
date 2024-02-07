using Npgsql;

public class SqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NpgsqlConnection Create() => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    
}