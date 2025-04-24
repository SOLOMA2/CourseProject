using CourseProject.Models.MainModelViews;
using CourseProject.Models.MainModelViews.HelpModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CourseProject.Models
{
    public class AppUser: IdentityUser
    {
        public DateTime LastLogin { get; set; }
        public List<Template> Templates { get; set; } = new();
        [ValidateNever]
        public List<Like> Likes { get; set; } = new();
        [ValidateNever]
        public List<Comment> Comments { get; set; } = new();


    }
}
