using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniUrl.Configs;
using MiniUrl.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add DB Config and Services
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("DBConfig"));
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var dbConfig = serviceProvider.GetRequiredService<IOptions<DbConfig>>().Value;
    options.UseNpgsql(dbConfig.BuildConnectionString());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();