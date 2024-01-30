using Marten;
using Microsoft.AspNetCore.Identity;

namespace Identity.Marten;

public class MartenRoleStore<TRole> : IRoleStore<TRole>
    where TRole : IdentityRole
{
    private readonly IDocumentSession _documentSession;

    public MartenRoleStore(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public void Dispose() { }

    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        _documentSession.Insert(role);
        await _documentSession.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        _documentSession.Update(role);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        _documentSession.Delete(role);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        
        role.Name = roleName;
        
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(role, nameof(role));
        
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<TRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
        return await _documentSession.LoadAsync<TRole>(roleId, cancellationToken);
    }

    public async Task<TRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(normalizedRoleName, nameof(normalizedRoleName));
        
        return await _documentSession.Query<TRole>()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, token: cancellationToken);
    }
}