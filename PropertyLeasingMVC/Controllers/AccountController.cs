using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PropertyLeasingAPI.Data;
using PropertyLeasingAPI.Models;
using PropertyLeasingAPI.Services;
using PropertyLeasingMVC.ViewModels;

namespace PropertyLeasingMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Account locked. Try again in 5 minutes.");
            else
                ModelState.AddModelError("", "Invalid email or password.");

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // C2 — block duplicates against the Tenants table BEFORE creating an Identity user,
            // so we never end up with an orphan ApplicationUser when Tenant insert fails.
            if (_context.Tenants.Any(t => t.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "A tenant with this email already exists.");
                return View(model);
            }
            if (_context.Tenants.Any(t => t.NationalId == model.NationalId))
            {
                ModelState.AddModelError(nameof(model.NationalId), "A tenant with this National ID already exists.");
                return View(model);
            }

            // L13: normalize phone once and re-use the canonical value everywhere we save it.
            var canonicalPhone = PhoneHelper.Normalize(model.Phone);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = canonicalPhone
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.Tenant);

                // C2 — every self-registered user gets a paired Tenant entity so the
                // Lease / MaintenanceRequest flows have something to bind to.
                var tenant = new Tenant
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = canonicalPhone,
                    NationalId = model.NationalId,
                    UserId = user.Id
                };

                try
                {
                    _context.Tenants.Add(tenant);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Roll back the Identity user so we don't leave an orphan account.
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError("", "Could not complete registration. Please try again.");
                    return View(model);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Welcome — your tenant account is ready.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}