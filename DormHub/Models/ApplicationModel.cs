using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("Applicant")]
        public PersonModel Applicant { get; set; }

        [ForeignKey("PreferredBuilding")]
        public BuildingModel PreferredBuilding { get; set; }
    }
}
