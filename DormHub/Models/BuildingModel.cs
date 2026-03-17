namespace DormHub.Models
{
    public class BuildingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<> FreeRooms { get; set; }
        public List<> OccupiedRooms { get; set; }
    }
}
