using JasperFx.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Identity.Marten;

public class UserAndRoleIntegrationTests : IntegrationTest
{
    public UserAndRoleIntegrationTests(IntegrationFixture integrationFixture) : base(integrationFixture) { }

    [Fact]
    public async Task can_add_role_to_user()
    {
        var rMgr = Services.GetRequiredService<RoleManager<IdentityRole>>();
        var uMgr = Services.GetRequiredService<UserManager<IdentityUser>>();

        var roleName = Guid.NewGuid().ToString();
        var createRoleResult = await rMgr.CreateAsync(new IdentityRole(roleName));
        Assert.True(createRoleResult.Succeeded);

        var user = new IdentityUser()
        {
            Email = $"{Guid.NewGuid()}@test.com",
            UserName = Guid.NewGuid().ToString()
        };
        var createUserResult = await uMgr.CreateAsync(user);
        Assert.True(createUserResult.Succeeded);

        var addToRoleResult = await uMgr.AddToRoleAsync(user, roleName);
        Assert.True(addToRoleResult.Succeeded);

        var getRolesResult = await uMgr.GetRolesAsync(user);
        var assignedRole = Assert.Single(getRolesResult);
        Assert.Equal(roleName, assignedRole);
    }
}