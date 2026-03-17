namespace DormHub.Models
{
    public class RoomModel
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int BuildingId { get; set; }
        public int Capacity { get; set; }
        public bool isPenthouse { get; set; }

    }
}
