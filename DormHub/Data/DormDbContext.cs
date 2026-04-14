using DormHub.Models;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Data
{
    public class DormDbContext : DbContext
    {
        public DormDbContext(DbContextOptions<DormDbContext> options) : base(options)
        {
        }
        public DbSet<BuildingModel> Buildings { get; set; }
        public DbSet<RoomModel> Rooms { get; set; }
        public DbSet<RoomTypeModel> RoomTypes { get; set; }
        public DbSet<ApplicationModel> Applications { get; set; }
        public DbSet<FaultModel> Faults { get; set; }
        public DbSet<PersonModel> Persons { get; set; }
        public DbSet<ResidentModel> Residents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ResidentModel>()
                .HasOne(r => r.Room)
                .WithMany(room => room.Residents)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FaultModel>()
                .HasOne(f => f.ReportedBy)
                .WithMany()
                .HasForeignKey(f => f.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
