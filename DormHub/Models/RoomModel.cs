using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class RoomModel
    {
        [Key]
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int BuildingId { get; set; }
        public int Capacity { get; set; }
        public bool isDeluxe { get; set; }

    }
}
