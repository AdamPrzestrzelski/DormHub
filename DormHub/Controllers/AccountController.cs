using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DormHub.Services;

namespace DormHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly DormDbContext _db;

        public AccountController(DormDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet]
        public IActionResult Register()
        {
            var vm = new RegisterViewModel
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-18))
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _db.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Email.ToLower() == model.Email.Trim().ToLower());

            if (existing != null)
            {
               ModelState.AddModelError(nameof(model.Email), "Konto z tym adresem email już istnieje");
               return View(model);
            }

            var person = new PersonModel
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                DateOfBirth = model.DateOfBirth,
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                PasswordHash = CreatePasswordHash(model.Password),
                Role = "User",
                IsActive = true
            };

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Persons.FirstOrDefaultAsync(p => p.Email == model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owy login lub has³o.");
                return View(model);
            }

            if (!PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Nieprawid³owy login lub has³o.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (!string.IsNullOrEmpty(user.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            claims.Add(new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Optional: endpoint to create a test user (development only)
        [HttpPost]
        public async Task<IActionResult> CreateTestUser(string email, string password, string role = "User")
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return BadRequest();

            var exists = await _db.Persons.AnyAsync(p => p.Email == email);
            if (exists) return Conflict();

            var person = new PersonModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                PhoneNumber = "",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)),
                PasswordHash = PasswordHasher.Hash(password),
                Role = role,
                IsActive = true
            };

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return Ok();
        }

        private static string CreatePasswordHash(string password)
        {
            const int saltSize = 16;
            const int iterations = 100_000;
            const int hashSize = 32;

            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[saltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(hashSize);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
    }
}