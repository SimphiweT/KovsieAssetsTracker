using KovsieAssetsTracker.Application.Helpers;
using KovsieAssetsTracker.Application.Services;
using KovsieAssetsTracker.Domain;
using KovsieAssetsTracker.Domain.Entities;
using KovsieAssetsTracker.Domain.IRepositories;
using KovsieAssetsTracker.Domain.IRepositories.Base;
using KovsieAssetsTracker.Infrastructure.Data;
using KovsieAssetsTracker.Infrastructure.EF_Repositories;
using KovsieAssetsTracker.Infrastructure.EF_Repositories.Base;
using KovsieAssetsTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Kestrel & Limits --------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5250);
    options.Limits.MaxRequestBodySize = 100_000_000; // 100 MB
});

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// -------------------- Swagger / OpenAPI --------------------
var xmlFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kovsie Assets Tracker - API",
        Version = "v1",
        Description = "Full-stack Asset Manager System for Kovsie supporting QR tracking, user roles, and reporting.",
        TermsOfService = new Uri("https://github.com/SimphiweT/KovsieAssetTrackerUFS"),
        Contact = new OpenApiContact
        {
            Name = "Simphiwe Terrence",
            Email = "2016401495@ufs4life.ac.za",
            Url = new Uri("https://github.com/SimphiweT")
        },
        License = new OpenApiLicense { Name = "MIT License", Url = new Uri("https://opensource.org/licenses/MIT") }
    });

    if (File.Exists(xmlFilePath))
        c.IncludeXmlComments(xmlFilePath);

    c.UseAllOfToExtendReferenceSchemas();
    c.CustomSchemaIds(x => x.FullName);

    // JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// -------------------- Controllers --------------------
builder.Services.AddControllers();

// -------------------- DbContext --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Identity --------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -------------------- JWT Authentication --------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

// -------------------- Authorization --------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole(ConstantsUtils.ADMIN_ROLE));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole(ConstantsUtils.STAFF_ROLE));
    options.AddPolicy("RequireAgentRole", policy => policy.RequireRole(ConstantsUtils.AGENT_ROLE));
});

// -------------------- DI --------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// -------------------- Form options --------------------
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 100_000_000; // 100 MB
});

// -------------------- Build App --------------------
var app = builder.Build();

// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// -------------------- Global exception logging --------------------
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine($"[Unhandled Exception] {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine($"[Unobserved Task Exception] {e.Exception}");
};

// -------------------- Seed Database --------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Call the improved SeedAsync method
        await SeedData.SeedAsync(services, maxRetries: 3);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database seeding failed after retries: {ex.Message}");
        throw; // stop app if seeding fails
    }
}

// -------------------- Run App --------------------
app.Run();
