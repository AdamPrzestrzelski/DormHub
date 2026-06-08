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
    [Route("wnioski")]
    public class ApplicationModelsController : Controller
    {
        private readonly DormDbContext _context;

        public ApplicationModelsController(DormDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
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
            ViewData["ApplicantId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName");
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApplicantId,PreferredRoomTypeId,PreferredBuildingId")] ApplicationModel applicationModel)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                applicationModel.Id = Guid.NewGuid().ToString();
                applicationModel.SubmittedAt = DateTime.Now;
                applicationModel.StatusId = 1;
                _context.Add(applicationModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicantId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", applicationModel.ApplicantId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            return View(applicationModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var applicationModel = await _context.Applications.FindAsync(id);
            if (applicationModel == null) return NotFound();
            ViewData["ApplicantId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", applicationModel.ApplicantId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            ViewData["StatusId"] = new SelectList(_context.ApplicationStatuses, "Id", "Name", applicationModel.StatusId);
            return View(applicationModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,ApplicantId,PreferredRoomTypeId,PreferredBuildingId,StatusId")] ApplicationModel applicationModel)
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
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name", applicationModel.PreferredBuildingId);
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

        [HttpPost("usun/{id}"), ActionName("Delete")]
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

        [HttpPost("zatwierdz/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(string id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();
            app.StatusId = 2;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("odrzuc/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();
            app.StatusId = 3;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
