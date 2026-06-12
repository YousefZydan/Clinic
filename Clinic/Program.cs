using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Hubs;
using Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using MyProject.Infrastructure.Extensions;
using Serilog;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()   
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



// Fire base Admin SDK initialization

// ---------- Firebase ----------
var firebaseRelativePath = builder.Configuration["Firebase:CredentialsPath"];
var firebaseFullPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseRelativePath);

Console.WriteLine($"🔥 Firebase Path: {firebaseFullPath}");
Console.WriteLine($"🔥 Exists: {File.Exists(firebaseFullPath)}");

if (!File.Exists(firebaseFullPath))
{
    throw new Exception($"Firebase JSON not found at: {firebaseFullPath}");
}

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseFullPath)
    });

    Console.WriteLine("🔥 Firebase Initialized Successfully");
}




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

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }

    await next();
});

// بعده ييجي redirect
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

// ---------- Run ----------
try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}



public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = ex.Message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}