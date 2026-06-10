using DormHub.Data;
using DormHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers.Api
{
    [ApiController]
    [Route("api/rooms")]
    [Produces("application/json")]
    [Authorize]
    public class RoomsApiController : ControllerBase
    {
        private readonly DormDbContext _db;
        public RoomsApiController(DormDbContext db) => _db = db;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoomDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _db.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .Select(r => new RoomDto
                {
                    Id           = r.Id,
                    RoomNumber   = r.RoomNumber,
                    Floor        = r.Floor,
                    Building     = r.Building != null ? r.Building.Name : null,
                    RoomType     = r.RoomType != null ? r.RoomType.Name : null,
                    Capacity     = r.RoomType != null ? r.RoomType.Capacity : 0,
                    PricePerMonth = r.RoomType != null ? r.RoomType.PricePerMonth : 0,
                    Status       = r.Status != null ? r.Status.Name : null
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoomDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _db.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return NotFound(new { message = $"Pokój o ID {id} nie istnieje." });

            return Ok(new RoomDto
            {
                Id            = r.Id,
                RoomNumber    = r.RoomNumber,
                Floor         = r.Floor,
                Building      = r.Building?.Name,
                RoomType      = r.RoomType?.Name,
                Capacity      = r.RoomType?.Capacity ?? 0,
                PricePerMonth = r.RoomType?.PricePerMonth ?? 0,
                Status        = r.Status?.Name
            });
        }

        [HttpGet("by-building/{buildingId:int}")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetByBuilding(int buildingId)
        {
            var rooms = await _db.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .Where(r => r.BuildingId == buildingId)
                .OrderBy(r => r.RoomNumber)
                .Select(r => new
                {
                    id    = r.Id,
                    label = $"Pokój {r.RoomNumber} (p. {r.Floor}) – {(r.RoomType != null ? r.RoomType.Name : "?")} [{(r.Status != null ? r.Status.Name : "?")}]"
                })
                .ToListAsync();

            return Ok(rooms);
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<RoomDto>), 200)]
        public async Task<IActionResult> GetAvailable()
        {
            var rooms = await _db.Rooms
                .Include(r => r.Building)
                .Include(r => r.RoomType)
                .Include(r => r.Status)
                .Where(r => r.StatusId == 1)
                .Select(r => new RoomDto
                {
                    Id            = r.Id,
                    RoomNumber    = r.RoomNumber,
                    Floor         = r.Floor,
                    Building      = r.Building != null ? r.Building.Name : null,
                    RoomType      = r.RoomType != null ? r.RoomType.Name : null,
                    Capacity      = r.RoomType != null ? r.RoomType.Capacity : 0,
                    PricePerMonth = r.RoomType != null ? r.RoomType.PricePerMonth : 0,
                    Status        = r.Status != null ? r.Status.Name : null
                })
                .ToListAsync();

            return Ok(rooms);
        }
    }

    public class RoomDto
    {
        public int     Id            { get; set; }
        public int     RoomNumber    { get; set; }
        public int     Floor         { get; set; }
        public string? Building      { get; set; }
        public string? RoomType      { get; set; }
        public int     Capacity      { get; set; }
        public decimal PricePerMonth { get; set; }
        public string? Status        { get; set; }
    }
}
