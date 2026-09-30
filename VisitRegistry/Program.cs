using System.Globalization;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.ModelBinders;
using VisitRegistry.Models;
using VisitRegistry.Services;

var builder = WebApplication.CreateBuilder(args);

const string applicationUrl = "http://127.0.0.1:5080";
var applicationDataDirectory = Environment.GetEnvironmentVariable("VISITREGISTRY_DATA_DIR");
if (string.IsNullOrWhiteSpace(applicationDataDirectory))
{
    applicationDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VisitRegistry");
}
var keysDirectory = Path.Combine(applicationDataDirectory, "Keys");
Directory.CreateDirectory(applicationDataDirectory);
Directory.CreateDirectory(keysDirectory);

builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Data Source={Path.Combine(applicationDataDirectory, "VisitRegistry.db")}";

if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
    builder.WebHost.UseUrls(applicationUrl);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var persianCulture = new CultureInfo("fa-IR")
{
    DateTimeFormat = { Calendar = new PersianCalendar() },
    NumberFormat = { NativeDigits = ["۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹"] }
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("fa-IR");
    options.SupportedCultures = [persianCulture, CultureInfo.GetCultureInfo("en-US")];
    options.SupportedUICultures = [persianCulture, CultureInfo.GetCultureInfo("en-US")];
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("VisitRegistry");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<VisitService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<PersianDateService>();

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new PersianTimeSpanModelBinderProvider());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.InitializeAsync(services, app.Configuration);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (args.Contains("--open-browser", StringComparer.OrdinalIgnoreCase))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            Process.Start(new ProcessStartInfo(applicationUrl) { UseShellExecute = true });
        }
        catch
        {
            // The server remains available even if Windows cannot launch the default browser.
        }
    });
}

await app.RunAsync();
