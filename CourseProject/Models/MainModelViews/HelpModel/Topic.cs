using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class Topic
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<Template> Templates { get; set; } = new();
    }
}
