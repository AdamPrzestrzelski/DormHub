using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DormHub.Data;
using DormHub.Models;

namespace DormHub.Controllers
{
    [Authorize]
    [Route("ogloszenia")]
    public class AnnouncementsController : Controller
    {
        private readonly DormDbContext _context;

        public AnnouncementsController(DormDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Building)
                .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.Now)
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(announcements);
        }

        [HttpGet("szczegoly/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
            ViewData["AuthorId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Content,AuthorId,ExpiresAt,IsPinned,BuildingId")] AnnouncementModel announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.Now;
                _context.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name", announcement.BuildingId);
            ViewData["AuthorId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", announcement.AuthorId);
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var announcement = await _context.Announcements
                .Include(a => a.Author)
                .Include(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
                _context.Announcements.Remove(announcement);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
