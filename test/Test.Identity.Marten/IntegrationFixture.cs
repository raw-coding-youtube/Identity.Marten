using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Test.Identity.Marten;

public class IntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public IntegrationFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("identity_marten_test")
            .WithPortBinding(49111, 5432)
            .WithUsername("postgres")
            .WithPassword("password")
            .WithImage("postgres:15.1")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        App = new MockApp(_postgreSqlContainer.GetConnectionString());
        Client = App.CreateClient();
    }

    public MockApp App { get; set; }
    public HttpClient Client { get; set; }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }

    public class MockApp : WebApplicationFactory<Program>
    {
        private readonly string _postgresqlConnection;

        public MockApp(string postgresqlConnection)
        {
            _postgresqlConnection = postgresqlConnection;
        }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.ConfigureMarten(storeOptions =>
                {
                    storeOptions.Connection(_postgresqlConnection);
                });
            });
        }
    }
}

[CollectionDefinition(nameof(IntegrationFixtureCollection))]
public class IntegrationFixtureCollection : ICollectionFixture<IntegrationFixture> { }

[Collection(nameof(IntegrationFixtureCollection))]
public class IntegrationTest : IAsyncLifetime
{
    public IntegrationTest(IntegrationFixture integrationFixture)
    {
        IntegrationFixture = integrationFixture;
    }

    public IntegrationFixture IntegrationFixture { get; }
    public HttpClient Client => IntegrationFixture.Client;
    public IServiceScope Scope { get; set; }
    public IServiceProvider Services => Scope.ServiceProvider;
    
    public Task InitializeAsync()
    {
        Scope = IntegrationFixture.App.Services.CreateScope();
        return Task.CompletedTask;
    }
    
    public Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }
}