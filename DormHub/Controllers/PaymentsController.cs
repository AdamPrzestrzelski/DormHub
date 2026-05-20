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

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Resident)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
            return View(payments);
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Resident)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            ViewData["ResidentId"] = new SelectList(
                _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                "Id", "FullName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ResidentId,Amount,DueDate,Status,Description")] PaymentModel payment)
        {
            if (ModelState.IsValid)
            {
                payment.CreatedAt = DateTime.Now;
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ResidentId"] = new SelectList(
                _context.Residents.Select(r => new { r.Id, FullName = r.FirstName + " " + r.LastName }),
                "Id", "FullName", payment.ResidentId);
            return View(payment);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("zatwierdz/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            payment.Status = PaymentStatus.Paid;
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
            if (payment != null)
                _context.Payments.Remove(payment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
