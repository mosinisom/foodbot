using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DbService>();
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")), ServiceLifetime.Singleton);
builder.Services.AddSingleton<TelegramService>();
builder.Services.AddSingleton<AppLogicService>();

var app = builder.Build();

app.Services.GetRequiredService<TelegramService>().Start();

app.Run();

