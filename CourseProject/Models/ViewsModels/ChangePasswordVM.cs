using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.ViewsModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm is required.")]
        [Compare("Password", ErrorMessage = "Password don't match(")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
    