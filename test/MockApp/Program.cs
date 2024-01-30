using Identity.Marten;
using Marten;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten("host=127.0.0.1;port=5432;username=postgres;password=password;database=test;");

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddMartenStore();

var app = builder.Build();

app.MapGet("/user", (string name, UserManager<IdentityUser> uMgr) => uMgr.FindByNameAsync(name));

app.MapGet("/add-user", async (UserManager<IdentityUser> uMgr) =>
{
    var result = await uMgr.CreateAsync(new IdentityUser()
    {
        Email = "test@test.com",
        UserName = "bob",
    });

    return Results.Ok(result);
});

app.MapGet("/role", async (string name, RoleManager<IdentityRole> rMgr) =>
{
    var role = await rMgr.FindByNameAsync(name);
    return Results.Ok(role);
});

app.MapGet("/add-role", async (string name, RoleManager<IdentityRole> rMgr) =>
{
    var result = await rMgr.CreateAsync(new IdentityRole(name));
    return Results.Ok(result);
});

// app.MapGet("/set-admin", async (UserManager<IdentityUser> uMgr) =>
// {
//     var result = await uMgr.CreateAsync(new IdentityUser()
//     {
//         Email = "test@test.com",
//         UserName = "bob",
//     });
//
//     uMgr.AddToRoleAsync()
//     return Results.Ok(result);
// });

app.Run();

public partial class Program
{
    
} 