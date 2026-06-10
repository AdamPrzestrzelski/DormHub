using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers
{
    [Authorize]
    [Route("moje-wnioski")]
    public class MyApplicationsController : Controller
    {
        private readonly DormDbContext _context;

        public MyApplicationsController(DormDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────── helpers ───────────────────────────

        private int? GetPersonId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : (int?)null;
        }

        private async Task<ResidentModel?> GetActiveResidentAsync(int personId)
        {
            var today = DateTime.Today;
            return await _context.Residents
                .Include(r => r.Room).ThenInclude(room => room!.RoomType)
                .Include(r => r.Room).ThenInclude(room => room!.Building)
                .Where(r => r.PersonId == personId &&
                            (r.MoveOutDate == null || r.MoveOutDate >= today))
                .OrderByDescending(r => r.MoveInDate)
                .FirstOrDefaultAsync();
        }

        private static DateTime UpcomingOct1()
        {
            var t = DateTime.Today;
            var d = new DateTime(t.Year, 10, 1);
            if (t > d) d = d.AddYears(1);
            return d;
        }

        private static DateTime UpcomingJul1()
        {
            var t = DateTime.Today;
            var d = new DateTime(t.Year, 7, 1);
            if (t > d) d = d.AddYears(1);
            return d;
        }

        private void LoadRoomTypeDropdowns(SelfApplicationViewModel vm)
        {
            var roomTypes = _context.RoomTypes
                .OrderBy(rt => rt.Name)
                .Select(rt => new { rt.Id, Label = rt.Name + " (" + rt.Capacity + " os.)" })
                .ToList();

            ViewData["RoomTypes1"] = new SelectList(roomTypes, "Id", "Label", vm.PreferredRoomTypeId);
            ViewData["RoomTypes2"] = new SelectList(roomTypes, "Id", "Label", vm.SecondRoomTypeId);
            ViewData["RoomTypes3"] = new SelectList(roomTypes, "Id", "Label", vm.ThirdRoomTypeId);
            ViewData["Buildings"] = new SelectList(_context.Buildings.OrderBy(b => b.Name), "Id", "Name", vm.PreferredBuildingId);
        }

        // ─────────────────────────── Index ───────────────────────────

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var personId = GetPersonId();
            if (personId == null) return RedirectToAction("Login", "Account");

            var resident = await GetActiveResidentAsync(personId.Value);
            ViewBag.IsResident = resident != null;
            ViewBag.CurrentRoom = resident?.Room == null
                ? null
                : $"Pokój {resident.Room.RoomNumber}" +
                  (resident.Room.Building != null ? $", {resident.Room.Building.Name}" : "");

            var apps = await _context.Applications
                .Where(a => a.ApplicantId == personId.Value)
                .Include(a => a.Type)
                .Include(a => a.Status)
                .Include(a => a.PreferredRoomType)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(apps);
        }

        // ─────────────────────────── Create (GET) ───────────────────────────

        [HttpGet("nowy/{type:int}")]
        public async Task<IActionResult> Create(int type)
        {
            var personId = GetPersonId();
            if (personId == null) return RedirectToAction("Login", "Account");

            var resident = await GetActiveResidentAsync(personId.Value);
            bool isResident = resident != null;

            var typeEntity = await _context.ApplicationTypes.FindAsync(type);
            if (typeEntity == null)
            {
                TempData["Error"] = "Nieznany typ wniosku.";
                return RedirectToAction(nameof(Index));
            }

            // Walidacja uprawnień
            if (type == ApplicationTypes.Place && isResident)
            {
                TempData["Error"] = "Masz już miejsce w akademiku — wybierz inny typ wniosku.";
                return RedirectToAction(nameof(Index));
            }
            if (type != ApplicationTypes.Place && !isResident)
            {
                TempData["Error"] = "Ten wniosek jest dostępny tylko dla mieszkańców.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new SelfApplicationViewModel
            {
                TypeId = type,
                TypeName = typeEntity.Name,
                TypeNameEn = typeEntity.NameEn
            };

            if (resident?.Room != null)
            {
                vm.CurrentRoomInfo = $"Pokój {resident.Room.RoomNumber}" +
                    (resident.Room.Building != null ? $", {resident.Room.Building.Name}" : "") +
                    (resident.Room.RoomType != null ? $" ({resident.Room.RoomType.Name})" : "");
            }

            ConfigureViewModelForType(vm, resident);
            LoadRoomTypeDropdowns(vm);
            return View(vm);
        }

        // ─────────────────────────── Create (POST) ───────────────────────────

        [HttpPost("nowy/{type:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int type, SelfApplicationViewModel vm)
        {
            var personId = GetPersonId();
            if (personId == null) return RedirectToAction("Login", "Account");

            var resident = await GetActiveResidentAsync(personId.Value);
            bool isResident = resident != null;
            vm.TypeId = type;

            var typeEntity = await _context.ApplicationTypes.FindAsync(type);
            if (typeEntity == null)
            {
                TempData["Error"] = "Nieznany typ wniosku.";
                return RedirectToAction(nameof(Index));
            }
            vm.TypeName = typeEntity.Name;
            vm.TypeNameEn = typeEntity.NameEn;

            if (type == ApplicationTypes.Place && isResident)
            {
                TempData["Error"] = "Masz już miejsce w akademiku.";
                return RedirectToAction(nameof(Index));
            }
            if (type != ApplicationTypes.Place && !isResident)
            {
                TempData["Error"] = "Ten wniosek jest dostępny tylko dla mieszkańców.";
                return RedirectToAction(nameof(Index));
            }

            ConfigureViewModelForType(vm, resident, applyDefaults: false);

            // Walidacja zależna od typu
            if ((type == ApplicationTypes.RoomChange || type == ApplicationTypes.NextYear)
                && vm.PreferredRoomTypeId == null)
            {
                ModelState.AddModelError(nameof(vm.PreferredRoomTypeId), "Wybierz przynajmniej pierwszy typ pokoju.");
            }

            if (!ModelState.IsValid)
            {
                if (resident?.Room != null)
                {
                    vm.CurrentRoomInfo = $"Pokój {resident.Room.RoomNumber}" +
                        (resident.Room.Building != null ? $", {resident.Room.Building.Name}" : "");
                }
                LoadRoomTypeDropdowns(vm);
                return View(vm);
            }

            var app = new ApplicationModel
            {
                Id = Guid.NewGuid().ToString(),
                ApplicantId = personId.Value,
                TypeId = type,
                StatusId = ApplicationStatuses.Pending,
                SubmittedAt = DateTime.Now,
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
            };

            if (vm.ShowSinglePreference)
            {
                app.PreferredRoomTypeId = vm.PreferredRoomTypeId;
                app.PreferredBuildingId = vm.PreferredBuildingId;
            }

            if (vm.ShowRoomChoices)
            {
                app.PreferredRoomTypeId = vm.PreferredRoomTypeId;
                app.SecondRoomTypeId = vm.SecondRoomTypeId;
                app.ThirdRoomTypeId = vm.ThirdRoomTypeId;
            }

            if (vm.ShowPeriod)
            {
                app.PreferredStartDate = vm.PreferredStartDate;
                app.PreferredEndDate = vm.PreferredEndDate;
            }

            if (vm.ShowCheckoutDate)
            {
                app.PreferredEndDate = vm.PreferredEndDate;
            }

            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wniosek został złożony i oczekuje na rozpatrzenie.";
            return RedirectToAction(nameof(Index));
        }

        // Ustawia flagi widoku i (opcjonalnie) domyślne daty dla danego typu wniosku
        private void ConfigureViewModelForType(SelfApplicationViewModel vm, ResidentModel? resident, bool applyDefaults = true)
        {
            switch (vm.TypeId)
            {
                case ApplicationTypes.Place:
                    vm.ShowSinglePreference = true;
                    vm.ShowBuilding = true;
                    vm.ShowPeriod = true;
                    vm.Hint = "Standardowy okres zamieszkania trwa od 1 października do 1 lipca.";
                    if (applyDefaults)
                    {
                        vm.PreferredStartDate ??= UpcomingOct1();
                        vm.PreferredEndDate ??= UpcomingOct1().AddMonths(9);
                    }
                    break;

                case ApplicationTypes.RoomChange:
                    vm.ShowRoomChoices = true;
                    vm.Hint = "Wskaż preferowane typy pokoju w kolejności priorytetu. W opisie możesz wskazać konkretny pokój.";
                    break;

                case ApplicationTypes.SummerExtension:
                    vm.ShowPeriod = true;
                    vm.Hint = "Przedłużenie obejmuje okres wakacyjny: od 1 lipca do 1 października.";
                    if (applyDefaults)
                    {
                        vm.PreferredStartDate ??= UpcomingJul1();
                        vm.PreferredEndDate ??= UpcomingJul1().AddMonths(3);
                    }
                    break;

                case ApplicationTypes.NextYear:
                    vm.ShowRoomChoices = true;
                    vm.ShowPeriod = true;
                    vm.Hint = "Wybierz typy pokoju na kolejny rok akademicki (priorytetowo). W opisie możesz napisać, że chcesz ten sam pokój.";
                    if (applyDefaults)
                    {
                        vm.PreferredStartDate ??= UpcomingOct1();
                        vm.PreferredEndDate ??= UpcomingOct1().AddMonths(9);
                    }
                    break;

                case ApplicationTypes.Checkout:
                    vm.ShowCheckoutDate = true;
                    vm.Hint = "Złóż wniosek o wymeldowanie. Podaj planowaną datę opuszczenia akademika.";
                    if (applyDefaults)
                    {
                        vm.PreferredEndDate ??= DateTime.Today;
                    }
                    break;
            }
        }
    }
}
