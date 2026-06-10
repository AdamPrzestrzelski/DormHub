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
            var dormDbContext = _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Type)
                .Include(a => a.Status)
                .Include(a => a.PreferredBuilding)
                .Include(a => a.PreferredRoomType)
                .OrderByDescending(a => a.SubmittedAt);

            var today = DateTime.Today;
            ViewBag.ResidentsWithRoom = await _context.Residents
                .Where(r => r.RoomId != null && (r.MoveOutDate == null || r.MoveOutDate >= today))
                .Select(r => r.PersonId)
                .Distinct()
                .ToListAsync();

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
                .Include(a => a.Type)
                .Include(a => a.Status)
                .Include(a => a.PreferredBuilding)
                .Include(a => a.PreferredRoomType)
                .Include(a => a.SecondRoomType)
                .Include(a => a.ThirdRoomType)
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
            ViewData["TypeId"] = new SelectList(_context.ApplicationTypes, "Id", "Name");
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApplicantId,TypeId,PreferredRoomTypeId,PreferredBuildingId,Description,PreferredStartDate,PreferredEndDate")] ApplicationModel applicationModel)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                applicationModel.Id = Guid.NewGuid().ToString();
                applicationModel.SubmittedAt = DateTime.Now;
                applicationModel.StatusId = ApplicationStatuses.Pending;
                if (applicationModel.TypeId == 0) applicationModel.TypeId = ApplicationTypes.Place;
                _context.Add(applicationModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicantId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", applicationModel.ApplicantId);
            ViewData["TypeId"] = new SelectList(_context.ApplicationTypes, "Id", "Name", applicationModel.TypeId);
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
            ViewData["TypeId"] = new SelectList(_context.ApplicationTypes, "Id", "Name", applicationModel.TypeId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            ViewData["StatusId"] = new SelectList(_context.ApplicationStatuses, "Id", "Name", applicationModel.StatusId);
            return View(applicationModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,ApplicantId,TypeId,PreferredRoomTypeId,PreferredBuildingId,Description,PreferredStartDate,PreferredEndDate,StatusId")] ApplicationModel applicationModel)
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
            ViewData["ApplicantId"] = new SelectList(
                _context.Persons.Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName }),
                "Id", "FullName", applicationModel.ApplicantId);
            ViewData["TypeId"] = new SelectList(_context.ApplicationTypes, "Id", "Name", applicationModel.TypeId);
            ViewData["PreferredBuildingId"] = new SelectList(_context.Buildings, "Id", "Name", applicationModel.PreferredBuildingId);
            ViewData["PreferredRoomTypeId"] = new SelectList(_context.RoomTypes, "Id", "Name", applicationModel.PreferredRoomTypeId);
            ViewData["StatusId"] = new SelectList(_context.ApplicationStatuses, "Id", "Name", applicationModel.StatusId);
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
                .Include(a => a.Type)
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

            app.StatusId = ApplicationStatuses.Accepted;
            await _context.SaveChangesAsync();

            switch (app.TypeId)
            {
                case ApplicationTypes.Checkout:
                {
                    var resident = await GetLatestResidentAsync(app.ApplicantId);
                    if (resident != null)
                    {
                        var oldRoomId = resident.RoomId;
                        resident.MoveOutDate = app.PreferredEndDate ?? DateTime.Today;
                        resident.RoomId = null; // mieszkaniec znika z pokoju
                        await _context.SaveChangesAsync();
                        if (oldRoomId.HasValue) await UpdateRoomStatusAsync(oldRoomId.Value);
                    }
                    break;
                }
                case ApplicationTypes.SummerExtension:
                {
                    var resident = await GetLatestResidentAsync(app.ApplicantId);
                    if (resident != null && app.PreferredEndDate != null)
                    {
                        resident.MoveOutDate = app.PreferredEndDate;
                        await _context.SaveChangesAsync();
                    }
                    break;
                }
            }

            TempData["Success"] = "Wniosek został zaakceptowany.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("odrzuc/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();
            app.StatusId = ApplicationStatuses.Rejected;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Wniosek został odrzucony.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ten-sam-pokoj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSameRoom(string id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            var resident = await GetLatestResidentAsync(app.ApplicantId);
            if (resident == null || resident.RoomId == null)
            {
                TempData["Error"] = "Wnioskodawca nie ma przypisanego pokoju, nie można przydzielić tego samego miejsca.";
                return RedirectToAction(nameof(Index));
            }

            if (app.PreferredStartDate != null) resident.MoveInDate = app.PreferredStartDate.Value;
            resident.MoveOutDate = app.PreferredEndDate;

            app.StatusId = ApplicationStatuses.Accepted;
            await _context.SaveChangesAsync();
            await UpdateRoomStatusAsync(resident.RoomId.Value);

            TempData["Success"] = "Przydzielono ten sam pokój na nowy rok akademicki.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("przydziel/{id}")]
        public async Task<IActionResult> AssignRoom(string id)
        {
            var app = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Type)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();

            var existing = await GetLatestResidentAsync(app.ApplicantId);

            var vm = new AssignRoomViewModel
            {
                ApplicationId = app.Id,
                ApplicantName = app.Applicant == null ? "" : $"{app.Applicant.FirstName} {app.Applicant.LastName}",
                TypeId = app.TypeId,
                TypeName = app.Type?.Name ?? "",
                RoomId = existing?.RoomId,
                MoveInDate = app.PreferredStartDate ?? existing?.MoveInDate ?? DateTime.Today,
                MoveOutDate = app.PreferredEndDate ?? existing?.MoveOutDate,
                Description = app.Description
            };

            if (existing?.RoomId != null)
            {
                var room = await _context.Rooms.Include(r => r.Building).FirstOrDefaultAsync(r => r.Id == existing.RoomId);
                if (room != null)
                    vm.CurrentRoomInfo = $"Pokój #{room.RoomNumber}" + (room.Building != null ? $", {room.Building.Name}" : "");
            }

            await LoadAvailableRoomsAsync(vm.RoomId, existing?.RoomId);
            return View(vm);
        }

        [HttpPost("przydziel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoom(string id, AssignRoomViewModel vm)
        {
            var app = await _context.Applications
                .Include(a => a.Applicant)
                .Include(a => a.Type)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (app == null) return NotFound();

            var existing = await GetLatestResidentAsync(app.ApplicantId);

            if (!ModelState.IsValid)
            {
                vm.ApplicationId = app.Id;
                vm.ApplicantName = app.Applicant == null ? "" : $"{app.Applicant.FirstName} {app.Applicant.LastName}";
                vm.TypeId = app.TypeId;
                vm.TypeName = app.Type?.Name ?? "";
                await LoadAvailableRoomsAsync(vm.RoomId, existing?.RoomId);
                return View(vm);
            }

            int? oldRoomId = existing?.RoomId;

            if (existing != null)
            {
                existing.RoomId = vm.RoomId;
                existing.MoveInDate = vm.MoveInDate;
                existing.MoveOutDate = vm.MoveOutDate;
            }
            else
            {
                _context.Residents.Add(new ResidentModel
                {
                    PersonId = app.ApplicantId,
                    RoomId = vm.RoomId,
                    MoveInDate = vm.MoveInDate,
                    MoveOutDate = vm.MoveOutDate
                });
            }

            app.StatusId = ApplicationStatuses.Accepted;
            await _context.SaveChangesAsync();

            if (vm.RoomId.HasValue) await UpdateRoomStatusAsync(vm.RoomId.Value);
            if (oldRoomId.HasValue && oldRoomId != vm.RoomId) await UpdateRoomStatusAsync(oldRoomId.Value);

            TempData["Success"] = "Wniosek zaakceptowany — przydzielono pokój i zaktualizowano mieszkańca.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadAvailableRoomsAsync(int? selected, int? includeRoomId)
        {
            var today = DateTime.Today;

            var rooms = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Where(r => r.StatusId != 3) // pomijamy pokoje w remoncie
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            var activeCounts = await _context.Residents
                .Where(r => r.RoomId != null && (r.MoveOutDate == null || r.MoveOutDate >= today))
                .GroupBy(r => r.RoomId!.Value)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoomId, x => x.Count);

            var list = rooms.Select(r =>
                {
                    int cnt = activeCounts.TryGetValue(r.Id, out var c) ? c : 0;
                    int cap = r.RoomType?.Capacity ?? 1;
                    return new
                    {
                        r.Id,
                        Label = $"#{r.RoomNumber} – {r.Building?.Name} ({r.RoomType?.Name}) [{cnt}/{cap}]",
                        Full = cnt >= cap
                    };
                })
                .Where(x => !x.Full || x.Id == includeRoomId)
                .ToList();

            ViewData["Rooms"] = new SelectList(list, "Id", "Label", selected);
        }

        private async Task<ResidentModel?> GetLatestResidentAsync(int personId)
        {
            return await _context.Residents
                .Where(r => r.PersonId == personId)
                .OrderByDescending(r => r.MoveInDate)
                .FirstOrDefaultAsync();
        }

        private async Task UpdateRoomStatusAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return;

            if (room.StatusId == 3) return; // remont

            var today = DateTime.Today;
            var currentCount = await _context.Residents
                .CountAsync(r => r.RoomId == roomId && (r.MoveOutDate == null || r.MoveOutDate >= today));
            var capacity = room.RoomType?.Capacity ?? 1;

            if (currentCount == 0) room.StatusId = 1;       // wolny
            else if (currentCount < capacity) room.StatusId = 4; // częściowo zajęty
            else room.StatusId = 2;                          // zajęty

            await _context.SaveChangesAsync();
        }
    }
}
