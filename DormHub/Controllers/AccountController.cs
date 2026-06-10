using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DormHub.Data;
using DormHub.Models;
using DormHub.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                PasswordHash = PasswordHasher.Hash(model.Password),
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
                ModelState.AddModelError(string.Empty, "Nieprawidlowy login lub haslo.");
                return View(model);
            }

            if (!PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Nieprawidlowy login lub haslo.");
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _db.Persons.FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                Person = user
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _db.Persons.FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var profileVm = new ProfileViewModel { Person = user, ChangeEmail = model };
                return View("Profile", profileVm);
            }

            if (!PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.Password), "Nieprawidłowe hasło.");
                var profileVm = new ProfileViewModel { Person = user, ChangeEmail = model };
                return View("Profile", profileVm);
            }

            var trimmedEmail = model.NewEmail.Trim().ToLower();
            if (trimmedEmail != user.Email.ToLower())
            {
                var exists = await _db.Persons.AnyAsync(p => p.Email.ToLower() == trimmedEmail);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.NewEmail), "Ten adres e-mail jest już zajęty.");
                    var profileVm = new ProfileViewModel { Person = user, ChangeEmail = model };
                    return View("Profile", profileVm);
                }

                user.Email = model.NewEmail.Trim();
                await _db.SaveChangesAsync();

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
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                TempData["SuccessMessage"] = "Adres e-mail został pomyślnie zmieniony.";
            }
            else
            {
                TempData["SuccessMessage"] = "Wprowadzony e-mail jest taki sam jak aktualny.";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _db.Persons.FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var profileVm = new ProfileViewModel { Person = user, ChangePassword = model };
                return View("Profile", profileVm);
            }

            if (!PasswordHasher.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Nieprawidłowe aktualne hasło.");
                var profileVm = new ProfileViewModel { Person = user, ChangePassword = model };
                return View("Profile", profileVm);
            }

            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hasło zostało pomyślnie zmienione.";
            return RedirectToAction("Profile");
        }
    }
}