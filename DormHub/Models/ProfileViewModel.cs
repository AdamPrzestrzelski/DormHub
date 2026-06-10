namespace DormHub.Models
{
    public class ProfileViewModel
    {
        public PersonModel Person { get; set; } = null!;
        public ChangeEmailViewModel ChangeEmail { get; set; } = new();
        public ChangePasswordViewModel ChangePassword { get; set; } = new();
    }
}
