namespace ITStockM.Application.Common.Interfaces;

public interface IUserService
{
    Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
}
