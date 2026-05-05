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
    [Route("rodzaj_pokoju")]
    public class RoomTypeModelsController : Controller
    {
        private readonly DormDbContext _context;

        public RoomTypeModelsController(DormDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.RoomTypes.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomTypeModel = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomTypeModel == null)
            {
                return NotFound();
            }

            return View(roomTypeModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RoomTypeModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Capacity")] RoomTypeModel roomTypeModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomTypeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roomTypeModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomTypeModel = await _context.RoomTypes.FindAsync(id);
            if (roomTypeModel == null)
            {
                return NotFound();
            }
            return View(roomTypeModel);
        }

        // POST: RoomTypeModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Capacity")] RoomTypeModel roomTypeModel)
        {
            if (id != roomTypeModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomTypeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeModelExists(roomTypeModel.Id))
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
            return View(roomTypeModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomTypeModel = await _context.RoomTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomTypeModel == null)
            {
                return NotFound();
            }

            return View(roomTypeModel);
        }

        // POST: RoomTypeModels/Delete/5
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomTypeModel = await _context.RoomTypes.FindAsync(id);
            if (roomTypeModel != null)
            {
                _context.RoomTypes.Remove(roomTypeModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeModelExists(int id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }
    }
}
