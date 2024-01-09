using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Marten;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddMartenStore(this IdentityBuilder identityBuilder)
    {
        var userType = typeof(MartenUserStore<,>).MakeGenericType(
            identityBuilder.UserType,
            identityBuilder.UserType.GenericTypeArguments.Length == 1
                ? identityBuilder.UserType.GenericTypeArguments[0]
                : identityBuilder.UserType.BaseType?.GenericTypeArguments[0]
                  ?? throw new ArgumentException("bad user type, couldn't find key type.")
        );
        // roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, keyType);

        identityBuilder.Services.AddScoped(typeof(IUserStore<>).MakeGenericType(identityBuilder.UserType), userType);
        identityBuilder.Services.AddScoped<IRoleStore<IdentityRole>, MockRoleStore>();

        return identityBuilder;
    }
}

public class MartenUserStore<TUser, TKey> : IUserStore<TUser>
    where TUser : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly IDocumentSession _documentSession;

    public MartenUserStore(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
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
                .FirstOrDefaultAsync(x => x.Id.Equals(userId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(Guid))
        {
            if (!Guid.TryParse(userId, out var guidId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.Equals(guidId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(int))
        {
            if (!int.TryParse(userId, out var intId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.Equals(intId), token: cancellationToken);
        }

        if (typeof(TKey) == typeof(long))
        {
            if (!long.TryParse(userId, out var longId))
            {
                return null;
            }

            return await _documentSession.Query<TUser>()
                .FirstOrDefaultAsync(x => x.Id.Equals(longId), token: cancellationToken);
        }

        throw new ArgumentException($"unsupported key type: {typeof(TKey).FullName}");
    }

    public async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await _documentSession.Query<TUser>()
            .FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, token: cancellationToken);
    }
}

public class MockRoleStore : IRoleStore<IdentityRole>
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}