using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Identity.Marten;

public class IUserStoreTests : IntegrationTest
{
    public IUserStoreTests(IntegrationFixture integrationFixture) : base(integrationFixture) { }

    [Fact]
    public async Task crud_operations()
    {
        var userId = Guid.NewGuid().ToString();

        var store = Services.GetRequiredService<IUserStore<IdentityUser>>();
        var createResult = await store.CreateAsync(
            new IdentityUser()
            {
                Id = userId,
                UserName = "tony"
            },
            CancellationToken.None
        );
        Assert.True(createResult.Succeeded);

        var user = await store.FindByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("tony", user.UserName);

        user.UserName = "boloney";
        var updateResult =await store.UpdateAsync(user, CancellationToken.None);
        Assert.True(updateResult.Succeeded);
        
        user = await store.FindByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("boloney", user.UserName);

        var deleteResult =await store.DeleteAsync(user, CancellationToken.None);
        Assert.True(deleteResult.Succeeded);
        
        user = await store.FindByIdAsync(userId, CancellationToken.None);
        Assert.Null(user);
    }
}