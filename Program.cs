using EntityFrameworkCore.UseRowNumberForPaging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebMVC.BackgroundWorkers;
using WebMVC.Data;
using WebMVC.Hubs;
using WebMVC.Interfaces;
using WebMVC.Mappers;
using WebMVC.Middlewares;
using WebMVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), builder => builder.UseRowNumberForPaging().UseCompatibilityLevel(100)));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHttpContextService, HttpContextService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOutOfStockService, OutOfStockService>();
builder.Services.AddScoped<ITrackingService, TrackingService>();
builder.Services.AddScoped<ITransportationService, TransportationService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IBigPackageService, BigPackageService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IPricingImportService, PricingImportService>();
builder.Services.AddScoped<ISendEmailService, SendEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<IWebConfigurationService, WebConfigurationService>();
builder.Services.AddScoped<IZaloAPIService, ZaloAPIService>();
builder.Services.AddScoped<IStaffTargetService, StaffTargetService>();

builder.Services.AddScoped<IAppApiService, AppApiService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddSingleton<UserConnectionManager>();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.ClearProviders(); 
builder.Logging.AddSerilog();    
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);  
builder.Logging.AddFilter("System", LogLevel.Warning);

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/dang-nhap";
    options.AccessDeniedPath = "/dang-nhap";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var app = builder.Build();
Log.Information("Application Starting");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();


