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
    [Route("mieszkancy")]
    public class ResidentModelsController : Controller
    {
        private readonly DormDbContext _context;

        public ResidentModelsController(DormDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var dormDbContext = _context.Residents.Include(r => r.Room);
            return View(await dormDbContext.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var residentModel = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null)
            {
                return NotFound();
            }

            return View(residentModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber");
            return View();
        }

        // POST: ResidentModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,MoveInDate,MoveOutDate,Id,FirstName,LastName,DateOfBirth,Email,PhoneNumber,PasswordHash")] ResidentModel residentModel)
        {
            residentModel.Role = "Resident";
            ModelState.Remove("Role");
            ModelState.Remove("Discriminator");
            if (ModelState.IsValid)
            {
                residentModel.PasswordHash = DormHub.Services.PasswordHasher.Hash(residentModel.PasswordHash);
                _context.Residents.Add(residentModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var residentModel = await _context.Residents.FindAsync(id);
            if (residentModel == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", residentModel.RoomId);
            return View(residentModel);
        }

        // POST: ResidentModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoomId,MoveInDate,MoveOutDate,Id,FirstName,LastName,DateOfBirth,Email,PhoneNumber")] ResidentModel residentModel, string? newPassword)
        {
            if (id != residentModel.Id)
            {
                return NotFound();
            }

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPerson = await _context.Residents.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (existingPerson == null) return NotFound();

                    if (!string.IsNullOrEmpty(newPassword))
                        residentModel.PasswordHash = DormHub.Services.PasswordHasher.Hash(newPassword);
                    else
                        residentModel.PasswordHash = existingPerson.PasswordHash;

                    residentModel.Role = existingPerson.Role;

                    _context.Update(residentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResidentModelExists(residentModel.Id))
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
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var residentModel = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null)
            {
                return NotFound();
            }

            return View(residentModel);
        }

        // POST: ResidentModels/Delete/5
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var residentModel = await _context.Residents.FindAsync(id);
            if (residentModel != null)
            {
                _context.Residents.Remove(residentModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResidentModelExists(int id)
        {
            return _context.Residents.Any(e => e.Id == id);
        }
    }
}
