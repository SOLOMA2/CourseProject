using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProject.Models.MainModelViews
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public QuestionType Type { get; set; }
        public int Order { get; set; }
        public bool ShowInTable { get; set; }
        public bool IsRequired { get; set; }

        public int? MaxLength { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }

        public int TemplateId { get; set; }
        [ValidateNever]
        public string? TextType { get; set; }
        [ValidateNever] 
        [ForeignKey("TemplateId")]
        public Template Template { get; set; }
        public List<QuestionOption> Options { get; set; } = new();
        public List<Answer> Answers { get; set; } = new List<Answer>();


    }
}
