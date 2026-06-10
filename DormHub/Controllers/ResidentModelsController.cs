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
    [Route("mieszkancy")]
    public class ResidentModelsController : Controller
    {
        private readonly DormDbContext _context;

        public ResidentModelsController(DormDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var residents = _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room);
            return View(await residents.ToListAsync());
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null) return NotFound();
            return View(residentModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            var existingPersonIds = _context.Residents.Select(r => r.PersonId).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingPersonIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();

            ViewData["PersonId"]   = new SelectList(availablePersons, "Id", "FullName");
            ViewData["BuildingId"] = new SelectList(_context.Buildings.OrderBy(b => b.Name), "Id", "Name");
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("PersonId,RoomId,MoveInDate,MoveOutDate")] ResidentModel residentModel)
        {
            if (await _context.Residents.AnyAsync(r => r.PersonId == residentModel.PersonId))
            {
                ModelState.AddModelError("PersonId", "Ta osoba jest już mieszkańcem.");
            }

            if (ModelState.IsValid)
            {
                _context.Residents.Add(residentModel);
                await _context.SaveChangesAsync();

                if (residentModel.RoomId.HasValue)
                {
                    await UpdateRoomStatusAsync(residentModel.RoomId.Value);
                }

                return RedirectToAction(nameof(Index));
            }

            var existingPersonIds = _context.Residents.Select(r => r.PersonId).ToHashSet();
            var availablePersons = _context.Persons
                .Where(p => !existingPersonIds.Contains(p.Id))
                .Select(p => new { p.Id, FullName = p.FirstName + " " + p.LastName + " (" + p.Email + ")" })
                .ToList();

            ViewData["PersonId"]   = new SelectList(availablePersons, "Id", "FullName", residentModel.PersonId);
            ViewData["BuildingId"] = new SelectList(_context.Buildings.OrderBy(b => b.Name), "Id", "Name");
            return View(residentModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents.FindAsync(id);
            if (residentModel == null) return NotFound();
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,PersonId,RoomId,MoveInDate,MoveOutDate")] ResidentModel residentModel)
        {
            if (id != residentModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(residentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Residents.Any(e => e.Id == residentModel.Id)) return NotFound();
                    else throw;
                }

                if (residentModel.RoomId.HasValue)
                {
                    await UpdateRoomStatusAsync(residentModel.RoomId.Value);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "RoomNumber", residentModel.RoomId);
            return View(residentModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var residentModel = await _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (residentModel == null) return NotFound();
            return View(residentModel);
        }

        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var residentModel = await _context.Residents.FindAsync(id);
            int? roomId = residentModel?.RoomId;
            if (residentModel != null) _context.Residents.Remove(residentModel);
                await _context.SaveChangesAsync();
            if (roomId.HasValue) await UpdateRoomStatusAsync(roomId.Value);
            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateRoomStatusAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return;

            if (room.StatusId == 3) return; //remont

            var today = DateTime.Today;
            var currentCount = await _context.Residents
                .CountAsync(r => r.RoomId == roomId && (r.MoveOutDate == null || r.MoveOutDate >= today));
            var capacity = room.RoomType?.Capacity ?? 1;

            if (currentCount == 0) room.StatusId = 1; //wolny
            else if (currentCount < capacity) room.StatusId = 4; //czesciowo zajety
            else room.StatusId = 2; //zajety

            await _context.SaveChangesAsync();
        }

        [HttpGet("przydziel/{id}")]
        public async Task<IActionResult> AssignRoom(int? id)
        {
            if (id == null) return NotFound();
            var resident = await _context.Residents
                .Include(r => r.Person)
                .Include(r => r.Room).ThenInclude(rm => rm!.Building)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (resident == null) return NotFound();

            var vm = new AssignRoomViewModel
            {
                ApplicationId = string.Empty,
                ApplicantName = resident.Person == null ? "" : $"{resident.Person.FirstName} {resident.Person.LastName}",
                TypeName = "Przydział pokoju",
                RoomId = resident.RoomId,
                MoveInDate = resident.MoveInDate,
                MoveOutDate = resident.MoveOutDate
            };

            if (resident.Room != null)
                vm.CurrentRoomInfo = $"Pokój #{resident.Room.RoomNumber}" +
                    (resident.Room.Building != null ? $", {resident.Room.Building.Name}" : "");

            ViewBag.FormAction = "AssignRoom";
            ViewBag.RouteId = resident.Id;
            await LoadAvailableRoomsAsync(vm.RoomId, resident.RoomId);
            return View(vm);
        }

        [HttpPost("przydziel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoom(int id, AssignRoomViewModel vm)
        {
            var resident = await _context.Residents
                .Include(r => r.Person)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (resident == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.ApplicantName = resident.Person == null ? "" : $"{resident.Person.FirstName} {resident.Person.LastName}";
                vm.TypeName = "Przydział pokoju";
                ViewBag.FormAction = "AssignRoom";
                ViewBag.RouteId = resident.Id;
                await LoadAvailableRoomsAsync(vm.RoomId, resident.RoomId);
                return View(vm);
            }

            int? oldRoomId = resident.RoomId;
            resident.RoomId = vm.RoomId;
            resident.MoveInDate = vm.MoveInDate;
            resident.MoveOutDate = vm.MoveOutDate;
            await _context.SaveChangesAsync();

            if (vm.RoomId.HasValue) await UpdateRoomStatusAsync(vm.RoomId.Value);
            if (oldRoomId.HasValue && oldRoomId != vm.RoomId) await UpdateRoomStatusAsync(oldRoomId.Value);

            TempData["Success"] = "Pokój został przypisany.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("przydziel-osobie/{personId}")]
        public async Task<IActionResult> AssignRoomToPerson(int personId)
        {
            var person = await _context.Persons.FindAsync(personId);
            if (person == null) return NotFound();

            var existing = await _context.Residents
                .Where(r => r.PersonId == personId)
                .OrderByDescending(r => r.MoveInDate)
                .FirstOrDefaultAsync();
            if (existing != null)
                return RedirectToAction(nameof(AssignRoom), new { id = existing.Id });

            var vm = new AssignRoomViewModel
            {
                ApplicantName = $"{person.FirstName} {person.LastName}",
                TypeName = "Przydział pokoju",
                MoveInDate = DateTime.Today
            };

            ViewBag.FormAction = "AssignRoomToPerson";
            ViewBag.RouteId = person.Id;
            await LoadAvailableRoomsAsync(null, null);
            return View("AssignRoom", vm);
        }

        [HttpPost("przydziel-osobie/{personId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRoomToPerson(int personId, AssignRoomViewModel vm)
        {
            var person = await _context.Persons.FindAsync(personId);
            if (person == null) return NotFound();

            var existing = await _context.Residents
                .Where(r => r.PersonId == personId)
                .OrderByDescending(r => r.MoveInDate)
                .FirstOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                vm.ApplicantName = $"{person.FirstName} {person.LastName}";
                vm.TypeName = "Przydział pokoju";
                ViewBag.FormAction = "AssignRoomToPerson";
                ViewBag.RouteId = person.Id;
                await LoadAvailableRoomsAsync(vm.RoomId, existing?.RoomId);
                return View("AssignRoom", vm);
            }

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
                    PersonId = personId,
                    RoomId = vm.RoomId,
                    MoveInDate = vm.MoveInDate,
                    MoveOutDate = vm.MoveOutDate
                });
            }

            await _context.SaveChangesAsync();
            if (vm.RoomId.HasValue) await UpdateRoomStatusAsync(vm.RoomId.Value);

            TempData["Success"] = "Przydzielono pokój i dodano mieszkańca.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadAvailableRoomsAsync(int? selected, int? includeRoomId)
        {
            var today = DateTime.Today;

            var rooms = await _context.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Where(r => r.StatusId != 3)
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
    }
}
