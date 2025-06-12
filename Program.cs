//using FinalProject.MVC.DataAccess; // <-- AppDbContext olan namespace
//using Microsoft.EntityFrameworkCore;
//using FinalProject.MVC.Models;
//using Microsoft.AspNetCore.Identity;
//using FinalProject.MVC.Services.Abstracts;
//using FinalProject.MVC.Services.Implemets;
//using FinalProject.MVC.Helpers;
//using Microsoft.Extensions.Options;
////Web App yaradırıq
//var builder = WebApplication.CreateBuilder(args);

//// AppDbContext servisini qeyd et
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// MVC üçün kontrollerləri əlavə et
//builder.Services.AddControllersWithViews();
//builder.Services.AddIdentity<User, IdentityRole>(opt =>
//{
//    opt.User.RequireUniqueEmail = true;
//    opt.SignIn.RequireConfirmedEmail = true;
//    opt.Password.RequiredLength = 3;
//    opt.Password.RequireDigit = false;
//    opt.Password.RequireLowercase = false;
//    opt.Password.RequireUppercase = false;
//    opt.Password.RequireNonAlphanumeric = false;
//    opt.Lockout.MaxFailedAccessAttempts = 3;
//    //opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(int.MaxValue);
//}).AddDefaultTokenProviders().AddEntityFrameworkStores<AppDbContext>();
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<ISearchService, SearchService>();
//builder.Services.AddMemoryCache();
//var opt = new SmtpOptions();
//builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.Name));
////builder.Services.AddSession();
//builder.Services.ConfigureApplicationCookie(x =>
//{
//    x.LoginPath = "/login";
//    x.LogoutPath = "/Account/Logout";
//    x.AccessDeniedPath = "/Home/AccessDenied";
//});
////Web appi build edirik
//var app = builder.Build();

//// Middleware-lər
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();
////app.UseUserSeed();
////app.UseSession();
//app.MapControllerRoute(
//    name: "login",
//    pattern: "login", new
//    {
//        Controller = "Account",
//        Action = "Login"
//    });
//app.MapControllerRoute(
//    name: "register",
//    pattern: "register", new
//    {
//        Controller = "Account",
//        Action = "Register"
//    });
//// Routing
//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.MapAreaControllerRoute(
//    name: "Admin",
//    areaName: "Admin",
//    pattern: "Admin/{controller=Product}/{action=Index}/{id?}");
//app.MapAreaControllerRoute(
//    name: "Admin",
//    areaName: "Admin",
//    pattern: "Admin/{controller=AdminReservation}/{action=PendingReservations}/{id?}");
//app.MapAreaControllerRoute(
//    name: "Admin",
//    areaName: "Admin",
//    pattern: "Admin/{controller=Room}/{action=Index}/{id?}");
//app.Run();
using FinalProject.MVC.DataAccess;
using Microsoft.EntityFrameworkCore;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity;
using FinalProject.MVC.Services.Abstracts;
using FinalProject.MVC.Services.Implemets;
using FinalProject.MVC.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// AppDbContext servisini qeyd et
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC üçün kontrollerləri əlavə et
builder.Services.AddControllersWithViews();

// Add Authentication before Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/GoogleLogin";
});

// Identity configuration
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;
    opt.Password.RequiredLength = 3;
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Lockout.MaxFailedAccessAttempts = 3;
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<AppDbContext>();

// Other services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddMemoryCache();
var opt = new SmtpOptions();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.Name));

var app = builder.Build();

// Middleware-lər
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Make sure Authentication comes before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { Controller = "Account", Action = "Login" });

app.MapControllerRoute(
    name: "register",
    pattern: "register",
    defaults: new { Controller = "Account", Action = "Register" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Product}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=AdminReservation}/{action=PendingReservations}/{id?}");

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Room}/{action=Index}/{id?}");

app.Run();