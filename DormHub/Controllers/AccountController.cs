using DormHub.Data;
using DormHub.Models;
using DormHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly DormDbContext _db;

        public AccountController(DormDbContext db)
        {
            _db = db;
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
                ModelState.AddModelError(string.Empty, "Nieprawid這wy login lub has這.");
                return View(model);
            }

            if (!PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Nieprawid這wy login lub has這.");
                return View(model);
            }

            // Build claims
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

            // Example additional claim
            claims.Add(new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
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
            return View(); // optional view to show access denied message
        }

        // Optional: endpoint to create a test user (useful in development only)
        // NOTE: Remove or protect in production.
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
    }
}