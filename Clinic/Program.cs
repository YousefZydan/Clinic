using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using MyProject.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()   // 👈 خليها Information
    .WriteTo.Console()
    .WriteTo.File("logs2/app-log-.txt",rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ---------- Seeding (Safe) ----------
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    await DefaultRolesAndUsersSeeder.SeedAsync(roleManager, userManager);
    await ApplicationSeeder.SeedAsync(context, userManager);
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error during database seeding");
}

// ---------- Middleware ----------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyProject API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ---------- Run ----------
try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
