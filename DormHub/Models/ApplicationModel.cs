using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("PersonModel")]
        public int ApplicantId { get; set; }

        [ForeignKey("RoomTypeModel")]
        public int PreferredRoomTypeId { get; set; }

        [ForeignKey("BuildingModel")]
        public int? PreferredBuildingId { get; set; }
    }
}
