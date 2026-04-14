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
    [Route("wnioski")]
    public class ApplicationModelsController : Controller
    {
        private readonly DormDbContext _context;

        public ApplicationModelsController(DormDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var dormDbContext = _context.Applications.Include(a => a.Applicant).Include(a => a.PreferredBuilding).Include(a => a.PreferredRoomType);
            return View(await dormDbContext.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationModel = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.PreferredBuilding)
                .Include(a => a.PreferredRoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationModel == null)
            {
                return NotFound();
            }

            return View(applicationModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["ApplicantId"] = new SelectList(_context.Persons, "Id", "Discriminator");
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Address");
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name");
            return View();
        }

        // POST: ApplicationModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ApplicantId,PreferredRoomTypeId,PreferredBuildingId")] ApplicationModel applicationModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(applicationModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicantId"] = new SelectList(_context.Persons, "Id", "Discriminator", applicationModel.ApplicantId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Address", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            return View(applicationModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationModel = await _context.Applications.FindAsync(id);
            if (applicationModel == null)
            {
                return NotFound();
            }
            ViewData["ApplicantId"] = new SelectList(_context.Persons, "Id", "Discriminator", applicationModel.ApplicantId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Address", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            return View(applicationModel);
        }

        // POST: ApplicationModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,ApplicantId,PreferredRoomTypeId,PreferredBuildingId")] ApplicationModel applicationModel)
        {
            if (id != applicationModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicationModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationModelExists(applicationModel.Id))
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
            ViewData["ApplicantId"] = new SelectList(_context.Persons, "Id", "Discriminator", applicationModel.ApplicantId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Address", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            return View(applicationModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationModel = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.PreferredBuilding)
                .Include(a => a.PreferredRoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationModel == null)
            {
                return NotFound();
            }

            return View(applicationModel);
        }

        // POST: ApplicationModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationModel = await _context.Applications.FindAsync(id);
            if (applicationModel != null)
            {
                _context.Applications.Remove(applicationModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationModelExists(string id)
        {
            return _context.Applications.Any(e => e.Id == id);
        }
    }
}
