using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models.MainModelViews.HelpModel
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TemplateId { get; set; }
        public Template Template { get; set; }

        public string AuthorId { get; set; }
        public AppUser Author { get; set; }
    }
}
