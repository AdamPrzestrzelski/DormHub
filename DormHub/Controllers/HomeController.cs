using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

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

        public bool HasRoom { get; set; }
        public string? MyRoomLabel { get; set; }
        public int MyRoommates { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly DormDbContext _context;

        public HomeController(DormDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalRooms            = await _context.Rooms.CountAsync(),
                AvailableRooms        = await _context.Rooms.CountAsync(r => r.StatusId == 1),
                OccupiedRooms         = await _context.Rooms.CountAsync(r => r.StatusId == 2),
                UnderMaintenanceRooms = await _context.Rooms.CountAsync(r => r.StatusId == 3),
                TotalResidents        = await _context.Residents.CountAsync(),
                OpenFaults            = await _context.Faults.CountAsync(f => !f.IsResolved),
                CriticalFaults        = await _context.Faults.CountAsync(f => !f.IsResolved && f.PriorityId == 4),
                PendingApplications   = await _context.Applications.CountAsync(a => a.StatusId == 1),
                OverduePayments       = await _context.Payments.CountAsync(p => p.StatusId == 3),
                PendingPayments       = await _context.Payments.CountAsync(p => p.StatusId == 1),
                RecentAnnouncements   = await _context.Announcements
                    .Include(a => a.Author)
                    .Where(a => a.ExpiresAt == null || a.ExpiresAt > DateTime.Now)
                    .OrderByDescending(a => a.IsPinned)
                    .ThenByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentFaults = await _context.Faults
                    .Include(f => f.Room)
                    .Include(f => f.Priority)
                    .Where(f => !f.IsResolved)
                    .OrderByDescending(f => f.PriorityId)
                    .ThenByDescending(f => f.ReportedAt)
                    .Take(5)
                    .ToListAsync()
            };

            var personIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(personIdStr, out var personId))
            {
                var today = DateTime.Today;
                var myResident = await _context.Residents
                    .Include(r => r.Room).ThenInclude(rm => rm!.Building)
                    .Where(r => r.PersonId == personId && r.RoomId != null &&
                                (r.MoveOutDate == null || r.MoveOutDate >= today))
                    .OrderByDescending(r => r.MoveInDate)
                    .FirstOrDefaultAsync();

                if (myResident?.Room != null)
                {
                    vm.HasRoom = true;
                    vm.MyRoomLabel = $"#{myResident.Room.RoomNumber}" +
                        (myResident.Room.Building != null ? $", {myResident.Room.Building.Name}" : "");
                    vm.MyRoommates = await _context.Residents.CountAsync(r =>
                        r.RoomId == myResident.RoomId && r.PersonId != personId &&
                        (r.MoveOutDate == null || r.MoveOutDate >= today));
                }
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
