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

        public int? PreferredRoomTypeId { get; set; }
        [ForeignKey("PreferredRoomTypeId")]
        public RoomTypeModel? PreferredRoomType { get; set; }

        public int? PreferredBuildingId { get; set; }
        [ForeignKey("PreferredBuildingId")]
        public BuildingModel? PreferredBuilding { get; set; }

        [Required]
        public int StatusId { get; set; } = 1;
        [ForeignKey("StatusId")]
        public ApplicationStatusModel? Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
