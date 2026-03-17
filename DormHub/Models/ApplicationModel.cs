using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; }
        public Person Applicant { get; set; }
        public BuildingModel PreferredBuilding { get; set; }
    }
}
