using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DormHub.Controllers
{
    public class DashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int UnderMaintenanceRooms { get; set; }
        public int TotalResidents { get; set; }
        public int OpenFaults { get; set; }
        public int CriticalFaults { get; set; }
        public int PendingApplications { get; set; }
        public int OverduePayments { get; set; }
        public int PendingPayments { get; set; }
        public List<AnnouncementModel> RecentAnnouncements { get; set; } = new();
        public List<FaultModel> RecentFaults { get; set; } = new();
    }

    public class HomeController : Controller
    {
        private readonly DormDbContext _context;

        public HomeController(DormDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalRooms            = await _context.Rooms.CountAsync(),
                AvailableRooms        = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Available),
                OccupiedRooms         = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied),
                UnderMaintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.UnderMaintenance),
                TotalResidents        = await _context.Residents.CountAsync(),
                OpenFaults            = await _context.Faults.CountAsync(f => !f.IsResolved),
                CriticalFaults        = await _context.Faults.CountAsync(f => !f.IsResolved && f.Priority == FaultPriority.Critical),
                PendingApplications   = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Pending),
                OverduePayments       = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Overdue),
                PendingPayments       = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending),
                RecentAnnouncements   = await _context.Announcements
                    .Include(a => a.Author)
                    .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.Now)
                    .OrderByDescending(a => a.IsPinned)
                    .ThenByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentFaults = await _context.Faults
                    .Include(f => f.Room)
                    .Where(f => !f.IsResolved)
                    .OrderByDescending(f => f.Priority)
                    .ThenByDescending(f => f.ReportedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
