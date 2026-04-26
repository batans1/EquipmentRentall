using System.ComponentModel.DataAnnotations;
using EquipmentRental.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentRental.Business;

public record CurrentUser(int Id, string Username, string Role, string FullName);

public class RegisterInput
{
    [Required, MaxLength(32)] public string Username { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string LastName { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}

public class LoginInput
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RentalCreateInput
{
    [Required] public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required] public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    [Required, MaxLength(255)] public string Purpose { get; set; } = string.Empty;
    public Dictionary<int, int> Quantities { get; set; } = new();
}

public class AdminDashboardVm
{
    public int UsersCount { get; set; }
    public int EquipmentCount { get; set; }
    public int RequestsCount { get; set; }
    public int PendingRequestsCount { get; set; }
}

public interface IAuthService
{
    Task<(bool ok, string? error)> RegisterAsync(RegisterInput input);
    Task<CurrentUser?> LoginAsync(LoginInput input);
}

public class AuthService(EquipmentRentalDbContext db) : IAuthService
{
    public async Task<(bool ok, string? error)> RegisterAsync(RegisterInput input)
    {
        if (await db.Users.AnyAsync(x => x.Username == input.Username))
        {
            return (false, "Потребителското име е заето.");
        }

        var role = await db.Roles.FirstAsync(x => x.Name == "User");
        db.Users.Add(new User
        {
            Username = input.Username,
            PasswordHash = PasswordHasher.Hash(input.Password),
            FirstName = input.FirstName,
            LastName = input.LastName,
            RoleId = role.Id
        });
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<CurrentUser?> LoginAsync(LoginInput input)
    {
        var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Username == input.Username);
        if (user is null || !PasswordHasher.Verify(input.Password, user.PasswordHash) || user.Role is null)
        {
            return null;
        }

        return new CurrentUser(user.Id, user.Username, user.Role.Name, $"{user.FirstName} {user.LastName}");
    }
}

public interface IAppService
{
    Task<AdminDashboardVm> GetDashboardAsync();
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserAsync(int id);
    Task<bool> CreateUserAsync(RegisterInput input);
    Task<bool> UpdateUserAsync(int id, RegisterInput input);
    Task DeleteUserAsync(int id);
    Task<List<EquipmentItem>> GetEquipmentAsync(string? search);
    Task<EquipmentItem?> GetEquipmentByIdAsync(int id);
    Task SaveEquipmentAsync(EquipmentItem item);
    Task DeleteEquipmentAsync(int id);
    Task<(bool ok, string? error)> CreateRequestAsync(int userId, RentalCreateInput input);
    Task<List<RentalRequest>> GetMyRequestsAsync(int userId);
    Task<List<RentalRequest>> GetAllRequestsAsync();
    Task<bool> DeletePendingRequestAsync(int userId, int id);
    Task UpdateRequestStatusAsync(int id, RentalRequestStatus status);
}

public class AppService(EquipmentRentalDbContext db) : IAppService
{
    public async Task<AdminDashboardVm> GetDashboardAsync()
    {
        return new AdminDashboardVm
        {
            UsersCount = await db.Users.CountAsync(),
            EquipmentCount = await db.EquipmentItems.CountAsync(),
            RequestsCount = await db.RentalRequests.CountAsync(),
            PendingRequestsCount = await db.RentalRequests.CountAsync(x => x.Status == RentalRequestStatus.Pending)
        };
    }

    public Task<List<User>> GetUsersAsync() => db.Users.Include(x => x.Role).Where(x => x.Role!.Name == "User").ToListAsync();
    public Task<User?> GetUserAsync(int id) => db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);

    public async Task<bool> CreateUserAsync(RegisterInput input)
    {
        if (await db.Users.AnyAsync(x => x.Username == input.Username)) return false;
        var userRole = await db.Roles.FirstAsync(x => x.Name == "User");
        db.Users.Add(new User
        {
            Username = input.Username,
            PasswordHash = PasswordHasher.Hash(input.Password),
            FirstName = input.FirstName,
            LastName = input.LastName,
            RoleId = userRole.Id
        });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(int id, RegisterInput input)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;
        if (await db.Users.AnyAsync(x => x.Username == input.Username && x.Id != id)) return false;
        user.Username = input.Username;
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        if (!string.IsNullOrWhiteSpace(input.Password)) user.PasswordHash = PasswordHasher.Hash(input.Password);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return;
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    public Task<List<EquipmentItem>> GetEquipmentAsync(string? search)
    {
        var query = db.EquipmentItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search));
        }
        return query.OrderBy(x => x.Name).ToListAsync();
    }

    public Task<EquipmentItem?> GetEquipmentByIdAsync(int id) => db.EquipmentItems.FindAsync(id).AsTask();

    public async Task SaveEquipmentAsync(EquipmentItem item)
    {
        if (item.Id == 0) db.EquipmentItems.Add(item);
        else db.EquipmentItems.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task DeleteEquipmentAsync(int id)
    {
        var item = await db.EquipmentItems.FindAsync(id);
        if (item is null) return;
        db.EquipmentItems.Remove(item);
        await db.SaveChangesAsync();
    }

    public async Task<(bool ok, string? error)> CreateRequestAsync(int userId, RentalCreateInput input)
    {
        if (input.EndDate < input.StartDate) return (false, "Крайната дата трябва да е след началната.");
        var selected = input.Quantities.Where(x => x.Value > 0).ToList();
        if (selected.Count == 0) return (false, "Изберете поне един артикул.");

        var equipmentIds = selected.Select(x => x.Key).ToList();
        var equipment = await db.EquipmentItems.Where(x => equipmentIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);

        foreach (var item in selected)
        {
            if (!equipment.TryGetValue(item.Key, out var eq) || item.Value > eq.AvailableQuantity)
            {
                return (false, "Невалидно количество за избрано оборудване.");
            }
        }

        var request = new RentalRequest
        {
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Purpose = input.Purpose,
            UserId = userId,
            Status = RentalRequestStatus.Pending
        };

        foreach (var item in selected)
        {
            request.RentalRequestItems.Add(new RentalRequestItem
            {
                EquipmentItemId = item.Key,
                Quantity = item.Value
            });
        }

        db.RentalRequests.Add(request);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public Task<List<RentalRequest>> GetMyRequestsAsync(int userId) => db.RentalRequests
        .Where(x => x.UserId == userId)
        .Include(x => x.RentalRequestItems)
        .ThenInclude(x => x.EquipmentItem)
        .OrderByDescending(x => x.Id)
        .ToListAsync();

    public Task<List<RentalRequest>> GetAllRequestsAsync() => db.RentalRequests
        .Include(x => x.User)
        .Include(x => x.RentalRequestItems)
        .ThenInclude(x => x.EquipmentItem)
        .OrderByDescending(x => x.Id)
        .ToListAsync();

    public async Task<bool> DeletePendingRequestAsync(int userId, int id)
    {
        var req = await db.RentalRequests
            .Include(x => x.RentalRequestItems)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (req is null || req.Status != RentalRequestStatus.Pending) return false;
        db.RentalRequestItems.RemoveRange(req.RentalRequestItems);
        db.RentalRequests.Remove(req);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateRequestStatusAsync(int id, RentalRequestStatus status)
    {
        var req = await db.RentalRequests.FindAsync(id);
        if (req is null) return;
        req.Status = status;
        await db.SaveChangesAsync();
    }
}
