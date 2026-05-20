using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DormHub.Data;
using DormHub.Models;

namespace DormHub.Controllers
{
    [Authorize]
    [Route("usterki")]
    public class FaultModelsController : Controller
    {
        private readonly DormDbContext _context;

        public FaultModelsController(DormDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdminOrStaff() =>
            User.IsInRole("Admin") || User.IsInRole("Staff");

        [Route("")]
        public async Task<IActionResult> Index()
        {
            IQueryable<FaultModel> query = _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category);

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FindAsync(CurrentUserId());
                if (resident?.RoomId == null)
                    return View(new List<FaultModel>());
                query = query.Where(f => f.RoomId == resident.RoomId);
            }

            return View(await query.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var fault = await _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fault == null) return NotFound();

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FindAsync(CurrentUserId());
                if (resident?.RoomId != fault.RoomId) return Forbid();
            }

            return View(fault);
        }

        [HttpGet("dodaj")]
        public async Task<IActionResult> Create()
        {
            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FindAsync(CurrentUserId());
                if (resident?.RoomId == null)
                {
                    TempData["Error"] = "Nie mozesz zglosic usterki – nie jestes przypisany do pokoju.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["PriorityId"] = new SelectList(_context.FaultPriorities, "Id", "Name", 2);
            ViewData["CategoryId"] = new SelectList(_context.FaultCategories,  "Id", "Name", 6);

            if (IsAdminOrStaff())
            {
                ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber");
                ViewData["ReportedById"] = new SelectList(
                    _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                    "Id", "FullName");
            }

            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,ReportedById,Description,PriorityId,CategoryId")] FaultModel faultModel)
        {
            ModelState.Remove("Id");
            ModelState.Remove("ReportedAt");

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FindAsync(CurrentUserId());
                if (resident == null || resident.RoomId == null) return Forbid();
                faultModel.RoomId       = resident.RoomId.Value;
                faultModel.ReportedById = resident.Id;
                ModelState.Remove("RoomId");
                ModelState.Remove("ReportedById");
            }

            faultModel.ReportedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(faultModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PriorityId"] = new SelectList(_context.FaultPriorities, "Id", "Name", faultModel.PriorityId);
            ViewData["CategoryId"] = new SelectList(_context.FaultCategories,  "Id", "Name", faultModel.CategoryId);
            if (IsAdminOrStaff())
            {
                ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", faultModel.RoomId);
                ViewData["ReportedById"] = new SelectList(
                    _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                    "Id", "FullName", faultModel.ReportedById);
            }
            return View(faultModel);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var fault = await _context.Faults.FindAsync(id);
            if (fault == null) return NotFound();
            ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", fault.RoomId);
            ViewData["ReportedById"] = new SelectList(
                _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                "Id", "FullName", fault.ReportedById);
            ViewData["PriorityId"]   = new SelectList(_context.FaultPriorities, "Id", "Name", fault.PriorityId);
            ViewData["CategoryId"]   = new SelectList(_context.FaultCategories,  "Id", "Name", fault.CategoryId);
            ViewData["ResolvedById"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", fault.ResolvedById);
            return View(fault);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,ReportedById,Description,ReportedAt,PriorityId,CategoryId,IsResolved,ResolvedAt,ResolutionNotes,ResolvedById")] FaultModel faultModel)
        {
            if (id != faultModel.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faultModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Faults.Any(e => e.Id == faultModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", faultModel.RoomId);
            ViewData["ReportedById"] = new SelectList(
                _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                "Id", "FullName", faultModel.ReportedById);
            ViewData["PriorityId"]   = new SelectList(_context.FaultPriorities, "Id", "Name", faultModel.PriorityId);
            ViewData["CategoryId"]   = new SelectList(_context.FaultCategories,  "Id", "Name", faultModel.CategoryId);
            ViewData["ResolvedById"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", faultModel.ResolvedById);
            return View(faultModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var fault = await _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fault == null) return NotFound();
            return View(fault);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fault = await _context.Faults.FindAsync(id);
            if (fault != null) _context.Faults.Remove(fault);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
