namespace ITStockM.WebApi.Services;

public class UserSession
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime LoginTime { get; set; }
}
