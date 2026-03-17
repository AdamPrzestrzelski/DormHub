using Microsoft.EntityFrameworkCore;

namespace DormHub.Data
{
    public class DormDbContext : DbContext
    {
        public DormDbContext(DbContextOptions<DormDbContext> options) : base(options)
        {
        }
        public DbSet<Models.BuildingModel> Buildings { get; set; }
        public DbSet<Models.RoomModel> Rooms { get; set; }
        public DbSet<Models.ApplicationModel> Applications { get; set; }
        public DbSet<Models.FaultModel> Faults { get; set; }
    {
    }
}
