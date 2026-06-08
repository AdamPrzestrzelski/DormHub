using DormHub.Models;
using Microsoft.EntityFrameworkCore;

namespace DormHub.Data
{
    public class DormDbContext : DbContext
    {
        public DormDbContext(DbContextOptions<DormDbContext> options) : base(options) { }

        public DbSet<BuildingModel>          Buildings          { get; set; }
        public DbSet<RoomModel>              Rooms              { get; set; }
        public DbSet<RoomTypeModel>          RoomTypes          { get; set; }
        public DbSet<RoomStatusModel>        RoomStatuses       { get; set; }
        public DbSet<ApplicationModel>       Applications       { get; set; }
        public DbSet<ApplicationStatusModel> ApplicationStatuses { get; set; }
        public DbSet<FaultModel>             Faults             { get; set; }
        public DbSet<FaultPriorityModel>     FaultPriorities    { get; set; }
        public DbSet<FaultCategoryModel>     FaultCategories    { get; set; }
        public DbSet<PersonModel>            Persons            { get; set; }
        public DbSet<ResidentModel>          Residents          { get; set; }
        public DbSet<PaymentModel>           Payments           { get; set; }
        public DbSet<PaymentStatusModel>     PaymentStatuses    { get; set; }
        public DbSet<AnnouncementModel>      Announcements      { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ResidentModel>()
                .HasOne(r => r.Person)
                .WithMany()
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<FaultModel>()
                .HasOne(f => f.ResolvedBy)
                .WithMany()
                .HasForeignKey(f => f.ResolvedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PaymentModel>()
                .HasOne(p => p.Resident)
                .WithMany()
                .HasForeignKey(p => p.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentModel>()
                .Property(p => p.Amount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<RoomTypeModel>()
                .Property(r => r.PricePerMonth)
                .HasPrecision(10, 2);

            modelBuilder.Entity<AnnouncementModel>()
                .HasOne(a => a.Author)
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnnouncementModel>()
                .HasOne(a => a.Building)
                .WithMany()
                .HasForeignKey(a => a.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RoomStatusModel>().HasData(
                new RoomStatusModel { Id = 1, Name = "Dostepny",   NameEn = "Available" },
                new RoomStatusModel { Id = 2, Name = "Zajety",     NameEn = "Occupied" },
                new RoomStatusModel { Id = 3, Name = "W remoncie", NameEn = "Under Maintenance" },
                new RoomStatusModel { Id = 4, Name = "Częściowo zajęty", NameEn = "Partially Occupied" }
            );

            modelBuilder.Entity<FaultPriorityModel>().HasData(
                new FaultPriorityModel { Id = 1, Name = "Niski",    NameEn = "Low" },
                new FaultPriorityModel { Id = 2, Name = "Sredni",   NameEn = "Medium" },
                new FaultPriorityModel { Id = 3, Name = "Wysoki",   NameEn = "High" },
                new FaultPriorityModel { Id = 4, Name = "Krytyczny",NameEn = "Critical" }
            );

            modelBuilder.Entity<FaultCategoryModel>().HasData(
                new FaultCategoryModel { Id = 1, Name = "Hydraulika",  NameEn = "Plumbing" },
                new FaultCategoryModel { Id = 2, Name = "Elektryka",   NameEn = "Electrical" },
                new FaultCategoryModel { Id = 3, Name = "Meble",       NameEn = "Furniture" },
                new FaultCategoryModel { Id = 4, Name = "Okna/Drzwi", NameEn = "Windows" },
                new FaultCategoryModel { Id = 5, Name = "Internet/TV", NameEn = "Internet" },
                new FaultCategoryModel { Id = 6, Name = "Inne",        NameEn = "Other" }
            );

            modelBuilder.Entity<ApplicationStatusModel>().HasData(
                new ApplicationStatusModel { Id = 1, Name = "Oczekujacy",    NameEn = "Pending" },
                new ApplicationStatusModel { Id = 2, Name = "Zaakceptowany", NameEn = "Accepted" },
                new ApplicationStatusModel { Id = 3, Name = "Odrzucony",     NameEn = "Rejected" }
            );

            modelBuilder.Entity<PaymentStatusModel>().HasData(
                new PaymentStatusModel { Id = 1, Name = "Oczekujaca", NameEn = "Pending" },
                new PaymentStatusModel { Id = 2, Name = "Zaplacona",  NameEn = "Paid" },
                new PaymentStatusModel { Id = 3, Name = "Zalegla",    NameEn = "Overdue" }
            );
        }
    }
}
