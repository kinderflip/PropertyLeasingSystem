using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingMVC.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddHttpClient("PropertyAPI", client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7121/";
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Seed manager + staff + tenant demo users on startup.
// Wrapped in try/catch so a temporarily unavailable DB (Azure SQL Serverless can
// auto-pause after idle time) doesn't crash the whole app on first boot.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string adminEmail = "manager@property.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Ahmed Faisal",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Manager123");
            await userManager.AddToRoleAsync(adminUser, "PropertyManager");
        }

        string staffEmail = "staff@property.com";
        if (await userManager.FindByEmailAsync(staffEmail) == null)
        {
            var staffUser = new ApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                FullName = "Bader Hamed",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(staffUser, "Staff123");
            await userManager.AddToRoleAsync(staffUser, "MaintenanceStaff");
        }

        string tenantEmail = "tenant@property.com";
        if (await userManager.FindByEmailAsync(tenantEmail) == null)
        {
            var tenantUser = new ApplicationUser
            {
                UserName = tenantEmail,
                Email = tenantEmail,
                FullName = "Ahmed Hassan",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(tenantUser, "Tenant123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(tenantUser, "Tenant");

                // Link the demo login to an existing tenant profile so MyLeases /
                // MyPayments / My Requests and the ownership checks resolve to real data.
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var profile = await dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId == 1);
                if (profile != null && profile.UserId == null)
                {
                    profile.UserId = tenantUser.Id;
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("User seeding skipped: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<MaintenanceHub>("/maintenanceHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
