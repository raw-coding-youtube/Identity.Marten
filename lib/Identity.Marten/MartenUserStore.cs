using Marten;
using Marten.Linq.MatchesSql;
using Microsoft.AspNetCore.Identity;

namespace Identity.Marten;

public class MartenUserStore<TUser, TRole, TKey> : IUserRoleStore<TUser>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TRole : IdentityRole
{
    private readonly IDocumentSession _documentSession;
    private readonly IRoleStore<TRole> _roleStore;

    public MartenUserStore(
        IDocumentSession documentSession,
        IRoleStore<TRole> roleStore
    )
    {
        _documentSession = documentSession;
        _roleStore = roleStore;
    }

    public void Dispose() { }

    public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString()!);
    }

    public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _documentSession.Insert(user);
        await _documentSession.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _documentSession.Update(user);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _documentSession.Delete(user);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        if (typeof(TKey) == typeof(string))
        {
            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.MatchesSql("data ->> 'Id' = ?", userId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(Guid))
        {
            if (!Guid.TryParse(userId, out var guidId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.MatchesSql("data ->> 'Id' = ?", guidId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(int))
        {
            if (!int.TryParse(userId, out var intId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.MatchesSql("data ->> 'Id' = ?", intId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(long))
        {
            if (!long.TryParse(userId, out var longId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.MatchesSql("data ->> 'Id' = ?", longId), token: cancellationToken);
        }

        throw new ArgumentException($"unsupported key type: {typeof(TKey).FullName}");
    }

    public async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await _documentSession.Query<TUser>()
            .FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, token: cancellationToken);
    }

    public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            throw new Exception($"role with name of {roleName} not found.");
        }

        _documentSession.Store(
            new UserRole<TKey>()
            {
                UserId = user.Id,
                RoleId = role.Id
            }
        );

        await _documentSession.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            throw new Exception($"role with name of {roleName} not found.");
        }

        var userRole = await _documentSession.Query<UserRole<TKey>>()
            .FirstOrDefaultAsync(ur => ur.MatchesSql("data ->> 'UserId' = ?", user)
                                       && ur.RoleId == role.Id, cancellationToken);

        if (userRole == null)
        {
            return;
        }

        _documentSession.Delete(userRole);

        await _documentSession.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var roles = new List<IdentityRole>();
        _ = await _documentSession.Query<UserRole<TKey>>()
            .Include<IdentityRole>(x => x.RoleId, roles.Add)
            .Where(ur => ur.MatchesSql("data ->> 'UserId' = ?", user.Id))
            .ToListAsync(cancellationToken);


        return roles.Select(x => x.Name).ToList()!;
    }

    public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            return false;
        }

        return await _documentSession.Query<UserRole<TKey>>()
            .AnyAsync(ur => ur.MatchesSql("data ->> 'UserId' = ?", user.Id)
                                       && ur.RoleId == role.Id, cancellationToken);
    }

    public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

        var role = await _roleStore.FindByNameAsync(roleName, cancellationToken);
        if (role == null)
        {
            throw new Exception($"role with name of {roleName} not found.");
        }

        var users = new List<TUser>();
        _ = await _documentSession.Query<UserRole<TKey>>()
            .Include<TUser>(x => x.UserId, users.Add)
            .Where(ur => ur.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        return users;
    }
}