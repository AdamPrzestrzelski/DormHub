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

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var residents = _context.Residents.Include(r => r.Room);
            return View(await residents.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null) return NotFound();
            return View(residentModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            var existingResidentIds = _context.Residents.Select(r => r.Id).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingResidentIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();

            ViewData["PersonId"] = new SelectList(availablePersons, "Id", "FullName");
            ViewData["RoomId"]   = new SelectList(_context.Rooms, "Id", "RoomNumber");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int PersonId, int? RoomId, DateTime MoveInDate, DateTime? MoveOutDate)
        {
            var person = await _context.Persons.FindAsync(PersonId);
            if (person == null)
            {
                ModelState.AddModelError("PersonId", "Nie znaleziono wybranej osoby.");
                goto ReturnView;
            }

            if (await _context.Residents.AnyAsync(r => r.Id == PersonId))
            {
                ModelState.AddModelError("PersonId", "Ta osoba jest juz mieszkancem.");
                goto ReturnView;
            }

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE [Persons] SET [Discriminator] = 'ResidentModel', [RoomId] = {0}, [MoveInDate] = {1}, [MoveOutDate] = {2} WHERE [Id] = {3}",
                (object?)RoomId ?? DBNull.Value,
                MoveInDate,
                (object?)MoveOutDate ?? DBNull.Value,
                PersonId);

            if (RoomId.HasValue)
            {
                await UpdateRoomStatusAsync(RoomId.Value);
            }

            return RedirectToAction(nameof(Index));

            ReturnView:
            var existingResidentIds = _context.Residents.Select(r => r.Id).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingResidentIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();
            ViewData["PersonId"] = new SelectList(availablePersons, "Id", "FullName", PersonId);
            ViewData["RoomId"]   = new SelectList(_context.Rooms, "Id", "RoomNumber", RoomId);
            return View();
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents.FindAsync(id);
            if (residentModel == null) return NotFound();
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("RoomId,MoveInDate,MoveOutDate,Id,FirstName,LastName,DateOfBirth,Email,PhoneNumber")]
            ResidentModel residentModel,
            string? newPassword)
        {
            if (id != residentModel.Id) return NotFound();

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            ModelState.Remove("Discriminator");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Residents.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null) return NotFound();

                    residentModel.PasswordHash = !string.IsNullOrEmpty(newPassword)
                        ? DormHub.Services.PasswordHasher.Hash(newPassword)
                        : existing.PasswordHash;
                    residentModel.Role = existing.Role;

                    _context.Update(residentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Residents.Any(e => e.Id == residentModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null) return NotFound();
            return View(residentModel);
        }

        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var residentModel = await _context.Residents.FindAsync(id);
            int? roomId = residentModel?.RoomId;
            if (residentModel != null) _context.Residents.Remove(residentModel);
                await _context.SaveChangesAsync();
            if (roomId.HasValue) await UpdateRoomStatusAsync(roomId.Value);
            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateRoomStatusAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return;

            var currentCount = await _context.Residents.CountAsync(r => r.RoomId == roomId);
            var capacity = room.RoomType?.Capacity ?? 1;

            if (room.StatusId == 3) return; //remont

            if (currentCount == 0) room.StatusId = 1; //wolny
            else if (currentCount < capacity) room.StatusId = 4; //czescizajety
            else room.StatusId = 2; //zajety

            await _context.SaveChangesAsync();
        }
    }
}
