using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public List<TemplateTag> TemplateTags { get; set; } = new();

    }
}
