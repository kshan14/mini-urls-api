using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniUrl.Configs;
using MiniUrl.Database;
using MiniUrl.Filters;
using MiniUrl.Services;
using Serilog;
using Serilog.Context;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// configure logging
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());

// configure logging to print to file only if it is non-development envrionment
if (!builder.Environment.IsDevelopment())
{
    loggerConfig.WriteTo.File(
        new Serilog.Formatting.Compact.CompactJsonFormatter(),
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day
    );
}

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();

// Add DB Config and Services
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("DBConfig"));
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var dbConfig = serviceProvider.GetRequiredService<IOptions<DbConfig>>().Value;
    options.UseNpgsql(dbConfig.BuildConnectionString());
});

// Add Redis Config and Services
builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("RedisConfig"));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = sp.GetRequiredService<IOptions<RedisConfig>>().Value;
    // prepare hosts
    var epCollection = new EndPointCollection();
    var hosts = redisConfig.Hosts.Split(",", StringSplitOptions.RemoveEmptyEntries);
    foreach (var h in hosts)
    {
        epCollection.Add(h.Trim());
    }

    var confOptions = new ConfigurationOptions
    {
        EndPoints = epCollection,
        User = redisConfig.Username,
        Password = redisConfig.Password,
        AbortOnConnectFail = redisConfig.AbortConnect,
        ConnectTimeout = redisConfig.ConnectTimeout,
        Ssl = redisConfig.Ssl,
        // Additional recommended settings
        ConnectRetry = 100,
        ReconnectRetryPolicy = new LinearRetry(3000),
        KeepAlive = 60
    };
    return ConnectionMultiplexer.Connect(confOptions);
});

// Add Jwt Config and Required Services
var jwtConfig = new JwtConfig();
builder.Configuration.GetSection("JWTConfig").Bind(jwtConfig);
builder.Services.AddSingleton(jwtConfig);
builder.Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.Key))
        };
    });

builder.Services.AddAuthorization();

// Add Validation Services
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Mini Url", Version = "v1" });
    // Add Fluent Validation Schema to reflect in Swagger
    c.SchemaFilter<FluentValidationSchemaFilter>();
});

// Add Services
builder.Services.AddHttpContextAccessor(); // this is required to get trace id in logger
builder.Services.AddScoped<IBase62Encoder, Base62Encoder>();
builder.Services.AddSingleton<IUrlCounter, UrlCounter>();
builder.Services.AddScoped<IMiniUrlGenerator, MiniUrlGenerator>();

var app = builder.Build();

// Middleware for Logging info
app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    using (LogContext.PushProperty("TraceId", traceId))
    using (LogContext.PushProperty("RequestPath", context.Request.Path))
    using (LogContext.PushProperty("RequestMethod", context.Request.Method))
    {
        await next();
    }
});


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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Staring web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web application terminated exception");
}
finally
{
    Log.CloseAndFlush();
}
