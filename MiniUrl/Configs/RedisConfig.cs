namespace MiniUrl.Configs;

public class RedisConfig
{
    public string Hosts { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool AbortConnect { get; set; }
    public int ConnectTimeout { get; set; }
    public bool Ssl { get; set; }
}
