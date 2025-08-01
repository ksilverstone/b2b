using b2b.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class AuthController : Controller
{
    private readonly B2BContext _context;
    public AuthController(B2BContext context)
    {
        _context = context;
    }

    // Login GET
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Login POST
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "Email adresi zorunludur.");
            return View();
        }

        if (string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Şifre zorunludur.");
            return View();
        }

        var user = await _context.Users
            .Include(u => u.Company)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("", "Kullanıcı bulunamadı.");
            return View();
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Hesabınız aktif değil. Lütfen yöneticinizle iletişime geçin.");
            return View();
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
        {
            ModelState.AddModelError("", "Şifre yanlış.");
            return View();
        }

        // Claims
        var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email),
    new Claim("FullName", user.FullName ?? ""),
    new Claim("UserId", user.Id.ToString()),
    new Claim("CompanyId", user.CompanyId.ToString()),
    new Claim("CompanyName", user.Company != null ? user.Company.Name : ""),
    new Claim("IsSeller", user.Company != null ? user.Company.IsSeller.ToString() : "false"),
    new Claim("IsBuyer", user.Company != null ? user.Company.IsBuyer.ToString() : "true")
};

        foreach (var userRole in user.UserRoles ?? new List<UserRole>())
        {
            if (userRole.Role != null)
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    // Register GET
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Register POST
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("", "Bu email adresi zaten kullanılıyor.");
            return View(model);
        }

        // Check if company name already exists
        if (await _context.Companies.AnyAsync(c => c.Name == model.CompanyName))
        {
            ModelState.AddModelError("", "Bu şirket adı zaten kullanılıyor.");
            return View(model);
        }

        var hasher = new PasswordHasher<User>();
        var tempUser = new User { Email = model.Email };
        var passwordHash = hasher.HashPassword(tempUser, model.Password);

        // Create company first
        var company = new Company
        {
            Name = model.CompanyName,
            Address = model.CompanyAddress,
            Phone = model.CompanyPhone,
            IsSeller = false,
            IsBuyer = true
        };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        // Create user with company
        var user = new User
        {
            Email = model.Email,
            PasswordHash = passwordHash,
            FullName = model.FullName,
            CompanyId = company.Id,
            IsActive = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Add default role (Satıcı ve Admin)
        var sellerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Satıcı");
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (sellerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = sellerRole.Id
            };
            await _context.UserRoles.AddAsync(userRole);
        }
        if (adminRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            };
            await _context.UserRoles.AddAsync(userRole);
        }
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    // ForgotPassword GET
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // ForgotPassword POST
    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            ViewBag.Success = "Şifre sıfırlama linki email adresinize gönderildi.";
            return View();
        }
        else
        {
            ViewBag.Error = "Bu email adresi sistemde kayıtlı değil.";
            return View();
        }
    }

    // LockScreen GET
    [HttpGet]
    public IActionResult LockScreen(string? message)
    {
        if (!string.IsNullOrEmpty(message))
            ViewBag.Info = message;
        return View();
    }
}