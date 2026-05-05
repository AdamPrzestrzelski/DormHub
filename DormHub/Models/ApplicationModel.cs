using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
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
    }
}
