using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CourseProject.Models.MainModelViews
{
    public class FormResponse
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public AppUser User { get; set; }


        [ValidateNever]
        [ForeignKey("TemplateId")]
        public Template Template { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
