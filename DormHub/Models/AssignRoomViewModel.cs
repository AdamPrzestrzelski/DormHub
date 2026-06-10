using System;
using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class AssignRoomViewModel
    {
        public string ApplicationId { get; set; } = string.Empty;

        public string ApplicantName { get; set; } = string.Empty;

        public int TypeId { get; set; }

        public string TypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wybierz pokój")]
        [Display(Name = "Pokój")]
        public int? RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data zameldowania")]
        public DateTime MoveInDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data wymeldowania")]
        public DateTime? MoveOutDate { get; set; }

        public string? CurrentRoomInfo { get; set; }
        public string? Description { get; set; }
    }
}
