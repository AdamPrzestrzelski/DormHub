using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imie jest wymagane")]
        [Display(Name = "Imie")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data urodzenia jest wymagane")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data urodzenia")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Bledny adres email")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "Bledny numer telefonu")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Numer telefonu")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Haslo jest wymagane")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Haslo musi miec co najmniej 6 znakow")]
        [Display(Name = "Haslo")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdzenie hasla jest wymagane")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Hasla nie sa zgodne")]
        [Display(Name = "Potwierdz haslo")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}