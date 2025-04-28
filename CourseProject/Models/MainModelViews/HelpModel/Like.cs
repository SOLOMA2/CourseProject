using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class Like
    {
        public int Id { get; set; } 

        public int TemplateId { get; set; }
        [ValidateNever]

        public Template Template { get; set; }

        public string UserId { get; set; } 
        [ValidateNever]

        public AppUser User { get; set; }

        public DateTime LikedAt { get; set; }
    }
}
