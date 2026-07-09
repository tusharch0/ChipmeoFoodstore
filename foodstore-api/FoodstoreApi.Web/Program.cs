using FoodstoreApi.Core.Configuration;
using FoodstoreApi.Core.Entities.Identity;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Infrastructure.Extensions;
using FoodstoreApi.Usecase.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using FoodstoreApi.Web.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FoodstoreApi.Web.Hubs;
using Amazon.S3;
using Amazon.Runtime;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi;
using System.Text.Json;
using System.Text.Json.Serialization;
using FoodstoreApi.Web.Seed;
using FoodstoreApi.Web.Tenancy;
using FoodstoreApi.Usecase.Interfaces;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        options.Conventions.Add(new FoodstoreApi.Web.Conventions.V2RouteConvention());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Foodstore API", Version = "v2", Description = "API quản lý cửa hàng Foodstore v2 — RESTful với response envelope chuẩn" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<StoreDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

var cacheProvider = builder.Configuration["Cache:Provider"] ?? "Redis";

if (cacheProvider.Equals("Memory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Connection string 'Redis' not found.");
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "foodstore:";
    });
}

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var endpoint = config["S3:Endpoint"] ?? "http://rustfs:9000";
    if (!endpoint.StartsWith("http://") && !endpoint.StartsWith("https://"))
        endpoint = $"http://{endpoint}";
    var s3Config = new AmazonS3Config
    {
        ServiceURL = endpoint,
        ForcePathStyle = true,
        UseHttp = true
    };
    var credentials = new BasicAWSCredentials(
        config["S3:AccessKey"] ?? throw new InvalidOperationException("S3:AccessKey not configured"),
        config["S3:SecretKey"] ?? throw new InvalidOperationException("S3:SecretKey not configured"));
    return new AmazonS3Client(credentials, s3Config);
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<IntaSendOptions>(builder.Configuration.GetSection(IntaSendOptions.SectionName));
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<FoodstoreApi.Web.BackgroundServices.PaymentEventWorker>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings not configured");

// ASP.NET Core Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<StoreDbContext>()
.AddSignInManager<SignInManager<ApplicationUser>>()
.AddDefaultTokenProviders();

// JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options => 
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("GlobalPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 60; 
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });
});

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Auto-migrate and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
    await db.Database.ExecuteSqlRawAsync("CREATE COLLATION IF NOT EXISTS vi_ci_ai (LOCALE = 'vi-VN-x-icu', PROVIDER = 'icu');");
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Foodstore API v2");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("CorsPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AppHub>("/hubs/app");

app.Run();
