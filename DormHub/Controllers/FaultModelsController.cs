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

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            IQueryable<FaultModel> query = _context.Faults
                .Include(f => f.ReportedBy)
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category);

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
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
                .Include(f => f.Photos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fault == null) return NotFound();

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident?.RoomId != fault.RoomId) return Forbid();
            }

            return View(fault);
        }

        [HttpGet("dodaj")]
        public async Task<IAct  ionResult> Create()
        {
            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
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
                    _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                    "Id", "FullName");
            }

            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,ReportedById,Description,PriorityId,CategoryId")] FaultModel faultModel, List<IFormFile>? photos)
        {
            ModelState.Remove("Id");
            ModelState.Remove("ReportedAt");

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident == null || resident.RoomId == null) return Forbid();
                faultModel.RoomId       = resident.RoomId.Value;
                faultModel.ReportedById = CurrentUserId();
                ModelState.Remove("RoomId");
                ModelState.Remove("ReportedById");
            }

            faultModel.ReportedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(faultModel);
                await _context.SaveChangesAsync();

                if (photos != null && photos.Count > 0)
                {
                    var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    foreach (var file in photos)
                    {
                        if (file.Length == 0 || !allowed.Contains(file.ContentType.ToLower())) continue;
                        if (file.Length > 10 * 1024 * 1024) continue;

                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        _context.FaultPhotos.Add(new FaultPhotoModel
                        {
                            FaultId     = faultModel.Id,
                            Data        = ms.ToArray(),
                            ContentType = file.ContentType,
                            FileName    = file.FileName,
                            UploadedAt  = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["PriorityId"] = new SelectList(_context.FaultPriorities, "Id", "Name", faultModel.PriorityId);
            ViewData["CategoryId"] = new SelectList(_context.FaultCategories,  "Id", "Name", faultModel.CategoryId);
            if (IsAdminOrStaff())
            {
                ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", faultModel.RoomId);
                ViewData["ReportedById"] = new SelectList(
                    _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                    "Id", "FullName", faultModel.ReportedById);
            }
            return View(faultModel);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var fault = await _context.Faults
                .Include(f => f.Photos)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (fault == null) return NotFound();
            ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", fault.RoomId);
            ViewData["ReportedById"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,ReportedById,Description,ReportedAt,PriorityId,CategoryId,IsResolved,ResolvedAt,ResolutionNotes,ResolvedById")] FaultModel faultModel, List<IFormFile>? photos)
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

                if (photos != null && photos.Count > 0)
                {
                    var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    foreach (var file in photos)
                    {
                        if (file.Length == 0 || !allowed.Contains(file.ContentType.ToLower())) continue;
                        if (file.Length > 10 * 1024 * 1024) continue;

                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        _context.FaultPhotos.Add(new FaultPhotoModel
                        {
                            FaultId     = faultModel.Id,
                            Data        = ms.ToArray(),
                            ContentType = file.ContentType,
                            FileName    = file.FileName,
                            UploadedAt  = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Edit), new { id = faultModel.Id });
            }

            ViewData["RoomId"]       = new SelectList(_context.Rooms, "Id", "RoomNumber", faultModel.RoomId);
            ViewData["ReportedById"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
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

        [HttpGet("zdjecie/{photoId}")]
        public async Task<IActionResult> Photo(int photoId)
        {
            var photo = await _context.FaultPhotos
                .Include(p => p.Fault)
                .FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null) return NotFound();

            if (!IsAdminOrStaff())
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident?.RoomId != photo.Fault?.RoomId) return Forbid();
            }

            return File(photo.Data, photo.ContentType);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("usun-zdjecie/{photoId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var photo = await _context.FaultPhotos.FindAsync(photoId);
            if (photo == null) return NotFound();
            int faultId = photo.FaultId;
            _context.FaultPhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = faultId });
        }
    }
}
