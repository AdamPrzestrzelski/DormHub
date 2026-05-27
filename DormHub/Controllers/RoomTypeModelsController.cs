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
using DormHub.Services;

namespace DormHub.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("rodzaj_pokoju")]
    public class RoomTypeModelsController : Controller
    {
        private readonly DormDbContext _context;
        private readonly CurrencyService _currency;

        public RoomTypeModelsController(DormDbContext context, CurrencyService currency)
        {
            _context = context;
            _currency = currency;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var types = await _context.RoomTypes.ToListAsync();

            var (eurRate, usdRate, cnyRate, ilsRate, rateDate) = await GetRatesAsync();
            ViewData["EurRate"]  = eurRate;
            ViewData["UsdRate"]  = usdRate;
            ViewData["CnyRate"]  = cnyRate;
            ViewData["IlsRate"]  = ilsRate;
            ViewData["RateDate"] = rateDate;

            return View(types);
        }

        [HttpGet("szczegoly/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var roomTypeModel = await _context.RoomTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (roomTypeModel == null) return NotFound();

            var (eur, usd, cny, ils, date) = await _currency.ConvertPlnAsync(roomTypeModel.PricePerMonth);
            ViewData["PriceEur"]  = eur;
            ViewData["PriceUsd"]  = usd;
            ViewData["PriceCny"]  = cny;
            ViewData["PriceIls"]  = ils;
            ViewData["RateDate"]  = date;

            return View(roomTypeModel);
        }

        [HttpGet("dodaj")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Capacity,PricePerMonth")] RoomTypeModel roomTypeModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomTypeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(roomTypeModel);
        }

        [HttpGet("edytuj/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var roomTypeModel = await _context.RoomTypes.FindAsync(id);
            if (roomTypeModel == null) return NotFound();
            return View(roomTypeModel);
        }

        [HttpPost("edytuj/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Capacity,PricePerMonth")] RoomTypeModel roomTypeModel)
        {
            if (id != roomTypeModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomTypeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeModelExists(roomTypeModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(roomTypeModel);
        }

        [HttpGet("usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var roomTypeModel = await _context.RoomTypes.FirstOrDefaultAsync(m => m.Id == id);
            if (roomTypeModel == null) return NotFound();

            return View(roomTypeModel);
        }

        [HttpPost("usun/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomTypeModel = await _context.RoomTypes.FindAsync(id);
            if (roomTypeModel != null)
            {
                _context.RoomTypes.Remove(roomTypeModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomTypeModelExists(int id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }

        private async Task<(decimal? eur, decimal? usd, decimal? cny, decimal? ils, string? date)> GetRatesAsync()
        {
            var table = await _currency.GetTableAsync();
            if (table == null) return (null, null, null, null, null);
            var eur = table.Rates.FirstOrDefault(r => r.Code == "EUR")?.Mid;
            var usd = table.Rates.FirstOrDefault(r => r.Code == "USD")?.Mid;
            var cny = table.Rates.FirstOrDefault(r => r.Code == "CNY")?.Mid;
            var ils = table.Rates.FirstOrDefault(r => r.Code == "ILS")?.Mid;
            return (eur, usd, cny, ils, table.EffectiveDate);
        }
    }
}
