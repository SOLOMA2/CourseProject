using CourseProject.Models.MainModelViews;
using CourseProject.Models.MainModelViews.HelpModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CourseProject.Models
{
    public class AppUser: IdentityUser
    {
        public DateTime LastLogin { get; set; }
        public List<Template> Templates { get; set; } = new List<Template>();
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<FormResponse> Responses { get; set; } = new List<FormResponse>();


    }
}
