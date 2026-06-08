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
            var residents = _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room);
            return View(await residents.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null) return NotFound();
            return View(residentModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            var existingPersonIds = _context.Residents.Select(r => r.PersonId).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingPersonIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();

            ViewData["PersonId"] = new SelectList(availablePersons, "Id", "FullName");
            ViewData["RoomId"]   = new SelectList(_context.Rooms, "Id", "RoomNumber");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PersonId,RoomId,MoveInDate,MoveOutDate")] ResidentModel residentModel)
        {
            if (await _context.Residents.AnyAsync(r => r.PersonId == residentModel.PersonId))
            {
                ModelState.AddModelError("PersonId", "Ta osoba jest już mieszkańcem.");
            }

            if (ModelState.IsValid)
            {
                _context.Residents.Add(residentModel);
                await _context.SaveChangesAsync();

                if (residentModel.RoomId.HasValue)
                {
                    await UpdateRoomStatusAsync(residentModel.RoomId.Value);
                }

                return RedirectToAction(nameof(Index));
            }

            var existingPersonIds = _context.Residents.Select(r => r.PersonId).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingPersonIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();

            ViewData["PersonId"] = new SelectList(availablePersons, "Id", "FullName", residentModel.PersonId);
            ViewData["RoomId"]   = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
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
            [Bind("Id,PersonId,RoomId,MoveInDate,MoveOutDate")] ResidentModel residentModel)
        {
            if (id != residentModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(residentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Residents.Any(e => e.Id == residentModel.Id)) return NotFound();
                    else throw;
                }

                if (residentModel.RoomId.HasValue)
                {
                    await UpdateRoomStatusAsync(residentModel.RoomId.Value);
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
                .Include(r => r.Person)
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
            room.StatusId = currentCount >= capacity ? 2 : 1;
            await _context.SaveChangesAsync();
        }
    }
}
