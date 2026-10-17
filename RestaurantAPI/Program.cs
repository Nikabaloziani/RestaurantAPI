using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RestaurantDAL.Repositories;
using RestaurantDATA.DB_Context;
using RestaurantDATA.Entities;
using RestaurantDATA.Infrastructures;
using RestaurantDATA.Roles;
using RestaurantSERVICE;
using RestaurantSERVICE.Interfaces;
using RestaurantSERVICE.Services;
using System.Security.Claims;
using System.Text;
using ProductRepository = RestaurantDAL.Repositories.ProductRepository;

var builder = WebApplication.CreateBuilder(args);


var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}


static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

static string? ResolveConnectionString(IConfiguration configuration)
{
    var direct = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(direct))
    {
        return direct;
    }

    var databaseUrl = Environment.GetEnvironmentVariable("${{ Postgres.DATABASE_PRIVATE_URL }}");
    if (!string.IsNullOrWhiteSpace(databaseUrl) &&
        Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
    {
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={Quote(uri.Host)};Port={port};Database={Quote(database)};" +
               $"Username={Quote(user)};Password={Quote(password)}";
    }

    var host = Environment.GetEnvironmentVariable("PGHOST");
    var db = Environment.GetEnvironmentVariable("PGDATABASE");
    var pgUser = Environment.GetEnvironmentVariable("PGUSER");
    var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
    var pgPort = Environment.GetEnvironmentVariable("PGPORT");

    if (!string.IsNullOrWhiteSpace(host) &&
        !string.IsNullOrWhiteSpace(db) &&
        !string.IsNullOrWhiteSpace(pgUser))
    {
        return $"Host={Quote(host)};Port={(string.IsNullOrWhiteSpace(pgPort) ? "5432" : pgPort)};" +
               $"Database={Quote(db)};Username={Quote(pgUser)};Password={Quote(pgPassword ?? "")}";
    }

    return null;
}

var connectionString = ResolveConnectionString(builder.Configuration);

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQL configuration is missing. Set ConnectionStrings__DefaultConnection, " +
        "or DATABASE_URL, or these environment variables on the API service: " +
        "PGHOST, PGDATABASE, PGUSER, PGPASSWORD");
}

builder.Services.AddDbContext<RestaurantDB>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Write 'Bearer ' followed by your JWT token.\n\n" +
            "Example: Bearer eyJhbGciOi..."
    });

    options.AddSecurityRequirement(doc =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", doc),
                new List<string>()
            }
        });
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<RestaurantDB>()
.AddDefaultTokenProviders();

// JWT Configuration
// On Railway set: Jwt__Key, Jwt__Issuer, Jwt__Audience
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Set the Jwt__Key environment variable.");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Jwt:Issuer and Jwt:Audience must be configured. " +
        "Set the Jwt__Issuer and Jwt__Audience environment variables.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),

        RoleClaimType = ClaimTypes.Role
    };
});

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => { },
    AppDomain.CurrentDomain.GetAssemblies());

// Repositories
builder.Services.AddScoped(
    typeof(IBaseRepository<>),
    typeof(BaseRepository<>));

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Apply migrations and seed roles/admin on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<RestaurantDB>();

    await dbContext.Database.MigrateAsync();

    await DataSeeder.SeedRolesAndAdminAsync(services);
}

// Swagger is enabled in every environment so the API can be demonstrated
// on Railway (the default environment there is Production).
app.UseSwagger();
app.UseSwaggerUI();

// Railway terminates HTTPS at its proxy, so redirect only when developing locally.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Run();