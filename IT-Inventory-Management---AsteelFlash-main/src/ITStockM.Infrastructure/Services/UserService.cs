using ITStockM.Application.Common.Interfaces;
using ITStockM.Data;
using Microsoft.EntityFrameworkCore;

namespace ITStockM.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly ITStockManagmentContext _context;

    public UserService(ITStockManagmentContext context)
    {
        _context = context;
    }

    public async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(userId, out var parsedId))
        {
            return null;
        }

        return await _context.Employees
            .Where(e => e.Id == parsedId)
            .Select(e => e.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(userId, out var parsedId))
        {
            return false;
        }

        var userRole = await _context.Employees
            .Where(e => e.Id == parsedId)
            .Select(e => e.Role)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
    }
}
