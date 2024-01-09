using Identity.Marten;
using Marten;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten("host=127.0.0.1;port=5432;username=postgres;password=password;database=test;");

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddMartenStore();

var app = builder.Build();

app.MapGet("/", (string name, UserManager<IdentityUser> uMgr) => uMgr.FindByNameAsync(name));

app.MapGet("/add-user", async (UserManager<IdentityUser> uMgr) =>
{
    var result = await uMgr.CreateAsync(new IdentityUser()
    {
        Email = "test@test.com",
        UserName = "bob",
    });

    return Results.Ok(result);
});

app.Run();