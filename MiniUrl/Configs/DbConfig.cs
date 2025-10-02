using Npgsql;

namespace MiniUrl.Configs;

public class DbConfig
{
    public string Host { get; set; } = "localhost";
    public string Port { get; set; } = "5432";
    public string Database { get; set; } = "mini_url";
    public string Username { get; set; } = "username";
    public string Password { get; set; } = "password";
    public int? Timeout { get; set; }
    public int? CommandTimeout { get; set; }

    public string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = int.TryParse(Port, out var port) ? port : 5432,
            Database = Database,
            Username = Username,
            Password = Password,
            Timeout = Timeout ?? 30,
            CommandTimeout = CommandTimeout ?? 30
        };
        return builder.ToString();
    }
}