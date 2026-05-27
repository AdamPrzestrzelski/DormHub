using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DormHub.Data;
using DormHub.Models;

namespace DormHub.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("osoby")]
    public class PersonModelsController : Controller
    {
        private readonly DormDbContext _context;

        public PersonModelsController(DormDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Persons.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personModel = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personModel == null)
            {
                return NotFound();
            }

            return View(personModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth,Email,PhoneNumber,PasswordHash,Role")] PersonModel personModel)
        {
            if (ModelState.IsValid)
            {
                personModel.PasswordHash = DormHub.Services.PasswordHasher.Hash(personModel.PasswordHash);
                _context.Persons.Add(personModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(personModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personModel = await _context.Persons.FindAsync(id);
            if (personModel == null)
            {
                return NotFound();
            }
            return View(personModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,Email,PhoneNumber,Role")] PersonModel personModel, string? newPassword)
        {
            if (id != personModel.Id)
            {
                return NotFound();
            }

            ModelState.Remove("PasswordHash");
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPerson = await _context.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (existingPerson == null) return NotFound();

                    if (!string.IsNullOrEmpty(newPassword))
                        personModel.PasswordHash = DormHub.Services.PasswordHasher.Hash(newPassword);
                    else
                        personModel.PasswordHash = existingPerson.PasswordHash;

                    _context.Update(personModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonModelExists(personModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(personModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personModel = await _context.Persons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personModel == null)
            {
                return NotFound();
            }

            return View(personModel);
        }

        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personModel = await _context.Persons.FindAsync(id);
            if (personModel != null)
            {
                _context.Persons.Remove(personModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonModelExists(int id)
        {
            return _context.Persons.Any(e => e.Id == id);
        }
    }
}
