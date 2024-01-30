using System.Net;

namespace Test.Identity.Marten;

public class AspNetCoreIntegration : IntegrationTest
{
    [Fact]
    public async Task create_and_read_user()
    {
        var createResult = await Client.GetAsync("/add-user");
        Assert.Equal(HttpStatusCode.OK, createResult.StatusCode);
        
        var getResult = await Client.GetAsync("/user?name=bob");
        Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
    }
    
    [Fact]
    public async Task create_and_read_role()
    {
        var createResult = await Client.GetAsync("/add-role?name=test_role");
        Assert.Equal(HttpStatusCode.OK, createResult.StatusCode);
        
        var getResult = await Client.GetAsync("/role?name=test_role");
        Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
    }

    public AspNetCoreIntegration(IntegrationFixture integrationFixture) : base(integrationFixture) { }
}