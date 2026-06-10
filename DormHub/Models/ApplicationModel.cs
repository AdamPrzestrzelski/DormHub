using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public int ApplicantId { get; set; }
        [ForeignKey("ApplicantId")]
        public PersonModel? Applicant { get; set; }

        [Required]
        public int TypeId { get; set; } = ApplicationTypes.Place;
        [ForeignKey("TypeId")]
        public ApplicationTypeModel? Type { get; set; }

        public int? PreferredRoomTypeId { get; set; }
        [ForeignKey("PreferredRoomTypeId")]
        public RoomTypeModel? PreferredRoomType { get; set; }

        public int? SecondRoomTypeId { get; set; }
        [ForeignKey("SecondRoomTypeId")]
        public RoomTypeModel? SecondRoomType { get; set; }

        public int? ThirdRoomTypeId { get; set; }
        [ForeignKey("ThirdRoomTypeId")]
        public RoomTypeModel? ThirdRoomType { get; set; }

        public int? PreferredBuildingId { get; set; }
        [ForeignKey("PreferredBuildingId")]
        public BuildingModel? PreferredBuilding { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PreferredStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PreferredEndDate { get; set; }

        [Required]
        public int StatusId { get; set; } = ApplicationStatuses.Pending;
        [ForeignKey("StatusId")]
        public ApplicationStatusModel? Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
