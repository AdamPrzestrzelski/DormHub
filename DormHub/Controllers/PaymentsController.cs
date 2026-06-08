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
    [Route("platnosci")]
    public class PaymentsController : Controller
    {
        private readonly DormDbContext _context;

        public PaymentsController(DormDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            IQueryable<PaymentModel> query = _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.Status);

            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident == null)
                    return View(new List<PaymentModel>());
                query = query.Where(p => p.ResidentId == resident.Id);
            }

            return View(await query.OrderByDescending(p => p.DueDate).ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _context.Payments
                .Include(p => p.Resident)
                .Include(p => p.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident == null || payment.ResidentId != resident.Id)
                    return Forbid();
            }

            return View(payment);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                ViewData["ResidentId"] = new SelectList(
                    _context.Residents
                        .Where(r => r.Person != null)
                        .Select(r => new { r.Id, FullName = r.Person.FirstName + " " + r.Person.LastName }),
                    "Id", "FullName");
            }
            ViewData["StatusId"] = new SelectList(_context.PaymentStatuses, "Id", "Name");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ResidentId,Amount,Description")] PaymentModel payment)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.PersonId == CurrentUserId());
                if (resident == null)
                    return Forbid();
                payment.ResidentId = resident.Id;
                ModelState.Remove("ResidentId");
            }

            ModelState.Remove("DueDate");
            ModelState.Remove("StatusId");

            if (ModelState.IsValid)
            {
                payment.DueDate   = DateTime.Now;
                payment.PaidAt    = DateTime.Now;
                payment.StatusId  = 2;
                payment.CreatedAt = DateTime.Now;
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                ViewData["ResidentId"] = new SelectList(
                    _context.Residents
                        .Where(r => r.Person != null)
                        .Select(r => new { r.Id, FullName = r.Person.FirstName + " " + r.Person.LastName }),
                    "Id", "FullName", payment.ResidentId);
            }
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("zatwierdz/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();
            payment.StatusId = 2;
            payment.PaidAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _context.Payments
                .Include(p => p.Resident)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (payment == null) return NotFound();
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null) _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
