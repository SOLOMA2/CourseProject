using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.ViewsModels
{
    public class VerifyEmailVM
    {
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
