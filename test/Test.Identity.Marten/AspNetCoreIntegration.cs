using System.Net;

namespace Test.Identity.Marten;

public class AspNetCoreIntegration : IntegrationTest
{
    [Fact]
    public async Task create_and_read_user()
    {
        var createResult = await Client.GetAsync("/add-user");
        Assert.Equal(HttpStatusCode.OK, createResult.StatusCode);
        
        var getResult = await Client.GetAsync("/?name=bob");
        Assert.Equal(HttpStatusCode.OK, getResult.StatusCode);
    }

    public AspNetCoreIntegration(IntegrationFixture integrationFixture) : base(integrationFixture) { }
}