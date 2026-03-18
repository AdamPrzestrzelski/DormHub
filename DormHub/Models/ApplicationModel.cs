using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ApplicationModel
    {
        [Key]
        public string Id { get; set; }

        [ForeignKey("Person")]
        public PersonModel Applicant { get; set; }

        [ForeignKey("Building")]
        public BuildingModel PreferredBuilding { get; set; }
    }
}
