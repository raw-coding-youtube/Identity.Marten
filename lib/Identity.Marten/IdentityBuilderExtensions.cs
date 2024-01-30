using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Marten;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddMartenStore(this IdentityBuilder identityBuilder)
    {
        var userStoreType = typeof(MartenUserStore<,,>).MakeGenericType(
            identityBuilder.UserType,
            identityBuilder.RoleType,
            identityBuilder.UserType.GenericTypeArguments.Length == 1
                ? identityBuilder.UserType.GenericTypeArguments[0]
                : identityBuilder.UserType.BaseType?.GenericTypeArguments[0]
                  ?? throw new ArgumentException("bad user type, couldn't find key type.")
        );

        identityBuilder.Services.AddScoped(
            typeof(IUserStore<>).MakeGenericType(identityBuilder.UserType),
            userStoreType
        );
        
        identityBuilder.Services.AddScoped(
            typeof(IRoleStore<>).MakeGenericType(identityBuilder.RoleType),
            typeof(MartenRoleStore<>).MakeGenericType(identityBuilder.RoleType)
        );

        return identityBuilder;
    }
}