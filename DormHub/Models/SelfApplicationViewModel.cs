using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class SelfApplicationViewModel
    {
        public int TypeId { get; set; }

        [Display(Name = "Typ wniosku")]
        public string TypeName { get; set; } = string.Empty;

        public string? TypeNameEn { get; set; }

        [Display(Name = "1. wybór typu pokoju")]
        public int? PreferredRoomTypeId { get; set; }

        [Display(Name = "2. wybór typu pokoju")]
        public int? SecondRoomTypeId { get; set; }

        [Display(Name = "3. wybór typu pokoju")]
        public int? ThirdRoomTypeId { get; set; }

        [Display(Name = "Preferowany budynek")]
        public int? PreferredBuildingId { get; set; }

        [StringLength(1000, ErrorMessage = "Opis może mieć maksymalnie 1000 znaków")]
        [Display(Name = "Opis / uwagi")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data rozpoczęcia")]
        public DateTime? PreferredStartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data zakończenia")]
        public DateTime? PreferredEndDate { get; set; }

        // Flagi sterujące widokiem formularza
        public bool ShowSinglePreference { get; set; }
        public bool ShowRoomChoices { get; set; }
        public bool ShowBuilding { get; set; }
        public bool ShowPeriod { get; set; }
        public bool ShowCheckoutDate { get; set; }

        public string? CurrentRoomInfo { get; set; }
        public string? Hint { get; set; }
    }
}
