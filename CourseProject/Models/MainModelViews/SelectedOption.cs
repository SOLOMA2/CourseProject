using CourseProject.Models.MainModelViews;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class SelectedOption
{
    public int Id { get; set; }

    [Required]
    public int AnswerId { get; set; }

    [Required]
    public int QuestionOptionId { get; set; }

    [ForeignKey("AnswerId")]
    public Answer Answer { get; set; }

    [ForeignKey("QuestionOptionId")]
    public QuestionOption QuestionOption { get; set; }
}