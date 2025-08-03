using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using StopGame.Application.Interfaces;
using StopGame.Application.Services;
using StopGame.Infrastructure.Data;
using StopGame.Infrastructure.Scheduling;
using StopGame.Infrastructure.Repositories;
using StopGame.Infrastructure.Services;
using StopGame.Web.Services;
using StopGame.Web.Hubs;
using StackExchange.Redis;
using Hangfire;
using Hangfire.Redis.StackExchange;
using StopGame.Infrastructure.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Entity Framework
builder.Services.AddDbContext<StopGameDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// Add distributed lock service from Infrastructure
builder.Services.AddInfrastructureLocks(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

// Register repositories
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IRoomRepository, RedisRoomRepository>();

// Register services
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAnswerSubmissionService, AnswerSubmissionService>();
builder.Services.AddScoped<IAppJobScheduler, StopGame.Infrastructure.Scheduling.HangfireJobScheduler>();

// Add Hangfire
builder.Services.AddHangfire(config => config
    .UseRedisStorage(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", new RedisStorageOptions
    {
        Prefix = "hangfire:stopgame:"
    }));

builder.Services.AddHangfireServer();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new string[] { };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gameHub");
app.UseHangfireDashboard();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StopGameDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
