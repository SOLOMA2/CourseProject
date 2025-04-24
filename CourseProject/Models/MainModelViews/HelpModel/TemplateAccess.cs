using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class TemplateAccess
    {
        public int TemplateId { get; set; }
        public Template Template { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        [Required]
        public AccessPermission Permission { get; set; }
    }
}
