using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DormHub.Data;
using DormHub.Models;

namespace DormHub.Controllers
{
    [Route("pokoje")]
    public class RoomModelsController : Controller
    {
        private readonly DormDbContext _context;

        public RoomModelsController(DormDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var dormDbContext = _context.Rooms.Include(r => r.Building).Include(r => r.RoomType);
            return View(await dormDbContext.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomModel = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomModel == null)
            {
                return NotFound();
            }

            return View(roomModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Address");
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "Id", "Name");
            return View();
        }

        // POST: RoomModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomNumber,BuildingId,TypeId")] RoomModel roomModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Address", roomModel.BuildingId);
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", roomModel.TypeId);
            return View(roomModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomModel = await _context.Rooms.FindAsync(id);
            if (roomModel == null)
            {
                return NotFound();
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Address", roomModel.BuildingId);
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", roomModel.TypeId);
            return View(roomModel);
        }

        // POST: RoomModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,BuildingId,TypeId")] RoomModel roomModel)
        {
            if (id != roomModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomModelExists(roomModel.Id))
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
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Address", roomModel.BuildingId);
            ViewData["TypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", roomModel.TypeId);
            return View(roomModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomModel = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomModel == null)
            {
                return NotFound();
            }

            return View(roomModel);
        }

        // POST: RoomModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomModel = await _context.Rooms.FindAsync(id);
            if (roomModel != null)
            {
                _context.Rooms.Remove(roomModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomModelExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}
