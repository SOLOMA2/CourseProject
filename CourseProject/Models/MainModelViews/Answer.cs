using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProject.Models.MainModelViews
{
    public class Answer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserResponseId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string? Text { get; set; } // Nullable property

        [NotMapped]
        public List<int> SelectedOptionIds { get; set; } = new();
        public List<SelectedOption> SelectedOptions { get; set; } = new List<SelectedOption>();

        [ForeignKey("UserResponseId")]
        public FormResponse UserResponse { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; }
    }
}
