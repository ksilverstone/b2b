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

            // Giriş sayfası
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

            // Giriş işlemi
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

        // Kullanıcı bilgileri
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

            // Kayıt sayfası
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

            // Kayıt işlemi
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Email zaten var mı kontrol et
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("", "Bu email adresi zaten kullanılıyor.");
            return View(model);
        }

        // Şirket adı zaten var mı kontrol et
        if (await _context.Companies.AnyAsync(c => c.Name == model.CompanyName))
        {
            ModelState.AddModelError("", "Bu şirket adı zaten kullanılıyor.");
            return View(model);
        }

        var hasher = new PasswordHasher<User>();
        var tempUser = new User { Email = model.Email };
        var passwordHash = hasher.HashPassword(tempUser, model.Password);

        // Önce şirketi oluştur
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

        // Şirket ile kullanıcı oluştur
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

        // Varsayılan rol ekle (Alıcı)
        var buyerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Buyer");
        if (buyerRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = buyerRole.Id
            };
            await _context.UserRoles.AddAsync(userRole);
        }
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }

            // Şifre unutma sayfası
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

            // Şifre unutma işlemi
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

            // Ekran kilidi sayfası
    [HttpGet]
    public IActionResult LockScreen(string? message)
    {
        if (!string.IsNullOrEmpty(message))
            ViewBag.Info = message;
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Test için Seller rolü ekleme (sadece geliştirme ortamında)
    [HttpGet]
    public async Task<IActionResult> MakeSeller(string email)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user != null)
        {
            var sellerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Seller");
            if (sellerRole != null)
            {
                // Mevcut rolleri temizle
                var existingRoles = _context.UserRoles.Where(ur => ur.UserId == user.Id);
                _context.UserRoles.RemoveRange(existingRoles);

                // Seller rolü ekle
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = sellerRole.Id
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                return Content($"Kullanıcı {email} Seller rolüne atandı!");
            }
        }

        return Content("Kullanıcı bulunamadı veya Seller rolü bulunamadı!");
    }
}