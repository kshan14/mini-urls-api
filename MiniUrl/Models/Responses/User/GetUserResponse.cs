namespace MiniUrl.Models.Responses.User;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}