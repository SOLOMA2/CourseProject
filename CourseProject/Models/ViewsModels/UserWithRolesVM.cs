using Microsoft.AspNetCore.Identity;

namespace CourseProject.Models.ViewsModels
{
    public class UserWithRolesVM
    {
        public AppUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
