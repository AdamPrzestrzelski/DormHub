using System;
using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imiê jest wymagane")]
        [Display(Name = "Imiê")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagane")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data urodzenia")]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "B³êdny adres email")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "B³êdny numer telefonu")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Numer telefonu")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Has³o jest wymagane")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Has³o musi mieæ co najmniej 6 znaków")]
        [Display(Name = "Has³o")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potwierdzenie has³a jest wymagane")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Has³a nie s¹ zgodne")]
        [Display(Name = "PotwierdŸ has³o")]
        public string ConfirmPassword { get; set; }
    }
}