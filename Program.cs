using Microsoft.EntityFrameworkCore;
using ElevatorSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services
builder.Services.AddOpenApi(); // Optional if using OpenAPI
builder.Services.AddControllers();

// ✅ Add CORS for frontend connection
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // 🔁 React dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // 👈 Needed for SignalR
    });
});

builder.Services.AddSignalR();

builder.Services.AddDbContext<ElevatorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add BackgroundService (if implemented)
builder.Services.AddHostedService<ElevatorMovementService>(); //

var app = builder.Build();

// ✅ Dev tools (Swagger / OpenAPI)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Or: app.UseSwagger(); app.UseSwaggerUI();
}

app.Urls.Add("http://localhost:5285");

// ✅ Middleware
app.UseCors(); // ⬅ MUST be before routing/auth if using CORS
app.UseHttpsRedirection();
app.UseAuthorization();

// ✅ Map API + SignalR Hub
app.MapControllers();
app.MapHub<ElevatorHub>("/elevatorHub"); // Make sure frontend matches this exact path

app.Run(); // 🔁 Required to run the server
