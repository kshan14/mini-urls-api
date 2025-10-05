namespace MiniUrl.Models.Responses.Login;

public class LoginResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
