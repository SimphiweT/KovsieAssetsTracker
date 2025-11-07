using KovsieAssetsTracker.WebAppMVC.Helpers;
using KovsieAssetsTracker.WebAppMVC.Services.IRepositories;
using KovsieAssetsTracker.WebAppMVC.Services.Repositories;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.Configure<ApiBehaviorOptions>(options =>
//{
//    options.SuppressInferBindingSourcesForParameters = false;
//});

//builder.Services.AddControllers()
//    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);


// For session (JWT token tracking later)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // auto logout after 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Services for dependency injection
builder.Services.AddHttpContextAccessor(); // for session access
builder.Services.AddScoped<SessionManager>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IAssetService,AssetService>();
builder.Services.AddScoped<ICategoryService,CategoryService>();
builder.Services.AddScoped<IRoomService,RoomService>();
builder.Services.AddScoped<IFloorService,FloorService>();
builder.Services.AddScoped<IBuildingService,BuildingService>();
builder.Services.AddScoped<ICampusService,CampusService>();

// Configure HttpClient for services with SSL bypass (for development only)
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<IUserService, UserService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<IAccountService, AccountService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<IAssetService, AssetService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<ICategoryService, CategoryService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<IRoomService, RoomService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<IFloorService, FloorService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<IBuildingService, BuildingService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });

builder.Services.AddHttpClient<ICampusService, CampusService>()
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) => true
        };
    });


// Global exception handling for unobserved task exceptions
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Console.WriteLine("Unhandled Exception: " + args.ExceptionObject);
};
TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    Console.WriteLine("Unobserved Task Exception: " + args.Exception.Message);
};

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseHttpMethodOverride();
app.UseRouting();
app.UseSession();   
app.UseAuthorization();

// Default route to Home controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
