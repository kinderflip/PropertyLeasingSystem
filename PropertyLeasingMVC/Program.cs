using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingMVC.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
// Add DbContext - same database as API
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

// Seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create PropertyManager user if not exists
    string adminEmail = "manager@property.com";
    string adminPassword = "Manager123";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Property Manager",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, "PropertyManager");
    }

    // Create MaintenanceStaff user if not exists
    string staffEmail = "staff@property.com";
    string staffPassword = "Staff123";

    if (await userManager.FindByEmailAsync(staffEmail) == null)
    {
        var staffUser = new ApplicationUser
        {
            UserName = staffEmail,
            Email = staffEmail,
            FullName = "Maintenance Staff",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(staffUser, staffPassword);
        await userManager.AddToRoleAsync(staffUser, "MaintenanceStaff");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();   
app.UseAuthorization();
app.MapHub<MaintenanceHub>("/maintenanceHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();