using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class FaultModel
    {
        [Key]
        public int Id { get; set; }
        public RoomModel Room { get; set; }
        public PersonModel ReportedBy { get; set; }
        public string Description { get; set; }
        public DateTime ReportedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
