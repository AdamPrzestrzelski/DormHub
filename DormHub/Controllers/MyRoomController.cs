using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers
{
    [Authorize]
    [Route("moj-pokoj")]
    public class MyRoomController : Controller
    {
        private readonly DormDbContext _context;

        public MyRoomController(DormDbContext context)
        {
            _context = context;
        }

        private int? GetPersonId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : (int?)null;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var personId = GetPersonId();
            if (personId == null) return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var resident = await _context.Residents
                .Include(r => r.Room).ThenInclude(rm => rm!.Building)
                .Include(r => r.Room).ThenInclude(rm => rm!.RoomType)
                .Include(r => r.Room).ThenInclude(rm => rm!.Status)
                .Where(r => r.PersonId == personId.Value &&
                            (r.MoveOutDate == null || r.MoveOutDate >= today))
                .OrderByDescending(r => r.MoveInDate)
                .FirstOrDefaultAsync();

            if (resident?.RoomId != null)
            {
                ViewBag.Roommates = await _context.Residents
                    .Include(r => r.Person)
                    .Where(r => r.RoomId == resident.RoomId &&
                                r.PersonId != personId.Value &&
                                (r.MoveOutDate == null || r.MoveOutDate >= today))
                    .OrderBy(r => r.MoveInDate)
                    .ToListAsync();

                ViewBag.OpenFaults = await _context.Faults
                    .CountAsync(f => f.RoomId == resident.RoomId && !f.IsResolved);
            }
            else
            {
                ViewBag.Roommates = new List<ResidentModel>();
                ViewBag.OpenFaults = 0;
            }

            return View(resident);
        }
    }
}
