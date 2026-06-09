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
    [Route("zasoby")]
    public class ResourcesController : Controller
    {
        private readonly DormDbContext _context;

        public ResourcesController(DormDbContext context)
        {
            _context = context;
        }


        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdmin() => User.IsInRole("Admin");

        
        private async Task<int?> GetResidentBuildingIdAsync()
        {
            var resident = await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
            return resident?.Room?.BuildingId;
        }

        private async Task<ResidentModel?> GetCurrentResidentAsync()
        {
            return await _context.Residents
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
        }


        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            IQueryable<ResourceModel> query = _context.Resources
                .Include(r => r.Building)
                .OrderBy(r => r.Building!.Name)
                .ThenBy(r => r.Category)
                .ThenBy(r => r.Name);

            if (!IsAdmin())
            {
                var buildingId = await GetResidentBuildingIdAsync();
                if (buildingId == null)
                {
                    TempData["Error"] = "Nie masz przypisanego pokoju – nie możesz przeglądać zasobów.";
                    return View(new List<ResourceModel>());
                }
                query = query.Where(r => r.BuildingId == buildingId && r.IsActive);
            }

            return View(await query.ToListAsync());
        }

        

        [Authorize(Roles = "Admin")]
        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
            ViewData["Categories"] = GetCategorySelectList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Description,BuildingId,IsActive")] ResourceModel model)
        {
            if (model.Category == "Inne" && string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError("Description", "Dla kategorii 'Inne' opis/nazwa jest wymagana.");

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Zasób został dodany.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name", model.BuildingId);
            ViewData["Categories"] = GetCategorySelectList(model.Category);
            return View(model);
        }

        

        [Authorize(Roles = "Admin")]
        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name", resource.BuildingId);
            ViewData["Categories"] = GetCategorySelectList(resource.Category);
            return View(resource);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Category,Description,BuildingId,IsActive")] ResourceModel model)
        {
            if (id != model.Id) return NotFound();

            if (model.Category == "Inne" && string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError("Description", "Dla kategorii 'Inne' opis/nazwa jest wymagana.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Resources.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                TempData["Success"] = "Zasób został zaktualizowany.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name", model.BuildingId);
            ViewData["Categories"] = GetCategorySelectList(model.Category);
            return View(model);
        }

        

        [Authorize(Roles = "Admin")]
        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var resource = await _context.Resources
                .Include(r => r.Building)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) return NotFound();
            return View(resource);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Zasób został usunięty.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("rezerwuj/{id}")]
        public async Task<IActionResult> Book(int? id, DateTime? date)
        {
            if (id == null) return NotFound();

            var resource = await _context.Resources.Include(r => r.Building).FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null || !resource.IsActive) return NotFound();

            if (!IsAdmin())
            {
                var buildingId = await GetResidentBuildingIdAsync();
                if (buildingId == null)
                {
                    TempData["Error"] = "Nie masz przypisanego pokoju.";
                    return RedirectToAction(nameof(Index));
                }
                if (resource.BuildingId != buildingId)
                    return Forbid();
            }

            var selectedDate = date?.Date ?? DateTime.Today;

            var existingBookings = await _context.ResourceBookings
                .Include(b => b.Resident)
                    .ThenInclude(r => r!.Person)
                .Where(b => b.ResourceId == id && b.BookingDate.Date == selectedDate && !b.IsCancelled)
                .OrderBy(b => b.StartTime)
                .ToListAsync();

            ViewBag.Resource = resource;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.ExistingBookings = existingBookings;
            return View();
        }

        [HttpPost("rezerwuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int id, DateTime bookingDate, string startTime, string endTime, string? notes)
        {
            var resource = await _context.Resources.Include(r => r.Building).FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null || !resource.IsActive) return NotFound();

            if (!TimeSpan.TryParse(startTime, out var start) || !TimeSpan.TryParse(endTime, out var end))
            {
                TempData["Error"] = "Nieprawidłowy format godziny.";
                return RedirectToAction(nameof(Book), new { id, date = bookingDate });
            }

            if (end <= start)
            {
                TempData["Error"] = "Godzina końca musi być późniejsza niż godzina początku.";
                return RedirectToAction(nameof(Book), new { id, date = bookingDate });
            }

            if (bookingDate.Date < DateTime.Today)
            {
                TempData["Error"] = "Nie można rezerwować w przeszłości.";
                return RedirectToAction(nameof(Book), new { id, date = bookingDate });
            }

            ResidentModel? resident;
            if (IsAdmin())
            {
                resident = await _context.Residents
                    .FirstOrDefaultAsync(r => r.Room!.BuildingId == resource.BuildingId);
                if (resident == null)
                {
                    TempData["Error"] = "Brak mieszkańców w tym budynku.";
                    return RedirectToAction(nameof(Book), new { id, date = bookingDate });
                }
            }
            else
            {
                resident = await GetCurrentResidentAsync();
                if (resident == null)
                {
                    TempData["Error"] = "Nie masz przypisanego pokoju.";
                    return RedirectToAction(nameof(Index));
                }
                if (resident.Room?.BuildingId != resource.BuildingId)
                    return Forbid();
            }

            bool conflict = await _context.ResourceBookings.AnyAsync(b =>
                b.ResourceId == id &&
                b.BookingDate.Date == bookingDate.Date &&
                !b.IsCancelled &&
                b.StartTime < end &&
                b.EndTime > start);

            if (conflict)
            {
                TempData["Error"] = "Wybrany przedział czasowy nakłada się z istniejącą rezerwacją. Wybierz inny termin.";
                return RedirectToAction(nameof(Book), new { id, date = bookingDate });
            }

            var booking = new ResourceBookingModel
            {
                ResourceId = id,
                ResidentId = resident.Id,
                BookingDate = bookingDate.Date,
                StartTime = start,
                EndTime = end,
                Notes = notes,
                CreatedAt = DateTime.Now
            };

            _context.ResourceBookings.Add(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja została zapisana.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet("moje")]
        public async Task<IActionResult> MyBookings()
        {
            IQueryable<ResourceBookingModel> query = _context.ResourceBookings
                .Include(b => b.Resource)
                    .ThenInclude(r => r!.Building)
                .Include(b => b.Resident)
                    .ThenInclude(r => r!.Person)
                .OrderByDescending(b => b.BookingDate)
                    .ThenByDescending(b => b.StartTime);

            if (!IsAdmin())
            {
                var resident = await GetCurrentResidentAsync();
                if (resident == null) return View(new List<ResourceBookingModel>());
                query = query.Where(b => b.ResidentId == resident.Id);
            }

            return View(await query.ToListAsync());
        }

        [HttpPost("anuluj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.ResourceBookings
                .Include(b => b.Resident)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (!IsAdmin())
            {
                var resident = await GetCurrentResidentAsync();
                if (resident == null || booking.ResidentId != resident.Id)
                    return Forbid();
            }

            if (booking.BookingDate.Date < DateTime.Today ||
                (booking.BookingDate.Date == DateTime.Today && booking.StartTime < DateTime.Now.TimeOfDay))
            {
                TempData["Error"] = "Nie można anulować rezerwacji, która już się rozpoczęła lub minęła.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.IsCancelled = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(MyBookings));
        }


        private static SelectList GetCategorySelectList(string? selected = null)
        {
            var cats = new[]
            {
                new { Value = "Pralka",     Text = "🫧 Pralka" },
                new { Value = "Suszarka",   Text = "💨 Suszarka" },
                new { Value = "Zelazko",    Text = "👔 Żelazko" },
                new { Value = "Odkurzacz",  Text = "🌀 Odkurzacz" },
                new { Value = "Inne",       Text = "📦 Inne" },
            };
            return new SelectList(cats, "Value", "Text", selected);
        }
    }
}
