using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class TemplateAccess
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template Template { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }

    }
}
