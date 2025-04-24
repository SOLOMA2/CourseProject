using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.ViewsModels
{
    public class RegistrationVM
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm is required.")]
        [Compare("Password", ErrorMessage = "Password don't match(")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
