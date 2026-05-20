using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public enum ApplicationStatus
    {
        Pending,   // Oczekujący
        Accepted,  // Zaakceptowany
        Rejected   // Odrzucony
    }

    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public int ApplicantId { get; set; }
        [ForeignKey("ApplicantId")]
        public PersonModel? Applicant { get; set; }

        [Required]
        public int PreferredRoomTypeId { get; set; }
        [ForeignKey("PreferredRoomTypeId")]
        public RoomTypeModel? PreferredRoomType { get; set; }

        public int? PreferredBuildingId { get; set; }
        [ForeignKey("PreferredBuildingId")]
        public BuildingModel? PreferredBuilding { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [DataType(DataType.DateTime)]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? AdminNotes { get; set; }
    }
}
