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
    public class FaultModelsController : Controller
    {
        private readonly DormDbContext _context;

        public FaultModelsController(DormDbContext context)
        {
            _context = context;
        }

        // GET: FaultModels
        public async Task<IActionResult> Index()
        {
            var dormDbContext = _context.Faults.Include(f => f.ReportedBy).Include(f => f.Room);
            return View(await dormDbContext.ToListAsync());
        }

        // GET: FaultModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faultModel = await _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (faultModel == null)
            {
                return NotFound();
            }

            return View(faultModel);
        }

        // GET: FaultModels/Create
        public IActionResult Create()
        {
            ViewData["ReportedById"] = new SelectList(_context.Residents, "Id", "Discriminator");
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id");
            return View();
        }

        // POST: FaultModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,ReportedById,Description,ReportedAt,IsResolved,ResolvedAt")] FaultModel faultModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faultModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReportedById"] = new SelectList(_context.Residents, "Id", "Discriminator", faultModel.ReportedById);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", faultModel.RoomId);
            return View(faultModel);
        }

        // GET: FaultModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faultModel = await _context.Faults.FindAsync(id);
            if (faultModel == null)
            {
                return NotFound();
            }
            ViewData["ReportedById"] = new SelectList(_context.Residents, "Id", "Discriminator", faultModel.ReportedById);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", faultModel.RoomId);
            return View(faultModel);
        }

        // POST: FaultModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,ReportedById,Description,ReportedAt,IsResolved,ResolvedAt")] FaultModel faultModel)
        {
            if (id != faultModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faultModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaultModelExists(faultModel.Id))
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
            ViewData["ReportedById"] = new SelectList(_context.Residents, "Id", "Discriminator", faultModel.ReportedById);
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", faultModel.RoomId);
            return View(faultModel);
        }

        // GET: FaultModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faultModel = await _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (faultModel == null)
            {
                return NotFound();
            }

            return View(faultModel);
        }

        // POST: FaultModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faultModel = await _context.Faults.FindAsync(id);
            if (faultModel != null)
            {
                _context.Faults.Remove(faultModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FaultModelExists(int id)
        {
            return _context.Faults.Any(e => e.Id == id);
        }
    }
}
