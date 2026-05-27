using DormHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers.Api
{
    [ApiController]
    [Route("api/faults")]
    [Produces("application/json")]
    public class FaultsApiController : ControllerBase
    {
        private readonly DormDbContext _db;
        public FaultsApiController(DormDbContext db) => _db = db;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FaultDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var faults = await _db.Faults
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category)
                .Include(f => f.ReportedBy)
                .Select(f => new FaultDto
                {
                    Id             = f.Id,
                    RoomNumber     = f.Room != null ? f.Room.RoomNumber : 0,
                    Description    = f.Description,
                    Priority       = f.Priority != null ? f.Priority.Name : null,
                    Category       = f.Category != null ? f.Category.Name : null,
                    ReportedBy     = f.ReportedBy != null ? f.ReportedBy.FirstName + " " + f.ReportedBy.LastName : null,
                    ReportedAt     = f.ReportedAt,
                    IsResolved     = f.IsResolved,
                    ResolvedAt     = f.ResolvedAt,
                    ResolutionNotes = f.ResolutionNotes
                })
                .ToListAsync();

            return Ok(faults);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FaultDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var f = await _db.Faults
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category)
                .Include(f => f.ReportedBy)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (f == null) return NotFound(new { message = $"Usterka o ID {id} nie istnieje." });

            return Ok(new FaultDto
            {
                Id              = f.Id,
                RoomNumber      = f.Room?.RoomNumber ?? 0,
                Description     = f.Description,
                Priority        = f.Priority?.Name,
                Category        = f.Category?.Name,
                ReportedBy      = f.ReportedBy != null ? $"{f.ReportedBy.FirstName} {f.ReportedBy.LastName}" : null,
                ReportedAt      = f.ReportedAt,
                IsResolved      = f.IsResolved,
                ResolvedAt      = f.ResolvedAt,
                ResolutionNotes = f.ResolutionNotes
            });
        }

        [HttpGet("open")]
        [ProducesResponseType(typeof(IEnumerable<FaultDto>), 200)]
        public async Task<IActionResult> GetOpen()
        {
            var faults = await _db.Faults
                .Include(f => f.Room)
                .Include(f => f.Priority)
                .Include(f => f.Category)
                .Include(f => f.ReportedBy)
                .Where(f => !f.IsResolved)
                .OrderByDescending(f => f.PriorityId)
                .ThenByDescending(f => f.ReportedAt)
                .Select(f => new FaultDto
                {
                    Id              = f.Id,
                    RoomNumber      = f.Room != null ? f.Room.RoomNumber : 0,
                    Description     = f.Description,
                    Priority        = f.Priority != null ? f.Priority.Name : null,
                    Category        = f.Category != null ? f.Category.Name : null,
                    ReportedBy      = f.ReportedBy != null ? f.ReportedBy.FirstName + " " + f.ReportedBy.LastName : null,
                    ReportedAt      = f.ReportedAt,
                    IsResolved      = f.IsResolved,
                    ResolvedAt      = f.ResolvedAt,
                    ResolutionNotes = f.ResolutionNotes
                })
                .ToListAsync();

            return Ok(faults);
        }
    }

    public class FaultDto
    {
        public int      Id              { get; set; }
        public int      RoomNumber      { get; set; }
        public string   Description     { get; set; } = "";
        public string?  Priority        { get; set; }
        public string?  Category        { get; set; }
        public string?  ReportedBy      { get; set; }
        public DateTime ReportedAt      { get; set; }
        public bool     IsResolved      { get; set; }
        public DateTime? ResolvedAt     { get; set; }
        public string?  ResolutionNotes { get; set; }
    }
}
