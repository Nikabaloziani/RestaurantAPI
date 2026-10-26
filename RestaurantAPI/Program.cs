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

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// RAILWAY PORT
// ==========================================

var railwayPort = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

// ==========================================
// POSTGRESQL CONNECTION
// ==========================================

static string Quote(string value) =>
    "\"" + value.Replace("\"", "\"\"") + "\"";

static string? ResolveConnectionString(
    IConfiguration configuration)
{
    var databaseUrl =
        Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl) &&
        Uri.TryCreate(
            databaseUrl,
            UriKind.Absolute,
            out var uri))
    {
        var userInfo = uri.UserInfo.Split(':', 2);

        var user = Uri.UnescapeDataString(userInfo[0]);

        var password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : "";

        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return
            $"Host={Quote(uri.Host)};" +
            $"Port={port};" +
            $"Database={Quote(database)};" +
            $"Username={Quote(user)};" +
            $"Password={Quote(password)};" +
            "SSL Mode=Prefer;Trust Server Certificate=true";
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
        return
            $"Host={Quote(host)};" +
            $"Port={(string.IsNullOrWhiteSpace(pgPort) ? "5432" : pgPort)};" +
            $"Database={Quote(db)};" +
            $"Username={Quote(pgUser)};" +
            $"Password={Quote(pgPassword ?? "")};" +
            "SSL Mode=Prefer;Trust Server Certificate=true";
    }

    return configuration.GetConnectionString("DefaultConnection");
}

var connectionString =
    ResolveConnectionString(builder.Configuration);

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQL configuration is missing. " +
        "Configure DATABASE_URL, PostgreSQL variables, " +
        "or ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<RestaurantDB>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// CORS
// ==========================================

var frontendOrigins =
    (Environment.GetEnvironmentVariable("FRONTEND_ORIGINS") ?? "")
    .Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries)
    .ToList();

if (builder.Environment.IsDevelopment())
{
    frontendOrigins.Add("http://localhost:5173");
    frontendOrigins.Add("http://localhost:5174");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestaurantOS", policy =>
    {
        if (frontendOrigins.Count > 0)
        {
            policy
                .WithOrigins(frontendOrigins.Distinct().ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// ==========================================
// CONTROLLERS & SWAGGER
// ==========================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT bearer token."
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

// ASP.NET CORE IDENTITY

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<RestaurantDB>()
    .AddDefaultTokenProviders();

// JWT AUTHENTICATION

var jwtKey = builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"] ?? "RestaurantAPI";

var jwtAudience =
    builder.Configuration["Jwt:Audience"] ?? "RestaurantClient";

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey =
            "DevOnly_RestaurantApi_JwtKey_Change_Me_1234567890!";
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:Key is missing or shorter than 32 bytes. " +
            "Set Jwt__Key in environment variables.");
    }
}

// Ensure AuthService and JWT validation use
// the same resolved configuration values.
builder.Configuration["Jwt:Key"] = jwtKey;
builder.Configuration["Jwt:Issuer"] = jwtIssuer;
builder.Configuration["Jwt:Audience"] = jwtAudience;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                RoleClaimType = ClaimTypes.Role
            };
    });


builder.Services.AddAutoMapper(
    cfg => { },
    AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddScoped(
    typeof(IBaseRepository<>),
    typeof(BaseRepository<>));

builder.Services.AddScoped<
    IBasketRepository,
    BasketRepository>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IReservationRepository,
    ReservationRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();



builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    IBasketService,
    BasketService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    ITableService,
    TableService>();

builder.Services.AddScoped<
    IReservationService,
    ReservationService>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();

builder.Services.AddScoped<
    IRestaurantService,
    RestaurantService>();

builder.Services.AddScoped<
    ICurrentRestaurantService,
    CurrentRestaurantService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext =
        services.GetRequiredService<RestaurantDB>();

    await dbContext.Database.MigrateAsync();

    await DataSeeder.SeedRolesAndAdminAsync(services);
}


app.UseSwagger();
app.UseSwaggerUI();



app.UseCors("RestaurantOS");

app.UseAuthentication();
app.UseAuthorization();



app.MapGet(
    "/",
    () => Results.Redirect("/swagger"));

app.MapGet(
    "/health",
    () => Results.Ok(new { status = "ok" }));

app.MapControllers();


app.Run();
