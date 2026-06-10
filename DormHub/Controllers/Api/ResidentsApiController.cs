using DormHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Controllers.Api
{
    [ApiController]
    [Route("api/residents")]
    [Produces("application/json")]
    public class ResidentsApiController : ControllerBase
    {
        private readonly DormDbContext _db;
        public ResidentsApiController(DormDbContext db) => _db = db;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ResidentDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var residents = await _db.Residents
                .Include(r => r.Room)
                .Include(r => r.Person)
                .Select(r => new ResidentDto
                {
                    Id          = r.Id,
                    FirstName   = r.Person != null ? r.Person.FirstName : "",
                    LastName    = r.Person != null ? r.Person.LastName : "",
                    Email       = r.Person != null ? r.Person.Email : "",
                    RoomNumber  = r.Room != null ? r.Room.RoomNumber : (int?)null,
                    MoveInDate  = r.MoveInDate,
                    MoveOutDate = r.MoveOutDate
                })
                .ToListAsync();

            return Ok(residents);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResidentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _db.Residents
                .Include(r => r.Room)
                .Include(r => r.Person)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return NotFound(new { message = $"Mieszkaniec o ID {id} nie istnieje." });

            return Ok(new ResidentDto
            {
                Id          = r.Id,
                FirstName   = r.Person?.FirstName ?? "",
                LastName    = r.Person?.LastName ?? "",
                Email       = r.Person?.Email ?? "",
                RoomNumber  = r.Room?.RoomNumber,
                MoveInDate  = r.MoveInDate,
                MoveOutDate = r.MoveOutDate
            });
        }
    }

    public class ResidentDto
    {
        public int       Id          { get; set; }
        public string    FirstName   { get; set; } = "";
        public string    LastName    { get; set; } = "";
        public string    Email       { get; set; } = "";
        public int?      RoomNumber  { get; set; }
        public DateTime  MoveInDate  { get; set; }
        public DateTime? MoveOutDate { get; set; }
    }
}
