using CourseProject.Models.MainModelViews;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class SelectedOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int AnswerId { get; set; }

    [Required]
    public int QuestionOptionId { get; set; }

    // Навигационные свойства
    [ForeignKey("AnswerId")]
    public Answer Answer { get; set; }

    [ForeignKey("QuestionOptionId")]
    public QuestionOption QuestionOption { get; set; }
}