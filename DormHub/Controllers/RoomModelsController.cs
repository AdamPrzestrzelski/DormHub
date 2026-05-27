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
    [Route("pokoje")]
    public class RoomModelsController : Controller
    {
        private readonly DormDbContext _context;

        public RoomModelsController(DormDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var rooms = _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status);
            return View(await rooms.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var roomModel = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomModel == null) return NotFound();
            return View(roomModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
            ViewData["TypeId"]     = new SelectList(_context.RoomTypes,  "Id", "Name");
            ViewData["StatusId"]   = new SelectList(_context.RoomStatuses, "Id", "Name");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomNumber,BuildingId,TypeId")] RoomModel roomModel)
        {
            if (ModelState.IsValid)
            {
                roomModel.StatusId = 1;
                roomModel.Floor = roomModel.RoomNumber / 100;
                _context.Add(roomModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings,   "Id", "Name", roomModel.BuildingId);
            ViewData["TypeId"]     = new SelectList(_context.RoomTypes,   "Id", "Name", roomModel.TypeId);
            ViewData["StatusId"]   = new SelectList(_context.RoomStatuses,"Id", "Name", roomModel.StatusId);
            return View(roomModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var roomModel = await _context.Rooms.FindAsync(id);
            if (roomModel == null) return NotFound();
            ViewData["BuildingId"] = new SelectList(_context.Buildings,   "Id", "Name", roomModel.BuildingId);
            ViewData["TypeId"]     = new SelectList(_context.RoomTypes,   "Id", "Name", roomModel.TypeId);
            ViewData["StatusId"]   = new SelectList(_context.RoomStatuses,"Id", "Name", roomModel.StatusId);
            return View(roomModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,BuildingId,TypeId,StatusId")] RoomModel roomModel)
        {
            if (id != roomModel.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    roomModel.Floor = roomModel.RoomNumber / 100;
                    _context.Update(roomModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == roomModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings,   "Id", "Name", roomModel.BuildingId);
            ViewData["TypeId"]     = new SelectList(_context.RoomTypes,   "Id", "Name", roomModel.TypeId);
            ViewData["StatusId"]   = new SelectList(_context.RoomStatuses,"Id", "Name", roomModel.StatusId);
            return View(roomModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var roomModel = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomModel == null) return NotFound();
            return View(roomModel);
        }

        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomModel = await _context.Rooms.FindAsync(id);
            if (roomModel != null) _context.Rooms.Remove(roomModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
